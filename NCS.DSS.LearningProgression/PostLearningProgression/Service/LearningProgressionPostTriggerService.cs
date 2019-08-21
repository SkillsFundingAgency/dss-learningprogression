﻿using NCS.DSS.LearningProgression.Cosmos.Provider;
using NCS.DSS.LearningProgression.Models;
using NCS.DSS.LearningProgression.ServiceBus;
using System;
using System.Net;
using System.Threading.Tasks;

namespace NCS.DSS.LearningProgression.PostLearningProgression.Service
{
    public class LearningProgressionPostTriggerService : ILearningProgressionPostTriggerService
    {
        private readonly IDocumentDBProvider _documentDbProvider;
        private readonly IServiceBusClient _serviceBusClient;

        public LearningProgressionPostTriggerService(IDocumentDBProvider documentDbProvider,
             IServiceBusClient serviceBusClient)
        {
            _documentDbProvider = documentDbProvider;
            _serviceBusClient = serviceBusClient;
            
        }

        public async Task<Models.LearningProgression> CreateLearningProgressionAsync(Models.LearningProgression learningProgression)
        {
            if (learningProgression == null)
            {
                return null;
            }

            learningProgression.LearningProgressionId = Guid.NewGuid();

            if (!learningProgression.LastModifiedDate.HasValue)
            {
                learningProgression.LastModifiedDate = DateTime.UtcNow;
            }

            var response = await _documentDbProvider.CreateLearningProgressionAsync(learningProgression);

            return response.StatusCode == HttpStatusCode.Created ? (dynamic)response.Resource : (Guid?)null;
        }

        public async Task SendToServiceBusQueueAsync(Models.LearningProgression learningProgression, string reqUrl)
        {
            await _serviceBusClient.SendPostMessageAsync(learningProgression, reqUrl);
        }

        public bool DoesLearningProgressionExistForCustomer(Guid customerId)
        {
            return _documentDbProvider.DoesLearningProgressionExistForCustomer(customerId);
        }

        public void SetIds(Models.LearningProgression learningProgression, Guid customerGuid, string touchpointId)
        {
            learningProgression.LearningProgressionId = Guid.NewGuid();
            learningProgression.CustomerId = customerGuid;
            learningProgression.LastModifiedTouchpointID = touchpointId;
            learningProgression.CreatedBy = touchpointId;

            if (!learningProgression.DateProgressionRecorded.HasValue)
            {
                learningProgression.DateProgressionRecorded = DateTime.UtcNow;
            }

        }
    }
}
