using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;

namespace FunctionSender
{
    public class Function1
    {

        public Function1()
        {

        }


        [FunctionName("Function1")]
        public void Run([TimerTrigger("0 */5 * * * *")]TimerInfo myTimer, ILogger log)
        {
            try
            {
                log.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");
            }
            catch (Exception ex)
            {
                log.LogInformation(ex.Message);
            }
        }
    }
}
