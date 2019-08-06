using System;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using NCS.DSS.LearningProgression.Models;
using Microsoft.Azure.ServiceBus;

namespace NCS.DSS.LearningProgression.ServiceBus
{
    public class ServiceBusClient : IServiceBusClient
    {
        private readonly LearningProgressionConfigurationSettings _learningProgressionConfigurationSettings;

        public ServiceBusClient(LearningProgressionConfigurationSettings learnerProgressConfigurationSettings)
        {
            _learningProgressionConfigurationSettings = learnerProgressConfigurationSettings;
        }

        public async Task SendPostMessageAsync(Models.LearningProgression learningProgression, string reqUrl)
        {
            var queueClient = new QueueClient(_learningProgressionConfigurationSettings.ServiceBusConnectionString, _learningProgressionConfigurationSettings.QueueName);

            var messageModel = new MessageModel()
            {
                TitleMessage = "New Leatrning Progression record {" + learningProgression.LearningProgressionId + "} added at " + DateTime.UtcNow,
                CustomerGuid = learningProgression.CustomerId,
                LastModifiedDate = learningProgression.LastModifiedDate,
                URL = reqUrl + "/" + learningProgression.LearningProgressionId,
                IsNewCustomer = false,
                TouchpointId = learningProgression.LastModifiedTouchpointID
            };

            var msg = new Message(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(messageModel)))
            {
                ContentType = "application/json",
                MessageId = $"{learningProgression.CustomerId} {DateTime.UtcNow}"
            };

            await queueClient.SendAsync(msg);
        }

        public async Task SendPatchMessageAsync(Models.LearningProgression learningProgression, Guid customerId, string reqUrl)
        {
            var queueClient = new QueueClient(_learningProgressionConfigurationSettings.ServiceBusConnectionString, _learningProgressionConfigurationSettings.QueueName);

            var messageModel = new MessageModel
            {
                TitleMessage = "Learning Progrerssion record modification for {" + customerId + "} at " + DateTime.UtcNow,
                CustomerGuid = customerId,
                LastModifiedDate = learningProgression.LastModifiedDate,
                URL = reqUrl,
                IsNewCustomer = false,
                 TouchpointId = learningProgression.LastModifiedTouchpointID
            };

            var msg = new Message(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(messageModel)))
            {
                ContentType = "application/json",
                MessageId = customerId + " " + DateTime.UtcNow
            };

            await queueClient.SendAsync(msg);
        }
    }
}
