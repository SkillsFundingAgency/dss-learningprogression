﻿using System;
using System.Threading.Tasks;

namespace NCS.DSS.LearningProgression.GetLearningProgressionById.Service
{
    public interface ILearningProgressionGetByIdService
    {
        Task<Models.LearningProgression> GetLearningProgressionForCustomerAsync(Guid customerId, Guid progressionProgressionId);
    }
}