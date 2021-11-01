using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;

namespace AirdropFunctions
{
    public static class AlchemonAirdrop
    {
        [FunctionName("AlchemonAirdrop")]
        public static void Run([TimerTrigger("0 0 12 * * Sat")]TimerInfo myTimer, ILogger log)
        {
            log.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");
        }
    }
}
