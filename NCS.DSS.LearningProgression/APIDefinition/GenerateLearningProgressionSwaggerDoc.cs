using DFC.Swagger.Standard;
using Microsoft.AspNetCore.Http;
using NCS.DSS.LearningProgression.Constants;
using System.Net.Http;
using System.Reflection;
using Microsoft.Azure.Functions.Worker;
using Microsoft.AspNetCore.Mvc;

namespace NCS.DSS.LearningProgression.APIDefinition
{
    public class GenerateLearningProgressionSwaggerDoc
    {
        public const string ApiTitle = "LearningProgressions";
        public const string ApiDefinitionName = "API-Definition";
        public const string ApiDefRoute = ApiTitle + "/" + ApiDefinitionName;
        public const string ApiDescription = "Initial release of Learning Progression.";

        private readonly ISwaggerDocumentGenerator _swaggerDocumentGenerator;
        public const string ApiVersion = "2.0.0";

        public GenerateLearningProgressionSwaggerDoc(ISwaggerDocumentGenerator swaggerDocumentGenerator)
        {
            _swaggerDocumentGenerator = swaggerDocumentGenerator;
        }

        [Function(ApiDefinitionName)]
        public IActionResult Run([HttpTrigger(AuthorizationLevel.Anonymous, Constant.MethodGet, Route = ApiDefRoute)]HttpRequest req)
        {
            var swaggerDoc = _swaggerDocumentGenerator.GenerateSwaggerDocument(req, ApiTitle, ApiDescription, 
                ApiDefinitionName, ApiVersion, Assembly.GetExecutingAssembly(), false);

            if (string.IsNullOrEmpty(swaggerDoc))
                return new NoContentResult();

            return new OkObjectResult(new StringContent(swaggerDoc));
        }
    }
}