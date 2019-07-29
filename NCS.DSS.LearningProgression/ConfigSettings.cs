namespace NCS.DSS.LearningProgression
{
    public class LearnerProgressConfigurationSettings
    {
        public string CosmosDBConnectionString { get; set; }
        public string KeyName { get; internal set; }
        public string AccessKey { get; internal set; }
        public string BaseAddress { get; internal set; }
        public string QueueName { get; internal set; }
    }
}