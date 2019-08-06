using System;
using System.Threading.Tasks;

namespace NCS.DSS.LearningProgression.PostLearningProgression.Service
{
    public interface ILearningProgressionPostTriggerService
    {
        Task<Models.LearningProgression> CreateLearningProgressionAsync(Models.LearningProgression learningProgression);
        Task SendToServiceBusQueueAsync(Models.LearningProgression learningProgression, Guid customerId, string reqUrl);
        bool DoesLearningProgressionExistForCustomer(Guid customerId);
        void SetIds(Models.LearningProgression learningProgression, Guid customerGuid, string touchpointId);
    }
}