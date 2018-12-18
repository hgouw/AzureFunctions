using Microsoft.Azure;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.WindowsAzure.Storage;
using System;
using System.Globalization;
using System.Net;

namespace ASX.Api
{
    // TO DO
    // 1. Get the last downloaded zip file info
    // 2. Calculate if there is the latest zip file
    // 3. Check if the latest zip file is available
    // 4. If the latest zip file is available
    //    a. Copy the zip file to blob container (asx-zip)
    //    b. Unzip the zip file to blob container (asx-txt)
    //    c. Load the txt files to database
    //    d. Update the last download zip file info (asx-last)
    //    e. Remove the zip file and txt files
    //    f. Send message upon when loading is completed
    // 5. If the latest zip file is not available then send message
    // Note: log files 

    public static class LoadHistoricalData
    {
        [FunctionName("LoadHistoricalData")]
        // "*/5 * * * * *" - every 5 seconds
        // "0 */1 * * * *" - every minute
        // "0 */5 * * * *" - every 5 minutes
        public static void Run([TimerTrigger("0 */1 * * * *")]TimerInfo myTimer, TraceWriter log)
        {
            if (myTimer.IsPastDue)
            {
                log.Info("Timer is running late!");
            }

            var last = DateTime.ParseExact(CheckBlobContainer(log), "yyyyMMdd", CultureInfo.InvariantCulture);
            while (last < CurrentFriday())
            {
                last = last.AddDays(7);
                var filename = last.ToString("weekyyyyMMdd") + ".zip";
                var url = CloudConfigurationManager.GetSetting("HistorialDataUrl") + "/" + filename;
                if (CheckUrl(url))
                {
                    if (ProcessBlobContainer(filename, url, log))
                    {
                        UpdateBlobContainer(last.ToString("yyyyMMdd"), log);
                    }
                }
                else
                {
                    log.Info($"Unable to locate the file {url} at {DateTime.Now}");
                }
            }

            log.Info($"C# Timer trigger function executed at {DateTime.Now}");
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
            var text = "";

            try
            {
                var storageAccount = CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("AzureWebJobsStorage"));
                var blobClient = storageAccount.CreateCloudBlobClient();
                var blobContainer = blobClient.GetContainerReference(CloudConfigurationManager.GetSetting("ContainerName"));
                var lastBlockBlob = blobContainer.GetBlockBlobReference(CloudConfigurationManager.GetSetting("BlockBlob"));
                text = lastBlockBlob.DownloadText();
            }
            catch (Exception ex)
            {
                log.Info($"Error in CheckBlobContainer - {ex.Message}");
            }

            return text;
        }

        private static bool ProcessBlobContainer(string filename, string url, TraceWriter log)
        {
            try
            {
                var storageAccount = CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("AzureWebJobsStorage"));
                var blobClient = storageAccount.CreateCloudBlobClient();
                var blobContainer = blobClient.GetContainerReference(CloudConfigurationManager.GetSetting("ContainerName"));
                blobContainer.CreateIfNotExists();
                var zipBlockBlob = blobContainer.GetBlockBlobReference(filename);
                zipBlockBlob.StartCopy(new Uri(url), null, null, null);
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
                var blobContainer = blobClient.GetContainerReference(CloudConfigurationManager.GetSetting("ContainerName"));
                blobContainer.CreateIfNotExists();
                var lastBlockBlob = blobContainer.GetBlockBlobReference(CloudConfigurationManager.GetSetting("BlockBlob"));
                lastBlockBlob.UploadText(text);
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