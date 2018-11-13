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
    // 1. Download the zip file
    // 2. Unzip the zip file into txt 
    // 3. Load the txt files into database
    // 4. Remove the zip file and txt files

    public static class LoadHistoricalData
    {
        [FunctionName("LoadHistoricalData")]
        // "*/5 * * * * *" - every 5 seconds
        // "0 */1 * * * *" - every minute
        public static void Run([TimerTrigger("*/5 * * * * *")]TimerInfo myTimer, TraceWriter log)
        {
            var filename = "week20181102.zip";
            var url = "https://www.asxhistoricaldata.com/data";
            var ok = CheckUrl(url + "\\" + filename);

            try
            {
                using (var client = new WebClient())
                {
                    var destination = Path.GetTempPath() + filename;
                    log.Info($"Downloading the file to {destination}");
                    //client.DownloadFile(url, destination);
                }
            }
            catch (Exception ex)
            {
                log.Info($"Download Fail {ex.Message}");
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
            request.Timeout = 15000;

            try
            {
                var response = request.GetResponse();
            }
            catch
            {
                return false;
            }

            return true;
        }

        /*
        private static string GenerateId()
        {
            long i = 1;
            foreach (byte b in Guid.NewGuid().ToByteArray())
            {
                i *= ((int)b + 1);
            }
            return string.Format("{0:x}", i - DateTime.Now.Ticks);
        }

        //RemoveFilesAsync().Wait();
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
            var storageAccount = CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("AzureWebJobsStorage"));
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
        */
        /*
        public static void Run([BlobTrigger("samples-workitems/{name}", Connection = "AzureWebJobsStorage")]string myBlob, string name, [Blob("test2/{name}.csv", FileAccess.Write, Connection = "AzureWebJobsStorage")]out string outputBlob, TraceWriter log)
        {
            var outputstring = jsonToCSV(myBlob, ","); // add your logic to covert json to CSV

            log.Info($"C# Blob trigger function Processed blob\n Name:{name} \n Size: {myBlob.Length} Bytes");
            outputBlob = outputstring;
        }

        public static void Run([BlobTrigger("test/{name}", Connection = "AzureWebJobsStorage")]Stream myBlob, string name, [Blob("test2/{name}.csv", FileAccess.Write, Connection = "AzureWebJobsStorage")] Stream outputBlob, TraceWriter log)
        {
            myBlob.Position = 0;
            var str = StreamToString(myBlob);
            var outputstring = jsonToCSV(str, ",");// add your logic to covert json to CSV
            var stream = StringtoStream(outputstring);
            stream.Position = 0;
            stream.CopyTo(outputBlob);
            log.Info($"C# Blob trigger function Processed blob\n Name:{name} \n Size: {myBlob.Length} Bytes");
        }

        public static DataTable jsonStringToTable(string jsonContent)
        {
            DataTable dt = JsonConvert.DeserializeObject<DataTable>(jsonContent);
            return dt;
        }

        public static string jsonToCSV(string jsonContent, string delimiter)
        {
            StringWriter csvString = new StringWriter();
            using (var csv = new CsvWriter(csvString))
            {

                csv.Configuration.Delimiter = delimiter;

                using (var dt = jsonStringToTable(jsonContent))
                {

                    foreach (DataColumn column in dt.Columns)
                    {
                        csv.WriteField(column.ColumnName);
                    }
                    csv.NextRecord();

                    foreach (DataRow row in dt.Rows)
                    {
                        for (var i = 0; i < dt.Columns.Count; i++)
                        {
                            csv.WriteField(row[i]);
                        }
                        csv.NextRecord();
                    }
                }
            }

            return csvString.ToString();
        }
        */
    }
}