using Microsoft.Azure;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;

namespace ASX.Api
{
    public static class UnzipHistoricalData
    {
        [FunctionName("UnzipHistoricalData")]
        public static async Task Run([BlobTrigger("asx/{name}", Connection = "AzureWebJobsStorage")]Stream myBlob, string name, TraceWriter log)
        {
            log.Info($"C# Blob trigger function executed at {DateTime.Now} to process blob {name} of {myBlob.Length} bytes");

            try
            {
                if (name.Split('.').Last().ToLower() == "zip")
                {
                    var storageAccount = CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("AzureWebJobsStorage"));
                    var blobClient = storageAccount.CreateCloudBlobClient();
                    var blobContainer = blobClient.GetContainerReference(CloudConfigurationManager.GetSetting("ContainerName"));
                    using (var memoryStream = new MemoryStream())
                    {
                        await myBlob.CopyToAsync(memoryStream);
                        using (var archive = new ZipArchive(memoryStream))
                        {
                            foreach (var entry in archive.Entries)
                            {
                                log.Info($"Unzipping {entry.FullName}");
                                using (var fileStream = entry.Open())
                                {
                                    //await myBlob.WriteAsync(fileStream);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                log.Info($"Unable to unzip the file {name} at {DateTime.Now}");
            }
        }
    }
}