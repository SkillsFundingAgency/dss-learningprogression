namespace NCS.DSS.LearningProgression.ServiceBus
{
    public interface IServiceBusClient
    {
        Task SendPatchMessageAsync(Models.LearningProgression learningProgression, Guid customerId, string reqUrl, Guid correlationId);
        Task SendPostMessageAsync(Models.LearningProgression learningProgression, string reqUrl, Guid correlationId);
    }
}