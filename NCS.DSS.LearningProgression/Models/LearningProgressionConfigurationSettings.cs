namespace NCS.DSS.LearningProgression.Models
{
    public class LearningProgressionConfigurationSettings
    {
        public string CosmosDBConnectionString { get; set; }
        public string KeyName { get; internal set; }
        public string AccessKey { get; internal set; }
        public string BaseAddress { get; internal set; }
        public string QueueName { get; internal set; }
        public string ServiceBusConnectionString { get; set; }
    }
}