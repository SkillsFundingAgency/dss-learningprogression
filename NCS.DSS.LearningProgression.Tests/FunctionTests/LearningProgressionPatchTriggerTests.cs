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
using NCS.DSS.LearningProgression.Models;
using NCS.DSS.LearningProgression.Enumerations;
using Newtonsoft.Json.Linq;

namespace NCS.DSS.LearningProgression.Tests.FunctionTests
{
    public class LearningProgressionPatchTriggerTests
    {
        const string CustomerId = "844a6215-8413-41ba-96b0-b4cc7041ca33";
        const string InvalidCustomerId = "InvalidCustomerId";
        const string LearningProgressionId = "844a6215-8413-41ba-96b0-b4cc7041ca33";
        const string InvalidLearningProgressionId = "InvalidLearningProgressionId";

        [Fact]
        public async Task Get_WhenTouchPointHeaderIsMission_ReturnBadRequest()
        {

            var ResponseMessageHelper = new HttpResponseMessageHelper();
            var RequestHelper = Substitute.For<IHttpRequestHelper>();

            var LearningProgressionPatchTriggerService = Substitute.For<ILearningProgressionPatchTriggerService>();
            LearningProgressionPatchTriggerService.PatchLearningProgressionAsync(Arg.Any<string>(), Arg.Any<Models.LearningProgressionPatch>()).Returns<string>("AString");

            var JsonHelper = new JsonHelper();
            var ResourceHelper = Substitute.For<IResourceHelper>();
            var Valdiator = Substitute.For<IValidate>();

            var LoggerHelper = Substitute.For<ILoggerHelper>();

            var httpPostFunction = new LearningProgressionPatchTrigger(
                ResponseMessageHelper,
                RequestHelper,
                LearningProgressionPatchTriggerService,
                JsonHelper,
                ResourceHelper,

                Valdiator,
                LoggerHelper
                );

            // Act
            var response = await httpPostFunction.Run(TestFactory.CreateHttpRequest("", ""), TestFactory.CreateLogger(), CustomerId, LearningProgressionId);

            //Assert
            Assert.True(response.StatusCode == HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Get_WhenGetDssApimUrlGetDssApimUrlIsEMpty_ReturnBadRequest()
        {
            // arrange

            var ResponseMessageHelper = new HttpResponseMessageHelper();
            var RequestHelper = Substitute.For<IHttpRequestHelper>();
            RequestHelper.GetDssTouchpointId(Arg.Any<HttpRequest>()).Returns<string>("0000000001");

            var LearningProgressionPatchTriggerService = Substitute.For<ILearningProgressionPatchTriggerService>();
            LearningProgressionPatchTriggerService.PatchLearningProgressionAsync(Arg.Any<string>(), Arg.Any<Models.LearningProgressionPatch>()).Returns<string>("AString");

            var JsonHelper = new JsonHelper();
            var ResourceHelper = Substitute.For<IResourceHelper>();
            var Valdiator = Substitute.For<IValidate>();

            var LoggerHelper = Substitute.For<ILoggerHelper>();

            var httpPostFunction = new LearningProgressionPatchTrigger(

                ResponseMessageHelper,
                RequestHelper,
                LearningProgressionPatchTriggerService,
                JsonHelper,
                ResourceHelper,

                Valdiator,
                LoggerHelper
                );

            // Act
            var response = await httpPostFunction.Run(TestFactory.CreateHttpRequest("", ""), TestFactory.CreateLogger(), CustomerId, LearningProgressionId);

            //Assert
            Assert.True(response.StatusCode == HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Get_CustomerIdIsNotValidGuid_ReturnBadRequest()
        {
            // arrange

            var ResponseMessageHelper = new HttpResponseMessageHelper();
            var RequestHelper = Substitute.For<IHttpRequestHelper>();
            RequestHelper.GetDssTouchpointId(Arg.Any<HttpRequest>()).Returns<string>("0000000001");
            RequestHelper.GetDssApimUrl(Arg.Any<HttpRequest>()).Returns<string>("http://aurlvalue.com");

            var LearningProgressionPatchTriggerService = Substitute.For<ILearningProgressionPatchTriggerService>();
            LearningProgressionPatchTriggerService.PatchLearningProgressionAsync(Arg.Any<string>(), Arg.Any<Models.LearningProgressionPatch>()).Returns<string>("AString");

            var JsonHelper = new JsonHelper();
            var ResourceHelper = Substitute.For<IResourceHelper>();
            var Valdiator = Substitute.For<IValidate>();

            var LoggerHelper = Substitute.For<ILoggerHelper>();

            var httpPostFunction = new LearningProgressionPatchTrigger(

                ResponseMessageHelper,
                RequestHelper,
                LearningProgressionPatchTriggerService,
                JsonHelper,
                ResourceHelper,

                Valdiator,
                LoggerHelper
                );

            // Act
            var response = await httpPostFunction.Run(TestFactory.CreateHttpRequest("", ""), TestFactory.CreateLogger(), InvalidCustomerId, LearningProgressionId);

            //Assert
            Assert.True(response.StatusCode == HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Get_LearningProgressionIdIsNotValidGuid_ReturnBadRequest()
        {
            // arrange

            var ResponseMessageHelper = new HttpResponseMessageHelper();
            var RequestHelper = Substitute.For<IHttpRequestHelper>();
            RequestHelper.GetDssTouchpointId(Arg.Any<HttpRequest>()).Returns<string>("0000000001");
            RequestHelper.GetDssApimUrl(Arg.Any<HttpRequest>()).Returns<string>("http://aurlvalue.com");

            var LearningProgressionPatchTriggerService = Substitute.For<ILearningProgressionPatchTriggerService>();
            LearningProgressionPatchTriggerService.PatchLearningProgressionAsync(Arg.Any<string>(), Arg.Any<Models.LearningProgressionPatch>()).Returns<string>("AString");

            var JsonHelper = new JsonHelper();
            var ResourceHelper = Substitute.For<IResourceHelper>();
            var Valdiator = Substitute.For<IValidate>();

            var LoggerHelper = Substitute.For<ILoggerHelper>();

            var httpPostFunction = new LearningProgressionPatchTrigger(

                ResponseMessageHelper,
                RequestHelper,
                LearningProgressionPatchTriggerService,
                JsonHelper,
                ResourceHelper,

                Valdiator,
                LoggerHelper
                );

            // Act
            var response = await httpPostFunction.Run(TestFactory.CreateHttpRequest("", ""), TestFactory.CreateLogger(), CustomerId, InvalidLearningProgressionId);

            //Assert
            Assert.True(response.StatusCode == HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Get_InvalidBody_ReturnBadRequest()
        {
            // arrange

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

            var LoggerHelper = Substitute.For<ILoggerHelper>();

            var httpPostFunction = new LearningProgressionPatchTrigger(

                ResponseMessageHelper,
                RequestHelper,
                LearningProgressionPatchTriggerService,
                JsonHelper,
                ResourceHelper,

                Valdiator,
                LoggerHelper
                );

            // Act
            var response = await httpPostFunction.Run(TestFactory.CreateHttpRequest("", ""), TestFactory.CreateLogger(), CustomerId, LearningProgressionId);

            //Assert
            Assert.True(response.StatusCode == HttpStatusCode.NoContent);
        }

        [Fact]
        public async Task Get_ReadOnlyCustomer_ReturnBadRequest()
        {
            // arrange

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

            var LoggerHelper = Substitute.For<ILoggerHelper>();

            var httpPostFunction = new LearningProgressionPatchTrigger(

                ResponseMessageHelper,
                RequestHelper,
                LearningProgressionPatchTriggerService,
                JsonHelper,
                ResourceHelper,

                Valdiator,
                LoggerHelper
                );

            // Act
            var response = await httpPostFunction.Run(TestFactory.CreateHttpRequest("", ""), TestFactory.CreateLogger(), CustomerId, LearningProgressionId);

            //Assert
            Assert.True(response.StatusCode == HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task Get_CustomerIdIsValidGuidButCustomerDoesNotExist_ReturnBadRequest()
        {
            // arrange

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


            var LoggerHelper = Substitute.For<ILoggerHelper>();

            var httpPostFunction = new LearningProgressionPatchTrigger(

                ResponseMessageHelper,
                RequestHelper,
                LearningProgressionPatchTriggerService,
                JsonHelper,
                ResourceHelper,

                Valdiator,
                LoggerHelper
                );

            // Act
            var response = await httpPostFunction.Run(TestFactory.CreateHttpRequest("", ""), TestFactory.CreateLogger(), CustomerId, LearningProgressionId);

            //Assert
            Assert.True(response.StatusCode == HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Get_LearningProvideDoesNotExistForCustomer_ReturnBadRequest()
        {
            // arrange

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

            var LoggerHelper = Substitute.For<ILoggerHelper>();

            var httpPostFunction = new LearningProgressionPatchTrigger(

                ResponseMessageHelper,
                RequestHelper,
                LearningProgressionPatchTriggerService,
                JsonHelper,
                ResourceHelper,

                Valdiator,
                LoggerHelper
                );

            // Act
            var response = await httpPostFunction.Run(TestFactory.CreateHttpRequest("", ""), TestFactory.CreateLogger(), CustomerId, LearningProgressionId);

            //Assert
            Assert.True(response.StatusCode == HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Get_NoLearningProgressionPatchData_ReturnNoContent()
        {
            // arrange

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

            var LoggerHelper = Substitute.For<ILoggerHelper>();

            var httpPostFunction = new LearningProgressionPatchTrigger(
                ResponseMessageHelper,
                RequestHelper,
                LearningProgressionPatchTriggerService,
                JsonHelper,
                ResourceHelper,
                Valdiator,
                LoggerHelper
                );

            // Act
            var response = await httpPostFunction.Run(TestFactory.CreateHttpRequest("", ""), TestFactory.CreateLogger(), CustomerId, LearningProgressionId);

            //Assert
            Assert.True(response.StatusCode == HttpStatusCode.NoContent);
        }

        [Fact]
        public async Task Get_ValidationFailed_ReturnUnprocessableEntity()
        {
            // arrange

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
            Valdiator.ValidateResource(Arg.Any<Models.LearningProgressionPatch>()).Returns(ErrorResults);


            var LoggerHelper = Substitute.For<ILoggerHelper>();

            var httpPostFunction = new LearningProgressionPatchTrigger(

                ResponseMessageHelper,
                RequestHelper,
                LearningProgressionPatchTriggerService,
                JsonHelper,
                ResourceHelper,

                Valdiator,
                LoggerHelper
                );

            // Act
            var response = await httpPostFunction.Run(TestFactory.CreateHttpRequest("", ""), TestFactory.CreateLogger(), CustomerId, LearningProgressionId);

            //Assert
            Assert.True(response.StatusCode == HttpStatusCode.UnprocessableEntity);
        }

        [Fact]
        public async Task Get_FailedGetRequest_ReturnNoContent()
        {
            // arrange            
            var ResponseMessageHelper = new HttpResponseMessageHelper();
            var RequestHelper = Substitute.For<IHttpRequestHelper>();
            RequestHelper.GetDssTouchpointId(Arg.Any<HttpRequest>()).Returns<string>("0000000001");
            RequestHelper.GetDssApimUrl(Arg.Any<HttpRequest>()).Returns<string>("http://aurlvalue.com");
            RequestHelper.GetResourceFromRequest<Models.LearningProgressionPatch>(Arg.Any<HttpRequest>()).Returns(new Models.LearningProgressionPatch());

            var learningProgressionPatch = new Models.LearningProgressionPatch();
            RequestHelper.GetResourceFromRequest<Models.LearningProgressionPatch>(Arg.Any<HttpRequest>()).Returns<Models.LearningProgressionPatch>(learningProgressionPatch);

            var patch = new LearningProgressionPatch();

            var LearningProgressionPatchTriggerService = Substitute.For<ILearningProgressionPatchTriggerService>();
            LearningProgressionPatchTriggerService.PatchLearningProgressionAsync(Arg.Any<string>(), Arg.Any<Models.LearningProgressionPatch>()).Returns<string>(JsonConvert.SerializeObject(patch));
            LearningProgressionPatchTriggerService.DoesLearningProgressionExistForCustomer(Arg.Any<Guid>()).Returns<bool>(true);

            var JsonHelper = new JsonHelper();
            var ResourceHelper = Substitute.For<IResourceHelper>();
            ResourceHelper.IsCustomerReadOnly(Arg.Any<Guid>()).Returns<bool>(false);
            ResourceHelper.DoesCustomerExist(Arg.Any<Guid>()).Returns<bool>(true);

            var Valdiator = Substitute.For<IValidate>();
            List<ValidationResult> ErrorResults = new List<ValidationResult>();
            Valdiator.ValidateResource(Arg.Any<Models.LearningProgression>()).Returns(ErrorResults);

            var LoggerHelper = Substitute.For<ILoggerHelper>();

            var httpPostFunction = new LearningProgressionPatchTrigger(

                ResponseMessageHelper,
                RequestHelper,
                LearningProgressionPatchTriggerService,
                JsonHelper,
                ResourceHelper,

                Valdiator,
                LoggerHelper
                );

            // Act
            var response = await httpPostFunction.Run(TestFactory.CreateHttpRequest("", ""), TestFactory.CreateLogger(), CustomerId, LearningProgressionId);

            //Assert
            Assert.True(response.StatusCode == HttpStatusCode.NoContent);
        }

        [Fact]
        public async Task Get_SuccessfulRequest_ReturnOk()
        {
            // arrange            
            var ResponseMessageHelper = new HttpResponseMessageHelper();
            var RequestHelper = Substitute.For<IHttpRequestHelper>();
            RequestHelper.GetDssTouchpointId(Arg.Any<HttpRequest>()).Returns<string>("0000000001");
            RequestHelper.GetDssApimUrl(Arg.Any<HttpRequest>()).Returns<string>("http://aurlvalue.com");

            var patchRequestBody = new Models.LearningProgressionPatch
            {
                LearningProgressionId = Guid.NewGuid(),
                CustomerId = Guid.NewGuid(),
                CurrentQualificationLevel = QualificationLevel.Level3,
                DateLearningStarted = new DateTime(2018, 01, 01),
                LearningHours = LearningHours.LessThanSixteenHours,
                DateProgressionRecorded = new DateTime(2018, 01, 01),
                CurrentLearningStatus = CurrentLearningStatus.PreferNotToSay,
                DateQualificationLevelAchieved = new DateTime(2019, 07, 30),
                LastModifiedTouchpointId = "0000000001",
                LastModifiedDate = new DateTime(),
                LastLearningProvidersUKPRN = "LastLearningProviders-UKPRN",
                CreatedBy = "Created-By"
            };

            RequestHelper.GetResourceFromRequest<Models.LearningProgressionPatch>(Arg.Any<HttpRequest>()).Returns(patchRequestBody);

            var LearningProgressionPatchTriggerService = Substitute.For<ILearningProgressionPatchTriggerService>();
            LearningProgressionPatchTriggerService.PatchLearningProgressionAsync(Arg.Any<string>(), Arg.Any<Models.LearningProgressionPatch>()).Returns<string>(JsonConvert.SerializeObject(new LearningProgression.Models.LearningProgression()));
            LearningProgressionPatchTriggerService.DoesLearningProgressionExistForCustomer(Arg.Any<Guid>()).Returns<bool>(true);
            LearningProgressionPatchTriggerService.UpdateCosmosAsync(Arg.Any<string>(), Arg.Any<Guid>()).Returns<Models.LearningProgression>(new Models.LearningProgression());            

            var JsonHelper = new JsonHelper();
            var ResourceHelper = Substitute.For<IResourceHelper>();
            ResourceHelper.IsCustomerReadOnly(Arg.Any<Guid>()).Returns<bool>(false);
            ResourceHelper.DoesCustomerExist(Arg.Any<Guid>()).Returns<bool>(true);

            var Valdiator = Substitute.For<IValidate>();
            List<ValidationResult> ErrorResults = new List<ValidationResult>();
            Valdiator.ValidateResource(Arg.Any<Models.LearningProgression>()).Returns(ErrorResults);

            var LoggerHelper = Substitute.For<ILoggerHelper>();

            var httpPostFunction = new LearningProgressionPatchTrigger(
                ResponseMessageHelper,
                RequestHelper,
                LearningProgressionPatchTriggerService,
                JsonHelper,
                ResourceHelper,
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
