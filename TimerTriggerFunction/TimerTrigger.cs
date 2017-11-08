using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;

namespace TimerTriggerFunction
{
    public static class TimerTrigger
    {
        [FunctionName("TimerTrigger")]
        // Set the timer to be triggered daily at 12am
        public static void Run([TimerTrigger("0 0 0 */1 * *")]TimerInfo myTimer, TraceWriter log)
        {
            log.Info($"C# Timer trigger function executed at: {DateTime.Now}");
        }
    }
}