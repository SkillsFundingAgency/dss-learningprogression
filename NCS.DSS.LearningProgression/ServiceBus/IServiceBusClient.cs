using System;
using System.Threading.Tasks;

namespace NCS.DSS.LearningProgression.ServiceBus
{
    public interface IServiceBusClient
    {
        Task SendPatchMessageAsync(Models.LearningProgression learningProgression, Guid customerId, string reqUrl);
        Task SendPostMessageAsync(Models.LearningProgression learningProgression, string reqUrl);
    }
}