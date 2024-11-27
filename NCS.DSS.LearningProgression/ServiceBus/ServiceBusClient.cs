using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Logging;
using NCS.DSS.LearningProgression.Models;
using System.Text;
using System.Text.Json;

namespace NCS.DSS.LearningProgression.ServiceBus
{
    public class ServiceBusClient : IServiceBusClient
    {
        private readonly ILogger<ServiceBusClient> _logger;
        private readonly ServiceBusSender _serviceBusSender;

        public ServiceBusClient(Azure.Messaging.ServiceBus.ServiceBusClient serviceBusClient, LearningProgressionConfigurationSettings learningProgressionConfigurationSettings, ILogger<ServiceBusClient> logger)
        {
            _logger = logger;
            _serviceBusSender = serviceBusClient.CreateSender(learningProgressionConfigurationSettings.QueueName);
        }

        public async Task SendPostMessageAsync(Models.LearningProgression learningProgression, string reqUrl, Guid correlationId)
        {
            var messageModel = new MessageModel()
            {
                TitleMessage = "New Learning Progression record {" + learningProgression.LearningProgressionId + "} added at " + DateTime.UtcNow,
                CustomerGuid = learningProgression.CustomerId,
                LastModifiedDate = learningProgression.LastModifiedDate,
                URL = reqUrl + "/" + learningProgression.LearningProgressionId,
                IsNewCustomer = false,
                TouchpointId = learningProgression.LastModifiedTouchpointId
            };

            var msg = new ServiceBusMessage(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(messageModel)))
            {
                ContentType = "application/json",
                MessageId = $"{learningProgression.CustomerId} {DateTime.UtcNow}"
            };

            var messageModelSerialized = JsonSerializer.Serialize(messageModel, new JsonSerializerOptions()
            {
                WriteIndented = true
            });

            _logger.LogInformation(
                "New Employment Progression record {MessageModel}. LearningProgressionId: {LearningProgressionId}. CorrelationId: {correlationId}",
                messageModelSerialized, learningProgression.LearningProgressionId, correlationId);

            await _serviceBusSender.SendMessageAsync(msg);
        }

        public async Task SendPatchMessageAsync(Models.LearningProgression learningProgression, Guid customerId, string reqUrl, Guid correlationId)
        {
            var messageModel = new MessageModel
            {
                TitleMessage = "Learning Progression record modification for {" + customerId + "} at " + DateTime.UtcNow,
                CustomerGuid = customerId,
                LastModifiedDate = learningProgression.LastModifiedDate,
                URL = reqUrl,
                IsNewCustomer = false,
                TouchpointId = learningProgression.LastModifiedTouchpointId
            };

            var msg = new ServiceBusMessage(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(messageModel)))
            {
                ContentType = "application/json",
                MessageId = customerId + " " + DateTime.UtcNow
            };

            var messageModelSerialized = JsonSerializer.Serialize(messageModel, new JsonSerializerOptions()
            {
                WriteIndented = true
            });

            _logger.LogInformation(
                "Learning Progression record modification for [{customerId}] at {DateTime}. Model: {messageModelSerialized}. CorrelationId: {CorrelationId}",
                customerId, DateTime.UtcNow, messageModelSerialized, correlationId);

            await _serviceBusSender.SendMessageAsync(msg);
        }
    }
}
