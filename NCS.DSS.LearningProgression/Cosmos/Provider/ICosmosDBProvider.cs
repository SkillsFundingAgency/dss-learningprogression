using Microsoft.Azure.Cosmos;

namespace NCS.DSS.LearningProgression.Cosmos.Provider
{
    public interface ICosmosDBProvider
    {
        Task<bool> DoesCustomerResourceExist(Guid customerId);
        Task<bool> DoesCustomerHaveATerminationDate(Guid customerId);
        Task<bool> DoesLearningProgressionExistForCustomer(Guid customerId);
        Task<Models.LearningProgression?> GetLearningProgressionForCustomerAsync(Guid customerId, Guid learningProgressionId);
        Task<List<Models.LearningProgression>> GetLearningProgressionsForCustomerAsync(Guid customerId);
        Task<ItemResponse<Models.LearningProgression>> CreateLearningProgressionAsync(Models.LearningProgression learningProgression);
        Task<ItemResponse<Models.LearningProgression>?> UpdateLearningProgressionAsync(string learningProgressionJson, Guid learningProgressionId);
        Task<string?> GetLearningProgressionForCustomerToPatchAsync(Guid customerId, Guid learningProgressionId);
    }
}