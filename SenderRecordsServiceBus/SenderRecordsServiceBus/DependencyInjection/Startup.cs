using Azure.Messaging.ServiceBus;
using MessageManager;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Xml.Linq;


[assembly: FunctionsStartup(typeof(SenderRecordsServiceBus.DependencyInjection.Startup))]

namespace SenderRecordsServiceBus.DependencyInjection
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            var connectionString = Environment.GetEnvironmentVariable("AzureSB", EnvironmentVariableTarget.Process);
            
            builder.Services.AddSingleton<IBusLogic, BusLogic>();

            builder.Services.AddSingleton(service =>
            {
                return new ServiceBusClient(connectionString, new ServiceBusClientOptions()
                {
                    TransportType = ServiceBusTransportType.AmqpWebSockets
                });
            });

            builder.Services.AddApplicationInsightsTelemetry();
        }
    }
}
