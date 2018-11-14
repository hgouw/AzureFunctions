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
    }
}