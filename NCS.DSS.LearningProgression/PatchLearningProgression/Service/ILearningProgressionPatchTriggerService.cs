using Microsoft.Extensions.Logging;
using NCS.DSS.LearningProgression.Models;
using System;
using System.Threading.Tasks;

namespace NCS.DSS.LearningProgression.PatchLearningProgression.Service
{
    public interface ILearningProgressionPatchTriggerService
    {
        string PatchLearningProgressionAsync(string learningProgressionAsJson, LearningProgressionPatch learningProgressionPatch);
        Task<Models.LearningProgression> UpdateCosmosAsync(string learningProgressionAsJson, Guid learningProgressionId);
        Task SendToServiceBusQueueAsync(Models.LearningProgression learningProgression, Guid customerId, string reqUrl, Guid correlationId, ILogger log);
        Task<string> GetLearningProgressionForCustomerToPatchAsync(Guid customerId, Guid learningProgressionId);
        bool DoesLearningProgressionExistForCustomer(Guid customerId);
        Task<bool> DoesCustomerExist(Guid customerId);
        void SetIds(LearningProgressionPatch learningProgressionPatchRequest, Guid customerGuid, string touchpointId);
    }
}