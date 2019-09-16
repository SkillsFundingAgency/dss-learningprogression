using System;
using System.Net;
using System.Threading.Tasks;
using NCS.DSS.LearningProgression.ServiceBus;
using NCS.DSS.LearningProgression.Cosmos.Provider;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using DFC.JSON.Standard;

namespace NCS.DSS.LearningProgression.Services
{
    public class LearningProgressionServices
    {
        private IDocumentDBProvider _documentDbProvider;
        private readonly IServiceBusClient _serviceBusClient;
        private readonly LearnerProgressConfigurationSettings _learnerProgressConfigurationSettings;
        private readonly ILogger _logger;
        private readonly IJsonHelper _jsonHelper;

        public LearningProgressionServices(
            IDocumentDBProvider documentDbProvider,
            IServiceBusClient serviceBusClient,
            LearnerProgressConfigurationSettings learnerProgressConfigurationSettings,
            ILogger logger,
            IJsonHelper jsonHelper
            )
        {
            _documentDbProvider = documentDbProvider;
            _serviceBusClient = serviceBusClient;
            _learnerProgressConfigurationSettings = learnerProgressConfigurationSettings;
            _logger = logger;
            _jsonHelper = jsonHelper;
        }




    }
}