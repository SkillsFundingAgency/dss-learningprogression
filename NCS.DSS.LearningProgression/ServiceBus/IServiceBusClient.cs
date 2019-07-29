using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NCS.DSS.LearningProgression.Models;

namespace NCS.DSS.LearningProgression.ServiceBus
{
    public interface IServiceBusClient
    {
        Task SendPatchMessageAsync(Models.LearningProgression learningProgression, Guid customerId, string reqUrl);
        Task SendPostMessageAsync(Models.LearningProgression learningProgression, string reqUrl, LearnerProgressConfigurationSettings learnerProgressConfigurationSettings, ILogger logger);
    }
}