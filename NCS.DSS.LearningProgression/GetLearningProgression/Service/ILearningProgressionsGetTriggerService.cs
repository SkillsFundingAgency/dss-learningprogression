using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NCS.DSS.LearningProgression.GetLearningProgression.Service
{
    public interface ILearningProgressionsGetTriggerService
    {
        Task<List<Models.LearningProgression>> GetLearningProgressionsForCustomerAsync(Guid customerId);
    }
}