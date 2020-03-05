using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ASX.Api
{
    public static class StoreHistoricalData
    {
        [FunctionName("StoreHistoricalData")]
        public static async Task Run([BlobTrigger("asx-text/{name}", Connection = "AzureWebJobsStorage")]Stream myBlob, string name, TraceWriter log)
        {
            log.Info($"C# Blob trigger function executed at {DateTime.Now} to process blob {name} of {myBlob.Length} bytes");

            try
            {
                if (name.Split('.').Last().ToLower() == "txt")
                {
                    log.Info($"Storing file {name} at {DateTime.Now}");
                    /*
                    var storageAccount = CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("AzureWebJobsStorage"));
                    var blobClient = storageAccount.CreateCloudBlobClient();
                    var blobContainer = blobClient.GetContainerReference(CloudConfigurationManager.GetSetting("TextContainerName"));
                    */
                    await Task.Delay(1);
                }
            }
            catch (Exception ex)
            {
                log.Info($"Unable to store the file {name} at {DateTime.Now} - {ex.Message}");
            }
        }
    }
}