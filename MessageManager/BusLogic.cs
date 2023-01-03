using Azure.Messaging.ServiceBus;
using DataAccess;
using Entidades.Domain.DTO;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageManager
{
    public interface IBusLogic
    {
        /// <summary>
        /// metodo encargado de obtener la instancia de Services Bus Client de los services Worker 
        /// </summary>
        /// <param name="instanceFromWorkerService"></param>
        void SetInstance(ServiceBusClient instanceFromWorkerService);


        /// <summary>
        /// 
        /// </summary>
        /// <param name="messageToServiceBus">El Vuelo nuevo registrado en la base de datos que tiene que ser enviado al Service Bus</param>
        /// <returns></returns>
        Task<bool> SendMessageAsync(object messageToServiceBus);

        /// <summary>
        /// Metodo encargado de obtener los registros de la tabla y enviarlos a azure service bus
        /// </summary>
        /// <returns></returns>
        Task<bool> GetNewRecords(string connectionString);

        /// <summary>
        /// metodo encargado de obtener los mensajes que se encuentran activos en la cola de AzureServiceBus
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<List<bool>> GetQueue(CancellationToken cancellationToken, string connectionString);
    }
    public class BusLogic : IBusLogic
    {
        private ServiceBusClient _serviceBusClient;


        public void SetInstance(ServiceBusClient instanceFromWorkerService)
        {
            if (this._serviceBusClient is null)
            {
                this._serviceBusClient = instanceFromWorkerService;
            }
        }

        public async Task<bool> SendMessageAsync(object messageToServiceBus)
        {
            if (this._serviceBusClient is not null)
            {
                ServiceBusSender sender = this._serviceBusClient.CreateSender("notificaciones");//queue name
                var body = System.Text.Json.JsonSerializer.Serialize(messageToServiceBus);
                var serviceBusMessage = new ServiceBusMessage(body);
                await sender.SendMessageAsync(serviceBusMessage);
                return true;
            }
            return false;
        }

        public async Task<bool> GetNewRecords(string connectionString)
        {
            AzureDAO dao = new AzureDAO(connectionString);
            List<VueloDTO> lista = dao.GetNewRecordsFromDatabase().Result;

            if (lista is not null && lista.Count > 0)
            {
                foreach (var item in lista)
                {
                    //this._logger.LogInformation($"VUELO NUMERO #{item.NumeroVuelo} REGISTRADO EN EL DIA {DateTime.Now.ToShortDateString()}");
                    await SendMessageAsync(item);
                }
                return true;
            }
            else
            {
                return false;
            }
        }

        public async Task<List<bool>> GetQueue(CancellationToken cancellationToken, string connectionString)
        {
            List<bool> lista = new List<bool>();
            ServiceBusReceiver receiver = this._serviceBusClient.CreateReceiver("notificaciones");
            ServiceBusReceivedMessage message;
            AzureDAO dao = new AzureDAO(connectionString);
            while ((message = await receiver.ReceiveMessageAsync(TimeSpan.FromMilliseconds(1000), cancellationToken)) is not null)
            {
                var jsonString = message.Body.ToString();
                VueloDTO messageToModel = JsonConvert.DeserializeObject<VueloDTO>(jsonString)!;
                if (await dao.SetRecordsFromServiceBus(messageToModel))
                {
                    await receiver.CompleteMessageAsync(message);
                    lista.Add(true);
                }
            }
            return lista;

        }
    }
}
