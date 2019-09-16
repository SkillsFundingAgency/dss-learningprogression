using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;

namespace NCS.DSS.LearningProgression.Cosmos.Provider
{
    public interface IDocumentDBProvider
    {
        Task<bool> DoesCustomerResourceExist(Guid customerId);
        Task<bool> DoesCustomerHaveATerminationDate(Guid customerId);
        bool DoesLearningProgressionExistForCustomer(Guid customerId);
        Task<Models.LearningProgression> GetLearningProgressionForCustomerAsync(Guid customerId, Guid learningProgressionId);
        Task<List<Models.LearningProgression>> GetLearningProgressionsForCustomerAsync(Guid customerId);
        Task<ResourceResponse<Document>> CreateLearningProgressionAsync(Models.LearningProgression learningProgression);
        Task<ResourceResponse<Document>> UpdateLearningProgressionAsync(string learningProgressionJson, Guid learningProgressionId);
        Task<string> GetLearningProgressionForCustomerToPatchAsync(Guid customerId, Guid learningProgressionId);
    }
}