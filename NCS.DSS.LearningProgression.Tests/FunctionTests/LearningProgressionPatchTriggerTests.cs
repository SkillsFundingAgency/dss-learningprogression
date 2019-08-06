using System.Threading.Tasks;
using Xunit;
using System.Net;
using NSubstitute;
using DFC.HTTP.Standard;
using DFC.JSON.Standard;
using NCS.DSS.Contact.Cosmos.Helper;
using DFC.Common.Standard.Logging;
using NCS.DSS.LearningProgression.CosmosDocumentClient;
using Microsoft.AspNetCore.Http;
using NCS.DSS.LearningProgression.Validators;
using System;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using NCS.DSS.LearningProgression.PatchLearningProgression.Service;
using Newtonsoft.Json;

namespace NCS.DSS.LearningProgression.Tests.FunctionTests
{
    public class LearningProgressionPatchTriggerTests
    {
        const string CustomerId = "844a6215-8413-41ba-96b0-b4cc7041ca33";
        const string InvalidCustomerId = "InvalidCustomerId";
        const string LearningProgressionId = "844a6215-8413-41ba-96b0-b4cc7041ca33";
        const string InvalidLearningProgressionId = "InvalidLearningProgressionId";

        public LearningProgressionPatchTriggerTests()
        {
        }

        [Fact]
        async Task Get_WhenTouchPointHeaderIsMission_ReturnBadRequest()
        {
            var LearnerProgressConfigurationSettings = new LearnerProgressConfigurationSettings();
            var ResponseMessageHelper = new HttpResponseMessageHelper();
            var RequestHelper = Substitute.For<IHttpRequestHelper>();

            var LearningProgressionPatchTriggerService = Substitute.For<ILearningProgressionPatchTriggerService>();
            LearningProgressionPatchTriggerService.PatchLearningProgressionAsync(Arg.Any<string>(), Arg.Any<Models.LearningProgressionPatch>()).Returns<string>("AString");

            var JsonHelper = new JsonHelper();
            var ResourceHelper = Substitute.For<IResourceHelper>();
            var Valdiator = Substitute.For<IValidate>();
            var CosmosDBClient = Substitute.For<ICosmosDocumentClient>();
            var LoggerHelper = Substitute.For<ILoggerHelper>();

            var httpPostFunction = new LearningProgressionPatchTrigger(
                LearnerProgressConfigurationSettings,
                ResponseMessageHelper,
                RequestHelper,
                LearningProgressionPatchTriggerService,
                JsonHelper,
                ResourceHelper,
                CosmosDBClient,
                Valdiator,
                LoggerHelper
                );

            // Act
            var response = await httpPostFunction.Run(TestFactory.CreateHttpRequest("", ""), TestFactory.CreateLogger(), CustomerId, LearningProgressionId);

            //Assert
            Assert.True(response.StatusCode == HttpStatusCode.BadRequest);
        }

        [Fact]
        async Task Get_WhenGetDssApimUrlGetDssApimUrlIsEMpty_ReturnBadRequest()
        {
            // arrange
            var LearnerProgressConfigurationSettings = new LearnerProgressConfigurationSettings();
            var ResponseMessageHelper = new HttpResponseMessageHelper();
            var RequestHelper = Substitute.For<IHttpRequestHelper>();
            RequestHelper.GetDssTouchpointId(Arg.Any<HttpRequest>()).Returns<string>("0000000001");

            var LearningProgressionPatchTriggerService = Substitute.For<ILearningProgressionPatchTriggerService>();
            LearningProgressionPatchTriggerService.PatchLearningProgressionAsync(Arg.Any<string>(), Arg.Any<Models.LearningProgressionPatch>()).Returns<string>("AString");

            var JsonHelper = new JsonHelper();
            var ResourceHelper = Substitute.For<IResourceHelper>();
            var Valdiator = Substitute.For<IValidate>();
            var CosmosDBClient = Substitute.For<ICosmosDocumentClient>();
            var LoggerHelper = Substitute.For<ILoggerHelper>();

            var httpPostFunction = new LearningProgressionPatchTrigger(
                LearnerProgressConfigurationSettings,
                ResponseMessageHelper,
                RequestHelper,
                LearningProgressionPatchTriggerService,
                JsonHelper,
                ResourceHelper,
                CosmosDBClient,
                Valdiator,
                LoggerHelper
                );

            // Act
            var response = await httpPostFunction.Run(TestFactory.CreateHttpRequest("", ""), TestFactory.CreateLogger(), CustomerId, LearningProgressionId);

            //Assert
            Assert.True(response.StatusCode == HttpStatusCode.BadRequest);
        }

        [Fact]
        async Task Get_CustomerIdIsNotValidGuid_ReturnBadRequest()
        {
            // arrange
            var LearnerProgressConfigurationSettings = new LearnerProgressConfigurationSettings();
            var ResponseMessageHelper = new HttpResponseMessageHelper();
            var RequestHelper = Substitute.For<IHttpRequestHelper>();
            RequestHelper.GetDssTouchpointId(Arg.Any<HttpRequest>()).Returns<string>("0000000001");
            RequestHelper.GetDssApimUrl(Arg.Any<HttpRequest>()).Returns<string>("http://aurlvalue.com");

            var LearningProgressionPatchTriggerService = Substitute.For<ILearningProgressionPatchTriggerService>();
            LearningProgressionPatchTriggerService.PatchLearningProgressionAsync(Arg.Any<string>(), Arg.Any<Models.LearningProgressionPatch>()).Returns<string>("AString");

            var JsonHelper = new JsonHelper();
            var ResourceHelper = Substitute.For<IResourceHelper>();
            var Valdiator = Substitute.For<IValidate>();
            var CosmosDBClient = Substitute.For<ICosmosDocumentClient>();
            var LoggerHelper = Substitute.For<ILoggerHelper>();

            var httpPostFunction = new LearningProgressionPatchTrigger(
                LearnerProgressConfigurationSettings,
                ResponseMessageHelper,
                RequestHelper,
                LearningProgressionPatchTriggerService,
                JsonHelper,
                ResourceHelper,
                CosmosDBClient,
                Valdiator,
                LoggerHelper
                );

            // Act
            var response = await httpPostFunction.Run(TestFactory.CreateHttpRequest("", ""), TestFactory.CreateLogger(), InvalidCustomerId, LearningProgressionId);

            //Assert
            Assert.True(response.StatusCode == HttpStatusCode.BadRequest);
        }


        [Fact]
        async Task Get_LearningProgressionIdIsNotValidGuid_ReturnBadRequest()
        {
            // arrange
            var LearnerProgressConfigurationSettings = new LearnerProgressConfigurationSettings();
            var ResponseMessageHelper = new HttpResponseMessageHelper();
            var RequestHelper = Substitute.For<IHttpRequestHelper>();
            RequestHelper.GetDssTouchpointId(Arg.Any<HttpRequest>()).Returns<string>("0000000001");
            RequestHelper.GetDssApimUrl(Arg.Any<HttpRequest>()).Returns<string>("http://aurlvalue.com");

            var LearningProgressionPatchTriggerService = Substitute.For<ILearningProgressionPatchTriggerService>();
            LearningProgressionPatchTriggerService.PatchLearningProgressionAsync(Arg.Any<string>(), Arg.Any<Models.LearningProgressionPatch>()).Returns<string>("AString");

            var JsonHelper = new JsonHelper();
            var ResourceHelper = Substitute.For<IResourceHelper>();
            var Valdiator = Substitute.For<IValidate>();
            var CosmosDBClient = Substitute.For<ICosmosDocumentClient>();
            var LoggerHelper = Substitute.For<ILoggerHelper>();

            var httpPostFunction = new LearningProgressionPatchTrigger(
                LearnerProgressConfigurationSettings,
                ResponseMessageHelper,
                RequestHelper,
                LearningProgressionPatchTriggerService,
                JsonHelper,
                ResourceHelper,
                CosmosDBClient,
                Valdiator,
                LoggerHelper
                );

            // Act
            var response = await httpPostFunction.Run(TestFactory.CreateHttpRequest("", ""), TestFactory.CreateLogger(), CustomerId, InvalidLearningProgressionId);

            //Assert
            Assert.True(response.StatusCode == HttpStatusCode.BadRequest);
        }

        [Fact]
        async Task Get_InvalidBody_ReturnBadRequest()
        {
            // arrange
            var LearnerProgressConfigurationSettings = new LearnerProgressConfigurationSettings();
            var ResponseMessageHelper = new HttpResponseMessageHelper();
            var RequestHelper = Substitute.For<IHttpRequestHelper>();
            RequestHelper.GetDssTouchpointId(Arg.Any<HttpRequest>()).Returns<string>("0000000001");
            RequestHelper.GetDssApimUrl(Arg.Any<HttpRequest>()).Returns<string>("http://aurlvalue.com");
            RequestHelper.GetResourceFromRequest<Models.LearningProgressionPatch>(Arg.Any<HttpRequest>()).Returns((Models.LearningProgressionPatch)null);

            var LearningProgressionPatchTriggerService = Substitute.For<ILearningProgressionPatchTriggerService>();
            LearningProgressionPatchTriggerService.PatchLearningProgressionAsync(Arg.Any<string>(), Arg.Any<Models.LearningProgressionPatch>()).Returns<string>("AString");

         
            var JsonHelper = new JsonHelper();
            var ResourceHelper = Substitute.For<IResourceHelper>();
            var Valdiator = Substitute.For<IValidate>();
            var CosmosDBClient = Substitute.For<ICosmosDocumentClient>();
            var LoggerHelper = Substitute.For<ILoggerHelper>();

            var httpPostFunction = new LearningProgressionPatchTrigger(
                LearnerProgressConfigurationSettings,
                ResponseMessageHelper,
                RequestHelper,
                LearningProgressionPatchTriggerService,
                JsonHelper,
                ResourceHelper,
                CosmosDBClient,
                Valdiator,
                LoggerHelper
                );

            // Act
            var response = await httpPostFunction.Run(TestFactory.CreateHttpRequest("", ""), TestFactory.CreateLogger(), CustomerId, LearningProgressionId);

            //Assert
            Assert.True(response.StatusCode == HttpStatusCode.NoContent);
        }

        [Fact]
        async Task Get_ReadOnlyCustomer_ReturnBadRequest()
        {
            // arrange
            var LearnerProgressConfigurationSettings = new LearnerProgressConfigurationSettings();
            var ResponseMessageHelper = new HttpResponseMessageHelper();
            var RequestHelper = Substitute.For<IHttpRequestHelper>();
            RequestHelper.GetDssTouchpointId(Arg.Any<HttpRequest>()).Returns<string>("0000000001");
            RequestHelper.GetDssApimUrl(Arg.Any<HttpRequest>()).Returns<string>("http://aurlvalue.com");
            RequestHelper.GetResourceFromRequest<Models.LearningProgressionPatch>(Arg.Any<HttpRequest>()).Returns(new Models.LearningProgressionPatch());

            var LearningProgressionPatchTriggerService = Substitute.For<ILearningProgressionPatchTriggerService>();
            LearningProgressionPatchTriggerService.PatchLearningProgressionAsync(Arg.Any<string>(), Arg.Any<Models.LearningProgressionPatch>()).Returns<string>("AString");

            var JsonHelper = new JsonHelper();
            var ResourceHelper = Substitute.For<IResourceHelper>();
            ResourceHelper.IsCustomerReadOnly(Arg.Any<Guid>()).Returns<bool>(true);
            ResourceHelper.DoesCustomerExist(Arg.Any<Guid>()).Returns<bool>(true);

            var Valdiator = Substitute.For<IValidate>();
            var CosmosDBClient = Substitute.For<ICosmosDocumentClient>();
            var LoggerHelper = Substitute.For<ILoggerHelper>();

            var httpPostFunction = new LearningProgressionPatchTrigger(
                LearnerProgressConfigurationSettings,
                ResponseMessageHelper,
                RequestHelper,
                LearningProgressionPatchTriggerService,
                JsonHelper,
                ResourceHelper,
                CosmosDBClient,
                Valdiator,
                LoggerHelper
                );

            // Act
            var response = await httpPostFunction.Run(TestFactory.CreateHttpRequest("", ""), TestFactory.CreateLogger(), CustomerId, LearningProgressionId);

            //Assert
            Assert.True(response.StatusCode == HttpStatusCode.Forbidden);
        }


        [Fact]
        async Task Get_CustomerIdIsValidGuidButCustomerDoesNotExist_ReturnBadRequest()
        {
            // arrange
            var LearnerProgressConfigurationSettings = new LearnerProgressConfigurationSettings();
            var ResponseMessageHelper = new HttpResponseMessageHelper();
            var RequestHelper = Substitute.For<IHttpRequestHelper>();
            RequestHelper.GetDssTouchpointId(Arg.Any<HttpRequest>()).Returns<string>("0000000001");
            RequestHelper.GetDssApimUrl(Arg.Any<HttpRequest>()).Returns<string>("http://aurlvalue.com");
            RequestHelper.GetResourceFromRequest<Models.LearningProgressionPatch>(Arg.Any<HttpRequest>()).Returns(new Models.LearningProgressionPatch());

            var LearningProgressionPatchTriggerService = Substitute.For<ILearningProgressionPatchTriggerService>();
            LearningProgressionPatchTriggerService.PatchLearningProgressionAsync(Arg.Any<string>(), Arg.Any<Models.LearningProgressionPatch>()).Returns<string>(JsonConvert.SerializeObject(new Models.LearningProgressionPatch()));
            LearningProgressionPatchTriggerService.GetLearningProgressionForCustomerToPatchAsync(Arg.Any<Guid>(), Arg.Any<Guid>()).Returns(JsonConvert.SerializeObject(new Models.LearningProgressionPatch()));
            LearningProgressionPatchTriggerService.DoesLearningProgressionExistForCustomer(Arg.Any<Guid>()).Returns(true);

            var JsonHelper = new JsonHelper();
            var ResourceHelper = Substitute.For<IResourceHelper>();
            ResourceHelper.DoesCustomerExist(Arg.Any<Guid>()).Returns(false);
            ResourceHelper.IsCustomerReadOnly(Arg.Any<Guid>()).Returns<bool>(false);

            var Valdiator = Substitute.For<IValidate>();
            List<ValidationResult> ErrorResults = new List<ValidationResult>();

            var validationResult = new ValidationResult("Error MEssage");
            ErrorResults.Add(validationResult);
            Valdiator.ValidateResource(Arg.Any<Models.LearningProgression>()).Returns(ErrorResults);

            var CosmosDBClient = Substitute.For<ICosmosDocumentClient>();
            var LoggerHelper = Substitute.For<ILoggerHelper>();

            var httpPostFunction = new LearningProgressionPatchTrigger(
                LearnerProgressConfigurationSettings,
                ResponseMessageHelper,
                RequestHelper,
                LearningProgressionPatchTriggerService,
                JsonHelper,
                ResourceHelper,
                CosmosDBClient,
                Valdiator,
                LoggerHelper
                );

            // Act
            var response = await httpPostFunction.Run(TestFactory.CreateHttpRequest("", ""), TestFactory.CreateLogger(), CustomerId, LearningProgressionId);

            //Assert
            Assert.True(response.StatusCode == HttpStatusCode.BadRequest);
        }

        [Fact]
        async Task Get_LearningProvideDoesNotExistForCustomer_ReturnBadRequest()
        {
            // arrange
            var LearnerProgressConfigurationSettings = new LearnerProgressConfigurationSettings();
            var ResponseMessageHelper = new HttpResponseMessageHelper();
            var RequestHelper = Substitute.For<IHttpRequestHelper>();
            RequestHelper.GetDssTouchpointId(Arg.Any<HttpRequest>()).Returns<string>("0000000001");
            RequestHelper.GetDssApimUrl(Arg.Any<HttpRequest>()).Returns<string>("http://aurlvalue.com");
            RequestHelper.GetResourceFromRequest<Models.LearningProgressionPatch>(Arg.Any<HttpRequest>()).Returns(new Models.LearningProgressionPatch());

            var LearningProgressionPatchTriggerService = Substitute.For<ILearningProgressionPatchTriggerService>();
            LearningProgressionPatchTriggerService.PatchLearningProgressionAsync(Arg.Any<string>(), Arg.Any<Models.LearningProgressionPatch>()).Returns<string>("AString");
            LearningProgressionPatchTriggerService.DoesLearningProgressionExistForCustomer(Arg.Any<Guid>()).Returns(false);

            var JsonHelper = new JsonHelper();
            var ResourceHelper = Substitute.For<IResourceHelper>();
            var Valdiator = Substitute.For<IValidate>();
            var CosmosDBClient = Substitute.For<ICosmosDocumentClient>();
            var LoggerHelper = Substitute.For<ILoggerHelper>();

            var httpPostFunction = new LearningProgressionPatchTrigger(
                LearnerProgressConfigurationSettings,
                ResponseMessageHelper,
                RequestHelper,
                LearningProgressionPatchTriggerService,
                JsonHelper,
                ResourceHelper,
                CosmosDBClient,
                Valdiator,
                LoggerHelper
                );

            // Act
            var response = await httpPostFunction.Run(TestFactory.CreateHttpRequest("", ""), TestFactory.CreateLogger(), CustomerId, LearningProgressionId);

            //Assert
            Assert.True(response.StatusCode == HttpStatusCode.BadRequest);
        }

        [Fact]
        async Task Get_NoLearningProgressionPatchData_ReturnNoContent()
        {
            // arrange
            var LearnerProgressConfigurationSettings = new LearnerProgressConfigurationSettings();
            var ResponseMessageHelper = new HttpResponseMessageHelper();
            var RequestHelper = Substitute.For<IHttpRequestHelper>();
            RequestHelper.GetDssTouchpointId(Arg.Any<HttpRequest>()).Returns<string>("0000000001");
            RequestHelper.GetDssApimUrl(Arg.Any<HttpRequest>()).Returns<string>("http://aurlvalue.com");
            RequestHelper.GetResourceFromRequest<Models.LearningProgressionPatch>(Arg.Any<HttpRequest>()).Returns(new Models.LearningProgressionPatch());

            var LearningProgressionPatchTriggerService = Substitute.For<ILearningProgressionPatchTriggerService>();
            LearningProgressionPatchTriggerService.PatchLearningProgressionAsync(Arg.Any<string>(), Arg.Any<Models.LearningProgressionPatch>()).Returns<string>("AString");
            LearningProgressionPatchTriggerService.DoesLearningProgressionExistForCustomer(Arg.Any<Guid>()).Returns(true);
            LearningProgressionPatchTriggerService.GetLearningProgressionForCustomerToPatchAsync(Arg.Any<Guid>(), Arg.Any<Guid>()).Returns((string)null);

            var JsonHelper = new JsonHelper();
            var ResourceHelper = Substitute.For<IResourceHelper>();
            ResourceHelper.DoesCustomerExist(Arg.Any<Guid>()).Returns(true);

            var Valdiator = Substitute.For<IValidate>();
            var CosmosDBClient = Substitute.For<ICosmosDocumentClient>();
            var LoggerHelper = Substitute.For<ILoggerHelper>();

            var httpPostFunction = new LearningProgressionPatchTrigger(
                LearnerProgressConfigurationSettings,
                ResponseMessageHelper,
                RequestHelper,
                LearningProgressionPatchTriggerService,
                JsonHelper,
                ResourceHelper,
                CosmosDBClient,
                Valdiator,
                LoggerHelper
                );

            // Act
            var response = await httpPostFunction.Run(TestFactory.CreateHttpRequest("", ""), TestFactory.CreateLogger(), CustomerId, LearningProgressionId);

            //Assert
            Assert.True(response.StatusCode == HttpStatusCode.NoContent);
        }

        [Fact]
        async Task Get_ValidationFailed_ReturnUnprocessableEntity()
        {
            // arrange
            var LearnerProgressConfigurationSettings = new LearnerProgressConfigurationSettings();
            var ResponseMessageHelper = new HttpResponseMessageHelper();
            var RequestHelper = Substitute.For<IHttpRequestHelper>();
            RequestHelper.GetDssTouchpointId(Arg.Any<HttpRequest>()).Returns<string>("0000000001");
            RequestHelper.GetDssApimUrl(Arg.Any<HttpRequest>()).Returns<string>("http://aurlvalue.com");
            RequestHelper.GetResourceFromRequest<Models.LearningProgressionPatch>(Arg.Any<HttpRequest>()).Returns(new Models.LearningProgressionPatch());

            var LearningProgressionPatchTriggerService = Substitute.For<ILearningProgressionPatchTriggerService>();
            LearningProgressionPatchTriggerService.PatchLearningProgressionAsync(Arg.Any<string>(), Arg.Any<Models.LearningProgressionPatch>()).Returns<string>(JsonConvert.SerializeObject(new Models.LearningProgressionPatch()));
            LearningProgressionPatchTriggerService.GetLearningProgressionForCustomerToPatchAsync(Arg.Any<Guid>(), Arg.Any<Guid>()).Returns(JsonConvert.SerializeObject(new Models.LearningProgressionPatch()));
            LearningProgressionPatchTriggerService.DoesLearningProgressionExistForCustomer(Arg.Any<Guid>()).Returns(true);
  
            var JsonHelper = new JsonHelper();
            var ResourceHelper = Substitute.For<IResourceHelper>();
            ResourceHelper.DoesCustomerExist(Arg.Any<Guid>()).Returns(true);

            ResourceHelper.IsCustomerReadOnly(Arg.Any<Guid>()).Returns<bool>(false);
            ResourceHelper.DoesCustomerExist(Arg.Any<Guid>()).Returns<bool>(true);

            var Valdiator = Substitute.For<IValidate>();
            List<ValidationResult> ErrorResults = new List<ValidationResult>();

            var validationResult = new ValidationResult("Error MEssage");
            ErrorResults.Add(validationResult);
            Valdiator.ValidateResource(Arg.Any<Models.LearningProgression>()).Returns(ErrorResults);

            var CosmosDBClient = Substitute.For<ICosmosDocumentClient>();
            var LoggerHelper = Substitute.For<ILoggerHelper>();

            var httpPostFunction = new LearningProgressionPatchTrigger(
                LearnerProgressConfigurationSettings,
                ResponseMessageHelper,
                RequestHelper,
                LearningProgressionPatchTriggerService,
                JsonHelper,
                ResourceHelper,
                CosmosDBClient,
                Valdiator,
                LoggerHelper
                );

            // Act
            var response = await httpPostFunction.Run(TestFactory.CreateHttpRequest("", ""), TestFactory.CreateLogger(), CustomerId, LearningProgressionId);

            //Assert
            Assert.True(response.StatusCode == HttpStatusCode.UnprocessableEntity);
        }

        [Fact]
        async Task Get_SuccessRequest_ReturnOk()
        {
            // arrange
            var LearnerProgressConfigurationSettings = new LearnerProgressConfigurationSettings();
            var ResponseMessageHelper = new HttpResponseMessageHelper();
            var RequestHelper = Substitute.For<IHttpRequestHelper>();
            RequestHelper.GetDssTouchpointId(Arg.Any<HttpRequest>()).Returns<string>("0000000001");
            RequestHelper.GetDssApimUrl(Arg.Any<HttpRequest>()).Returns<string>("http://aurlvalue.com");
            RequestHelper.GetResourceFromRequest<Models.LearningProgressionPatch>(Arg.Any<HttpRequest>()).Returns(new Models.LearningProgressionPatch());

            var learningProgressionPatch = new Models.LearningProgressionPatch();
            RequestHelper.GetResourceFromRequest<Models.LearningProgressionPatch>(Arg.Any<HttpRequest>()).Returns<Models.LearningProgressionPatch>(learningProgressionPatch);

            var LearningProgressionPatchTriggerService = Substitute.For<ILearningProgressionPatchTriggerService>();
            LearningProgressionPatchTriggerService.PatchLearningProgressionAsync(Arg.Any<string>(), Arg.Any<Models.LearningProgressionPatch>()).Returns<string>(JsonConvert.SerializeObject(new Models.LearningProgressionPatch()));
            LearningProgressionPatchTriggerService.DoesLearningProgressionExistForCustomer(Arg.Any<Guid>()).Returns<bool>(true);

            var JsonHelper = new JsonHelper();
            var ResourceHelper = Substitute.For<IResourceHelper>();
            ResourceHelper.IsCustomerReadOnly(Arg.Any<Guid>()).Returns<bool>(false);
            ResourceHelper.DoesCustomerExist(Arg.Any<Guid>()).Returns<bool>(true);

            var Valdiator = Substitute.For<IValidate>();
            List<ValidationResult> ErrorResults = new List<ValidationResult>();
            Valdiator.ValidateResource(Arg.Any<Models.LearningProgression>()).Returns(ErrorResults);

            var CosmosDBClient = Substitute.For<ICosmosDocumentClient>();
            var LoggerHelper = Substitute.For<ILoggerHelper>();

            var httpPostFunction = new LearningProgressionPatchTrigger(
                LearnerProgressConfigurationSettings,
                ResponseMessageHelper,
                RequestHelper,
                LearningProgressionPatchTriggerService,
                JsonHelper,
                ResourceHelper,
                CosmosDBClient,
                Valdiator,
                LoggerHelper
                );

            // Act
            var response = await httpPostFunction.Run(TestFactory.CreateHttpRequest("", ""), TestFactory.CreateLogger(), CustomerId, LearningProgressionId);

            //Assert
            Assert.True(response.StatusCode == HttpStatusCode.OK);
        }
    }
}
