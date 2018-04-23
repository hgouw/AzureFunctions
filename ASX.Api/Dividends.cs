using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;

namespace ASX.Api
{
    public static class Dividends
    {
        [FunctionName("Dividends")]
        public static async Task<HttpResponseMessage> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)]
            HttpRequestMessage req, 
            TraceWriter log)
        {
            log.Info("C# HTTP trigger function processed a request.");

            Company endOfDay = await req.Content.ReadAsAsync<Company>();

            log.Info($"Dividends request received for {endOfDay.Code}");

            return req.CreateResponse(HttpStatusCode.OK, $"Returned Dividends for {endOfDay.Code}");
        }
    }
}
