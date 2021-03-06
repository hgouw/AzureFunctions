using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;

namespace ASX.Api
{
    public static class EndOfDays
    {
        [FunctionName("EndOfDays")]
        public static async Task<HttpResponseMessage> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)]
            // HttpTrigger parameters
            // 1. How the Http function is going to be authorised
            // AuthorizationLevel.Anonymous = No key required
            // AuthorizationLevel.Function = Individual function key
            // AuthorizationLevel.Admin = Function app key
            // 2. Supported Http methods for the function
            // "get"
            // "post"
            // "put"
            // "patch"
            // 3. (Optional) Function route template
            HttpRequestMessage req, 
            TraceWriter log)
        {
            log.Info("C# HTTP trigger function processed a request.");

            var param = await req.Content.ReadAsAsync<Param>();

            log.Info($"EndOfDays request received for {param.Company}");

            return req.CreateResponse(HttpStatusCode.OK, $"Returned EndOfDays for {param.Company}");
        }
    }
}