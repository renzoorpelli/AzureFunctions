using System;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using MessageManager;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;

namespace SenderRecordsServiceBus
{
    public class EnqueueMessage
    {
        private readonly IBusLogic _busLogic;
        private readonly ServiceBusClient serviceBusClient;

        public EnqueueMessage(IBusLogic busLogic, ServiceBusClient serviceBusClient)
        {
            this._busLogic = busLogic;
            this.serviceBusClient = serviceBusClient;
            busLogic.SetInstance(serviceBusClient);
        }

        [FunctionName("EnqueueMessage")]
        public async Task Run([TimerTrigger("0 */1 * * * *")]TimerInfo myTimer, ILogger log)
        {
            try
            {
                string cadenaConexion = String.Empty;
                if(Environment.GetEnvironmentVariable("databaseConnection", EnvironmentVariableTarget.Process) is not null)
                {
                    cadenaConexion = Environment.GetEnvironmentVariable("databaseConnection", EnvironmentVariableTarget.Process);
                }
                else
                {
                    cadenaConexion = System.Configuration.ConfigurationManager.ConnectionStrings["sql:databaseConnection"].ConnectionString;
                }

                bool respuesta = await this._busLogic.GetNewRecords(cadenaConexion);
                if (!respuesta)
                {
                    log.LogInformation("SIN VUELOS PENDIENTES A REGISTRO");
                }
                else
                {
                    log.LogInformation("VUELO REGISTRADO");
                }
            }catch(Exception ex)
            {
                log.LogInformation(ex.Message);
            }
        }
    }
}
