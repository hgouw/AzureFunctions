using Microsoft.Azure;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
//using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Threading.Tasks;
using System.Xml.Linq;

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
        public static void Run([TimerTrigger("0 */5 * * * *")]TimerInfo myTimer, TraceWriter log)
        {
            var filename = CloudConfigurationManager.GetSetting("HistoricalDataFilename");
            var source = CloudConfigurationManager.GetSetting("HistorialDataUrl") + "\\" + filename;
            if (CheckUrl(source))
            {
                try
                {
                    CheckBlobContainer(source, filename);
                }
                catch (Exception ex)
                {
                    log.Info($"Error in downloading {source} - {ex.Message}");
                }
            }
            else
            {
                log.Info($"Unable to locate the file {filename}");
            }

            if (myTimer.IsPastDue)
            {
                log.Info("Timer is running late!");
            }
            log.Info($"C# Timer trigger function executed at: {DateTime.Now}");
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
        private static void CheckBlobContainer(string url, string filename)
        {
            var storageAccount = CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("AzureWebJobsStorage"));
            var blobClient = storageAccount.CreateCloudBlobClient();

            var blobContainer = blobClient.GetContainerReference(CloudConfigurationManager.GetSetting("ContainerName"));
            blobContainer.CreateIfNotExists();
            var blockBlob1 = blobContainer.GetBlockBlobReference(filename);
            var blockBlob2 = blobContainer.GetBlockBlobReference(CloudConfigurationManager.GetSetting("FileName"));
            blockBlob1.StartCopy(new Uri(url), null, null, null);
            blockBlob2.UploadText(filename);
            var text = blockBlob2.DownloadText();
        }

        // Return the Friday of the current week
        private static DateTime CurrentFriday()
        {
            var today = DateTime.Today;
            return today.AddDays(-(int)today.DayOfWeek).AddDays(5);
        }
    }
}