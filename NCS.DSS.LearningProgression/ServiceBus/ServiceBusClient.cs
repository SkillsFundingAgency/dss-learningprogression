using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.Logging;
using NCS.DSS.LearningProgression.Models;
using Newtonsoft.Json;
using System.Text;

namespace NCS.DSS.LearningProgression.ServiceBus
{
    public class ServiceBusClient : IServiceBusClient
    {
        private readonly LearningProgressionConfigurationSettings _learningProgressionConfigurationSettings;
        private readonly QueueClient _queueClient;
        private readonly ILogger<ServiceBusClient> _logger;

        public ServiceBusClient(LearningProgressionConfigurationSettings learnerProgressConfigurationSettings, ILogger<ServiceBusClient> logger)
        {
            _learningProgressionConfigurationSettings = learnerProgressConfigurationSettings;
            _queueClient = new QueueClient(_learningProgressionConfigurationSettings.ServiceBusConnectionString, _learningProgressionConfigurationSettings.QueueName);
            _logger = logger;
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

            var msg = new Message(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(messageModel)))
            {
                ContentType = "application/json",
                MessageId = $"{learningProgression.CustomerId} {DateTime.UtcNow}"
            };

            var messageModelSerialized = JsonConvert.SerializeObject(messageModel, Formatting.Indented);
            _logger.LogInformation(
                "New Employment Progression record {MessageModel}. LearningProgressionId: {LearningProgressionId}. CorrelationId: {correlationId}",
                messageModelSerialized, learningProgression.LearningProgressionId, correlationId);

            await _queueClient.SendAsync(msg);
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

            var msg = new Message(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(messageModel)))
            {
                ContentType = "application/json",
                MessageId = customerId + " " + DateTime.UtcNow
            };

            var messageModelSerialized = JsonConvert.SerializeObject(messageModel, Formatting.Indented);

            _logger.LogInformation(
                "Learning Progression record modification for [{customerId}] at {DateTime}. Model: {messageModelSerialized}. CorrelationId: {CorrelationId}",
                customerId, DateTime.UtcNow, messageModelSerialized, correlationId);

            await _queueClient.SendAsync(msg);
        }
    }
}
