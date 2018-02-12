using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;

namespace ASX.Api
{
    public static class EndOfDaysApi
    {
        [FunctionName("EndOfDaysApi")]
        public static async Task<HttpResponseMessage> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)]
            HttpRequestMessage req, 
            TraceWriter log)
        {
            log.Info("C# HTTP trigger function processed a request.");

            EndOfDay endOfDay = await req.Content.ReadAsAsync<EndOfDay>();

            log.Info($"EndOfDays request received for {endOfDay.Code}");

            return req.CreateResponse(HttpStatusCode.OK, $"Returned EndOfDays for {endOfDay.Code}");
        }
    }
}
