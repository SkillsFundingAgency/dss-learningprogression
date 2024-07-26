using System;
using System.Linq;
using DFC.HTTP.Standard;
using DFC.JSON.Standard;
using DFC.Swagger.Standard;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NCS.DSS.Contact.Cosmos.Helper;
using NCS.DSS.LearningProgression.Cosmos.Client;
using NCS.DSS.LearningProgression.Cosmos.Provider;
using NCS.DSS.LearningProgression.GetLearningProgression.Service;
using NCS.DSS.LearningProgression.GetLearningProgressionById.Service;
using NCS.DSS.LearningProgression.Models;
using NCS.DSS.LearningProgression.PatchLearningProgression.Service;
using NCS.DSS.LearningProgression.PostLearningProgression.Service;
using NCS.DSS.LearningProgression.ServiceBus;
using NCS.DSS.LearningProgression.Validators;

var learningProgressionConfigurationSettings = new LearningProgressionConfigurationSettings
{
    CosmosDBConnectionString = Environment.GetEnvironmentVariable("CosmosDBConnectionString"),
    KeyName = Environment.GetEnvironmentVariable("KeyName"),
    AccessKey = Environment.GetEnvironmentVariable("AccessKey"),
    BaseAddress = Environment.GetEnvironmentVariable("BaseAddress"),
    QueueName = Environment.GetEnvironmentVariable("QueueName"),
    ServiceBusConnectionString = Environment.GetEnvironmentVariable("ServiceBusConnectionString"),
};

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication()
    .ConfigureServices(services =>
    {
        services.AddApplicationInsightsTelemetryWorkerService();
        services.ConfigureFunctionsApplicationInsights();
        services.AddLogging();
        services.AddSingleton(learningProgressionConfigurationSettings);
        services.AddSingleton<ICosmosDocumentClient, CosmosDocumentClient>(x =>
            new CosmosDocumentClient(learningProgressionConfigurationSettings.CosmosDBConnectionString ?? throw new ArgumentNullException()));
        services.AddTransient<ISwaggerDocumentGenerator, SwaggerDocumentGenerator>();
        services.AddTransient<IHttpRequestHelper, HttpRequestHelper>();
        services.AddTransient<IJsonHelper, JsonHelper>();
        services.AddTransient<IServiceBusClient, ServiceBusClient>();
        services.AddTransient<IDocumentDBProvider, DocumentDBProvider>();
        services.AddTransient<IResourceHelper, ResourceHelper>();
        services.AddTransient<IValidate, Validate>();
        services.AddTransient<ILearningProgressionsGetTriggerService, LearningProgressionsGetTriggerService>();
        services.AddTransient<ILearningProgressionGetByIdService, LearningProgressionGetByIdService>();
        services.AddTransient<ILearningProgressionPatchTriggerService, LearningProgressionPatchTriggerService>();
        services.AddTransient<ILearningProgressionPostTriggerService, LearningProgressionPostTriggerService>();
    })
    .ConfigureLogging(logging =>
    {
        // The Application Insights SDK adds a default logging filter that instructs ILogger to capture only Warning and more severe logs. Application Insights requires an explicit override.
        // For more information, see https://learn.microsoft.com/en-us/azure/azure-functions/dotnet-isolated-process-guide?tabs=windows#application-insights
        logging.Services.Configure<LoggerFilterOptions>(options =>
        {
            LoggerFilterRule defaultRule = options.Rules.FirstOrDefault(rule => rule.ProviderName
                == "Microsoft.Extensions.Logging.ApplicationInsights.ApplicationInsightsLoggerProvider");
            if (defaultRule is not null)
            {
                options.Rules.Remove(defaultRule);
            }
        });
    })
    .Build();

host.Run();
