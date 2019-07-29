using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NCS.DSS.LearningProgression.Services
{
    public interface ILearningProgressionServices
    {
        Task<Models.LearningProgression> CreateLearningProgressionAsync(Models.LearningProgression learningProgression);
        Task<Models.LearningProgression> GetLearningProgressionForCustomerAsync(Guid customerId, Guid learningProgressionId);
        Task<List<Models.LearningProgression>> GetLearningProgressionsForCustomerAsync(Guid customerId);
        bool DoesLearningProgressionExistForCustomer(Guid customerId);
        Task<bool> DoesCustomerExist(Guid customerId);
        Task SendToServiceBusQueueAsync(Models.LearningProgression learningProgression, string reqUrl);
        void SetIds(Models.LearningProgression learningProgression, Guid customerGuid, string touchpointId, string subcontractorId);
        string PatchLearningProgressionAsync(string learningProgressionAsJson, Models.LearningProgressionPatch learningProgressionPatch);
        Task<string> GetLearningProgressionForCustomerToPatchAsync(Guid customerId, Guid learningProgressionId);
    }
}