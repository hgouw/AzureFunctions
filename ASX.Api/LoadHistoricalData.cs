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
    public static class LoadHistoricalData
    {
        [FunctionName("LoadHistoricalData")]
        // "*/5 * * * * *" - every 5 seconds
        // "0 */1 * * * *" - every minute
        public static void Run([TimerTrigger("*/5 * * * * *")]TimerInfo myTimer, TraceWriter log)
        {
            //var url = "/assets/data/week20180615.zip";
            var url = "https://www.asxhistoricaldata.com/data/week20181102.zip";
            var ok = CheckUrl(url);

            //RemoveFilesAsync().Wait();

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
            request.Timeout = 15000;

            try
            {
                var response = request.GetResponse();
                //using (WebResponse response = request.GetResponse())
                //{
                //    using (Stream stream = response.GetResponseStream())
                //    {
                //        var doc = XDocument.Load(stream);
                //    }
                //}
            }
            catch (Exception ex)
            {
                return false;
            }

            return true;
        }

        private static async Task CopyFilesAsync()
        {
            var storageAccount = CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("AzureWebJobsStorage"));
            var blobClient = storageAccount.CreateCloudBlobClient();

            var blobContainer = blobClient.GetContainerReference(CloudConfigurationManager.GetSetting("ContainerName"));
            await blobContainer.CreateIfNotExistsAsync();

            var files = Directory.GetFiles(CloudConfigurationManager.GetSetting("DataFolder"));
            Array.Sort(files, StringComparer.InvariantCulture);
            foreach (var file in files)
            {
                var blockBlob = blobContainer.GetBlockBlobReference(Path.GetFileName(file));
                await blockBlob.UploadFromFileAsync(file); //FileMode.Open);
            }
        }

        private static async Task RemoveFilesAsync()
        {
            var storageAccount = CloudStorageAccount.Parse("DefaultEndpointsProtocol=https;AccountName=hgouw;AccountKey=lXr8WD/7ULcm9AIrEW9s0q2jdQaVYcTB9rSaaQZJ51m3VxZya+t9yfEpavg5OFuni/A6kYx9bLCdimNRq7roHg==");
            var client = storageAccount.CreateCloudBlobClient();
            var container = client.GetContainerReference("optus");
            var blobs = container.ListBlobs();
            foreach (var blob in blobs)
            {
                if (blob is CloudBlockBlob)
                {
                    var block = container.GetBlockBlobReference(((CloudBlockBlob)blob).Name);
                    await block.DeleteAsync();
                }
            }
        }
    }
}