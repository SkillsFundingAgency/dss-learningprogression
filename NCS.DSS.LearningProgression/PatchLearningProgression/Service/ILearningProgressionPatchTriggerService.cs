using NCS.DSS.LearningProgression.Models;
using System;
using System.Threading.Tasks;

namespace NCS.DSS.LearningProgression.PatchLearningProgression.Service
{
    public interface ILearningProgressionPatchTriggerService
    {
        string PatchLearningProgressionAsync(string learningProgressionAsJson, LearningProgressionPatch learningProgressionPatch);
        Task<Models.LearningProgression> UpdateCosmosAsync(string learningProgressionAsJson, Guid learningProgressionId);
        Task<string> GetLearningProgressionForCustomerToPatchAsync(Guid customerId, Guid learningProgressionId);
        bool DoesLearningProgressionExistForCustomer(Guid customerId);
        Task<bool> DoesCustomerExist(Guid customerId);
    }
}