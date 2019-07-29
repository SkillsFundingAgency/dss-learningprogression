using DFC.Swagger.Standard;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using System.Net;
using System.Net.Http;
using System.Reflection;
using NCS.DSS.LearningProgression.Constants;

namespace NCS.DSS.ActionPlan.APIDefinition
{
    public class ApiDefinition
    {
        public const string ApiTitle = "Contact";
        public const string ApiDefinitionName = "API-Definition";
        public const string ApiDefRoute = ApiTitle + "/" + ApiDefinitionName;
        public const string ApiDescription = "To support the Data Collections integration with DSS, SessionId and SubcontractorId " +
                                             "attributes have been added and DateActionPlanCreated, DateAndTimeCharterShown, " +
                                             "DateActionPlanSentToCustomer, DateActionPlanAcknowledged have new validation rules.";

        private readonly ISwaggerDocumentGenerator _swaggerDocumentGenerator;
        public const string ApiVersion = "2.0.0";

        public ApiDefinition(ISwaggerDocumentGenerator swaggerDocumentGenerator)
        {
            _swaggerDocumentGenerator = swaggerDocumentGenerator;
        }

        [FunctionName(ApiDefinitionName)]
        public HttpResponseMessage Run([HttpTrigger(AuthorizationLevel.Anonymous, Constant.MethodGet, Route = ApiDefRoute)]HttpRequest req)
        {
            var swaggerDoc = _swaggerDocumentGenerator.GenerateSwaggerDocument(req, ApiTitle, ApiDescription, 
                ApiDefinitionName, ApiVersion, Assembly.GetExecutingAssembly());

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(swaggerDoc)
            };
        }
    }
}