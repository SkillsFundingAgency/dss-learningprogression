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
            _logger.LogInformation(
                "Starting {MethodName}. LearningProgressionId: {LearningProgressionId}. CustomerId: {CustomerId}. CorrelationId: {CorrelationId}",
                nameof(SendPostMessageAsync), learningProgression.LearningProgressionId, learningProgression.CustomerId, correlationId);

            try
            {
                var messageModel = new MessageModel()
                {
                    TitleMessage = $"New Learning Progression record {learningProgression.LearningProgressionId} added at {DateTime.UtcNow}",
                    CustomerGuid = learningProgression.CustomerId,
                    LastModifiedDate = learningProgression.LastModifiedDate,
                    URL = $"{reqUrl}/{learningProgression.LearningProgressionId}",
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
                    "New Employment Progression record serialized: {MessageModel}. LearningProgressionId: {LearningProgressionId}. CorrelationId: {CorrelationId}",
                    messageModelSerialized, learningProgression.LearningProgressionId, correlationId);

                await _serviceBusSender.SendMessageAsync(msg);

                _logger.LogInformation(
                    "Successfully completed {MethodName}. LearningProgressionId: {LearningProgressionId}. CustomerId: {CustomerId}. CorrelationId: {CorrelationId}",
                    nameof(SendPostMessageAsync), learningProgression.LearningProgressionId, learningProgression.CustomerId, correlationId);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "An error occurred in {MethodName}. LearningProgressionId: {LearningProgressionId}. CustomerId: {CustomerId}. CorrelationId: {CorrelationId}",
                    nameof(SendPostMessageAsync), learningProgression.LearningProgressionId, learningProgression.CustomerId, correlationId);
                throw;
            }
        }

        public async Task SendPatchMessageAsync(Models.LearningProgression learningProgression, Guid customerId, string reqUrl, Guid correlationId)
        {
            _logger.LogInformation(
                "Starting {MethodName}. CustomerId: {CustomerId}. LearningProgressionId: {LearningProgressionId}. CorrelationId: {CorrelationId}",
                nameof(SendPatchMessageAsync), customerId, learningProgression.LearningProgressionId, correlationId);

            try
            {
                var messageModel = new MessageModel
                {
                    TitleMessage = $"Learning Progression record modification for {customerId} at {DateTime.UtcNow}",
                    CustomerGuid = customerId,
                    LastModifiedDate = learningProgression.LastModifiedDate,
                    URL = reqUrl,
                    IsNewCustomer = false,
                    TouchpointId = learningProgression.LastModifiedTouchpointId
                };

                var msg = new ServiceBusMessage(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(messageModel)))
                {
                    ContentType = "application/json",
                    MessageId = $"{customerId} {DateTime.UtcNow}"
                };

                var messageModelSerialized = JsonSerializer.Serialize(messageModel, new JsonSerializerOptions
                {
                    WriteIndented = true
                });

                _logger.LogInformation(
                    "Learning Progression record modification serialized. CustomerId: {CustomerId}. LearningProgressionId: {LearningProgressionId}. CorrelationId: {CorrelationId}, Model: {MessageModelSerialized}",
                    customerId, learningProgression.LearningProgressionId, correlationId, messageModelSerialized);

                await _serviceBusSender.SendMessageAsync(msg);

                _logger.LogInformation(
                    "Successfully completed {MethodName}. CustomerId: {CustomerId}. LearningProgressionId: {LearningProgressionId}. CorrelationId: {CorrelationId}",
                    nameof(SendPatchMessageAsync), customerId, learningProgression.LearningProgressionId, correlationId);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "An error occurred in {MethodName}. CustomerId: {CustomerId}. LearningProgressionId: {LearningProgressionId}. CorrelationId: {CorrelationId}",
                    nameof(SendPatchMessageAsync), customerId, learningProgression.LearningProgressionId, correlationId);
                throw;
            }
        }
    }
}
