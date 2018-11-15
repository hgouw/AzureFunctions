using Microsoft.Azure;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.IO;
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
    //    a. Copy the zip file to blob container
    //    b. Unzip the zip file to blob container
    //    c. Load the txt files to database
    //    d. Update the last download zip file info
    //    e. Remove the zip file and txt files
    //    f. Send message upon when loading is completed
    // 5. If the latest zip file is not available then send message

    public static class LoadHistoricalData
    {
        [FunctionName("LoadHistoricalData")]
        // "*/5 * * * * *" - every 5 seconds
        // "0 */1 * * * *" - every minute
        public static void Run([TimerTrigger("*/5 * * * * *")]TimerInfo myTimer, TraceWriter log)
        {
            var url = CloudConfigurationManager.GetSetting("HistorialDataUrl");
            var filename = CloudConfigurationManager.GetSetting("HistoricalDataFilename");
            if (CheckUrl(url + "\\" + filename))
            {
                CheckBlobContainer();
                var destination = Path.GetTempPath() + filename;
                try
                {
                    using (var client = new WebClient())
                    {
                        log.Info($"Downloading the file to {destination}");
                        //client.DownloadFile(url, destination);
                    }
                }
                catch (Exception ex)
                {
                    log.Info($"Error in downloading the file to {destination} - {ex.Message}");
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
            request.Timeout = 1000; // 1000 milliseconds (1 second)

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
        private static void CheckBlobContainer()
        {
            var storageAccount = CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("AzureWebJobsStorage"));
            var blobClient = storageAccount.CreateCloudBlobClient();

            var blobContainer = blobClient.GetContainerReference(CloudConfigurationManager.GetSetting("ContainerName"));
            blobContainer.CreateIfNotExists();
        }
    }
}