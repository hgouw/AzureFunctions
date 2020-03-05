using Microsoft.Azure;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.WindowsAzure.Storage;
using System;
using System.Globalization;
using System.Net;

namespace ASX.Api
{
    // 1. Get the last downloaded zip file info
    // 2. Calculate if there is the latest zip file
    // 3. Check if the latest zip file is available
    // 4. If the latest zip file is available then 
    //    a. Copy the latest zip file to blob container (asx-zip)
    //    b. Update the last download zip file info (asx-last)
    public static class LoadHistoricalData
    {
        [FunctionName("LoadHistoricalData")]
        // https://en.wikipedia.org/wiki/Cron
        // https://codehollow.com/2017/02/azure-functions-time-trigger-cron-cheat-sheet/
        // http://www.openjs.com/scripts/jslibrary/demos/crontab.php
        // "0 0  6  * * *" - every day at 6AM
        // "0 30 6  * * *" - every day at 6.30AM
        // "0 0 18  * * *" - every day at 6PM
        // "0 0 */6 * * *" - every 6 hours
        // "0 0 * * * *"   - every hour
        // "0 */5 * * * *" - every 5 minutes
        // "0 */1 * * * *" - every minute
        // "*/5 * * * * *" - every 5 seconds
        public static void Run([TimerTrigger("0 0 18  * * *")]TimerInfo myTimer, TraceWriter log)
        {
            log.Info($"C# Timer trigger function executed at {DateTime.Now}");
            if (myTimer.IsPastDue)
            {
                log.Info("Timer is running late!");
            }

            var dateFormat = CloudConfigurationManager.GetSetting("DateFormat");
            var last = DateTime.ParseExact(CheckBlobContainer(log), dateFormat, CultureInfo.InvariantCulture);
            while (last < CurrentFriday())
            {
                last = last.AddDays(7);
                var filename = "week" + last.ToString(dateFormat) + ".zip";
                var url = CloudConfigurationManager.GetSetting("HistoricalDataUrl") + "/" + filename;
                if (CheckUrl(url))
                {
                    if (ProcessBlobContainer(filename, url, log))
                    {
                        UpdateBlobContainer(last.ToString(dateFormat), log);
                        log.Info($"Successfully processed the file {url} at {DateTime.Now}");
                    }
                }
                else
                {
                    log.Info($"Unable to locate the file {url} at {DateTime.Now}");
                }
            }
        }

        private static bool CheckUrl(string url)
        {
            var urlCheck = new Uri(url);
            var request = WebRequest.Create(urlCheck);
            request.Timeout = 1000; // 1000 milliseconds = 1 second

            try
            {
                request.GetResponse();
            }
            catch
            {
                return false;
            }

            return true;
        }
        private static string CheckBlobContainer(TraceWriter log)
        {
            var last = "";

            try
            {
                var storageAccount = CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("AzureWebJobsStorage"));
                var blobClient = storageAccount.CreateCloudBlobClient();
                var blobContainer = blobClient.GetContainerReference(CloudConfigurationManager.GetSetting("LastContainerName"));
                blobContainer.CreateIfNotExists();
                var blockBlob = blobContainer.GetBlockBlobReference(CloudConfigurationManager.GetSetting("LastBlockBlob"));
                last = blockBlob.DownloadText();
            }
            catch (Exception ex)
            {
                log.Info($"Error in CheckBlobContainer - {ex.Message}");
            }

            return last;
        }

        private static bool ProcessBlobContainer(string filename, string url, TraceWriter log)
        {
            try
            {
                var storageAccount = CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("AzureWebJobsStorage"));
                var blobClient = storageAccount.CreateCloudBlobClient();
                var blobContainer = blobClient.GetContainerReference(CloudConfigurationManager.GetSetting("ZipContainerName"));
                blobContainer.CreateIfNotExists();
                var blockBlob = blobContainer.GetBlockBlobReference(filename);
                blockBlob.StartCopy(new Uri(url), null, null, null);
            }
            catch (Exception ex)
            {
                log.Info($"Error in ProcessBlobContainer - {ex.Message}");
                return false;
            }

            return true;
        }

        private static void UpdateBlobContainer(string text, TraceWriter log)
        {
            try
            {
                var storageAccount = CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("AzureWebJobsStorage"));
                var blobClient = storageAccount.CreateCloudBlobClient();
                var blobContainer = blobClient.GetContainerReference(CloudConfigurationManager.GetSetting("LastContainerName"));
                blobContainer.CreateIfNotExists();
                var blockBlob = blobContainer.GetBlockBlobReference(CloudConfigurationManager.GetSetting("LastBlockBlob"));
                blockBlob.UploadText(text);
            }
            catch (Exception ex)
            {
                log.Info($"Error in UpdateBlobContainer - {ex.Message}");
            }
        }

        // Return the Friday of the current week
        private static DateTime CurrentFriday()
        {
            var today = DateTime.Today;
            return today.AddDays(-(int)today.DayOfWeek).AddDays(5);
        }
    }
}