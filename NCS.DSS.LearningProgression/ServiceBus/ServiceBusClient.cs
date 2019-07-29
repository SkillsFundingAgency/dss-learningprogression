using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using Newtonsoft.Json;

namespace NCS.DSS.LearningProgression.ServiceBus
{
    public class ServiceBusClient : IServiceBusClient
    {
        private LearnerProgressConfigurationSettings _learnerProgressConfigurationSettings;

        public async Task SendPostMessageAsync(Models.LearningProgression learningProgression, string reqUrl, LearnerProgressConfigurationSettings learnerProgressConfigurationSettings, ILogger logger)
        {
            _learnerProgressConfigurationSettings = learnerProgressConfigurationSettings;

            var tokenProvider = TokenProvider.CreateSharedAccessSignatureTokenProvider(_learnerProgressConfigurationSettings.KeyName, _learnerProgressConfigurationSettings.AccessKey);
            var messagingFactory = MessagingFactory.Create(_learnerProgressConfigurationSettings.BaseAddress, tokenProvider);
            var sender = messagingFactory.CreateMessageSender(_learnerProgressConfigurationSettings.QueueName);

            var messageModel = new MessageModel()
            {
                //TitleMessage = "New Contact Details record {" + learningProgression.ContactId + "} added at " + DateTime.UtcNow,
                //CustomerGuid = learningProgression.CustomerId,
                //LastModifiedDate = learningProgression.LastModifiedDate,
                //URL = reqUrl + "/" + learningProgression.ContactId,
                //IsNewCustomer = false,
                //TouchpointId = learningProgression.LastModifiedTouchpointId
            };

            var msg = new BrokeredMessage(new MemoryStream(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(messageModel))))
            {
                ContentType = "application/json",
                MessageId = learningProgression.CustomerId + " " + DateTime.UtcNow
            };

            //msg.ForcePersistence = true; Required when we save message to cosmos
            await sender.SendAsync(msg);
        }

        public async Task SendPatchMessageAsync(Models.LearningProgression learningProgression, Guid customerId, string reqUrl)
        {
            var tokenProvider = TokenProvider.CreateSharedAccessSignatureTokenProvider(_learnerProgressConfigurationSettings.KeyName, _learnerProgressConfigurationSettings.AccessKey);
            var messagingFactory = MessagingFactory.Create(_learnerProgressConfigurationSettings.BaseAddress, tokenProvider);
            var sender = messagingFactory.CreateMessageSender(_learnerProgressConfigurationSettings.QueueName);
            var messageModel = new MessageModel
            {
                TitleMessage = "Contact Details record modification for {" + customerId + "} at " + DateTime.UtcNow,
                CustomerGuid = customerId,
                LastModifiedDate = learningProgression.LastModifiedDate,
                URL = reqUrl,
                IsNewCustomer = false,
                // TouchpointId = learningProgression.LastModifiedTouchpointId
            };

            var msg = new BrokeredMessage(new MemoryStream(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(messageModel))))
            {
                ContentType = "application/json",
                MessageId = customerId + " " + DateTime.UtcNow
            };

            //msg.ForcePersistence = true; Required when we save message to cosmos
            await sender.SendAsync(msg);
        }
    }
}
