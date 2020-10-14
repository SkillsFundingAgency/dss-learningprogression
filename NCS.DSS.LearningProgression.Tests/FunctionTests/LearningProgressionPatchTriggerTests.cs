﻿using DFC.Common.Standard.Logging;
using DFC.HTTP.Standard;
using DFC.JSON.Standard;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.Extensions.Logging;
using Moq;
using NCS.DSS.Contact.Cosmos.Helper;
using NCS.DSS.LearningProgression.Models;
using NCS.DSS.LearningProgression.PatchLearningProgression.Service;
using NCS.DSS.LearningProgression.Validators;
using Newtonsoft.Json;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace NCS.DSS.LearningProgression.Tests.FunctionTests
{
    [TestFixture]
    public class LearningProgressionPatchTriggerTests
    {
        const string CustomerId = "844a6215-8413-41ba-96b0-b4cc7041ca33";
        const string InvalidCustomerId = "InvalidCustomerId";
        const string LearningProgressionId = "844a6215-8413-41ba-96b0-b4cc7041ca33";
        const string InvalidLearningProgressionId = "InvalidLearningProgressionId";
        private Mock<ILogger> _log;
        private DefaultHttpRequest _request;
        private Mock<IResourceHelper> _resourceHelper;
        private Mock<IHttpRequestHelper> _httpRequestMessageHelper;
        private Mock<ILearningProgressionPatchTriggerService> _learningProgressionPatchTriggerService;
        private IHttpResponseMessageHelper _httpResponseMessageHelper;
        private IJsonHelper _jsonHelper;
        private LearningProgressionPatchTrigger _function;
        private Mock<ILoggerHelper> _loggerHelper;
        private IValidate _validate;


        [SetUp]
        public void Setup()
        {
            _log = new Mock<ILogger>();
            _resourceHelper = new Mock<IResourceHelper>();
            _loggerHelper = new Mock<ILoggerHelper>();
            _httpRequestMessageHelper = new Mock<IHttpRequestHelper>();
            _httpResponseMessageHelper = new HttpResponseMessageHelper();
            _learningProgressionPatchTriggerService = new Mock<ILearningProgressionPatchTriggerService>();
            _jsonHelper = new JsonHelper();
            _validate = new Validate();
            _function = new LearningProgressionPatchTrigger(
                _httpResponseMessageHelper, 
                _httpRequestMessageHelper.Object, 
                _learningProgressionPatchTriggerService.Object,
                _jsonHelper, 
                _resourceHelper.Object, 
                _validate, 
                _loggerHelper.Object);
        }

        [Test]
        public async Task Get_WhenTouchPointHeaderIsMission_ReturnBadRequest()
        {
            // Arrange

            // Act
            var response = await RunFunction(CustomerId, LearningProgressionId);

            //Assert
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Test]
        public async Task Get_WhenGetDssApimUrlGetDssApimUrlIsEMpty_ReturnBadRequest()
        {
            // arrange
            _httpRequestMessageHelper.Setup(x=>x.GetDssTouchpointId(It.IsAny<HttpRequest>())).Returns("0000000001");

            // Act
            var response = await RunFunction(CustomerId, LearningProgressionId);

            //Assert
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Test]
        public async Task Get_CustomerIdIsNotValidGuid_ReturnBadRequest()
        {
            // arrange
            _httpRequestMessageHelper.Setup(x=>x.GetDssTouchpointId(It.IsAny<HttpRequest>())).Returns("0000000001");
            _httpRequestMessageHelper.Setup(x => x.GetDssApimUrl(It.IsAny<HttpRequest>())).Returns("http://aurlvalue.com");

            // Act
            var response = await RunFunction(InvalidCustomerId, LearningProgressionId);

            //Assert
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Test]
        public async Task Get_LearningProgressionIdIsNotValidGuid_ReturnBadRequest()
        {
            // arrange
            _httpRequestMessageHelper.Setup(x=>x.GetDssTouchpointId(It.IsAny<HttpRequest>())).Returns("0000000001");
            _httpRequestMessageHelper.Setup(x=>x.GetDssApimUrl(It.IsAny<HttpRequest>())).Returns("http://aurlvalue.com");

            // Act
            var response = await RunFunction(CustomerId, InvalidLearningProgressionId);

            //Assert
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Test]
        public async Task Get_InvalidBody_ReturnBadRequest()
        {
            // arrange
            _httpRequestMessageHelper.Setup(x=>x.GetDssTouchpointId(It.IsAny<HttpRequest>())).Returns("0000000001");
            _httpRequestMessageHelper.Setup(x=>x.GetDssApimUrl(It.IsAny<HttpRequest>())).Returns("http://aurlvalue.com");
            _httpRequestMessageHelper.Setup(x=>x.GetResourceFromRequest<Models.LearningProgressionPatch>(It.IsAny<HttpRequest>())).Returns(Task.FromResult<Models.LearningProgressionPatch>(null));
            _learningProgressionPatchTriggerService.Setup(x=>x.PatchLearningProgressionAsync(It.IsAny<string>(), It.IsAny<Models.LearningProgressionPatch>())).Returns("AString");

            // Act
            var response = await RunFunction(CustomerId, LearningProgressionId);

            // Assert
            Assert.AreEqual(HttpStatusCode.NoContent, response.StatusCode);
        }

        [Test]
        public async Task Get_ReadOnlyCustomer_ReturnBadRequest()
        {
            // arrange
            _httpRequestMessageHelper.Setup(x=>x.GetDssTouchpointId(It.IsAny<HttpRequest>())).Returns("0000000001");
            _httpRequestMessageHelper.Setup(x => x.GetDssApimUrl(It.IsAny<HttpRequest>())).Returns("http://aurlvalue.com");
            _httpRequestMessageHelper.Setup(x => x.GetResourceFromRequest<Models.LearningProgressionPatch>(It.IsAny<HttpRequest>())).Returns(Task.FromResult(new Models.LearningProgressionPatch()));
            _learningProgressionPatchTriggerService.Setup(x=>x.PatchLearningProgressionAsync(It.IsAny<string>(), It.IsAny<Models.LearningProgressionPatch>())).Returns("AString");
            _resourceHelper.Setup(x=>x.IsCustomerReadOnly(It.IsAny<Guid>())).Returns(Task.FromResult(true));
            _resourceHelper.Setup(x=>x.DoesCustomerExist(It.IsAny<Guid>())).Returns(Task.FromResult(true));

            // Act
            var response = await RunFunction(CustomerId, LearningProgressionId);

            // Assert
            Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Test]
        public async Task Get_CustomerIdIsValidGuidButCustomerDoesNotExist_ReturnBadRequest()
        {
            // arrange
            _httpRequestMessageHelper.Setup(x => x.GetDssTouchpointId(It.IsAny<HttpRequest>())).Returns("0000000001");
            _httpRequestMessageHelper.Setup(x => x.GetDssApimUrl(It.IsAny<HttpRequest>())).Returns("http://aurlvalue.com");
            _httpRequestMessageHelper.Setup(x => x.GetResourceFromRequest<Models.LearningProgressionPatch>(It.IsAny<HttpRequest>())).Returns(Task.FromResult(new Models.LearningProgressionPatch()));
            _learningProgressionPatchTriggerService.Setup(x => x.PatchLearningProgressionAsync(It.IsAny<string>(), It.IsAny<Models.LearningProgressionPatch>())).Returns("AString");
            _resourceHelper.Setup(x => x.IsCustomerReadOnly(It.IsAny<Guid>())).Returns(Task.FromResult(false));
            _resourceHelper.Setup(x => x.DoesCustomerExist(It.IsAny<Guid>())).Returns(Task.FromResult(false));

            // Act
            var response = await RunFunction(CustomerId, LearningProgressionId);

            //Assert
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        }


        [Test]
        public async Task Get_NoLearningProgressionPatchData_ReturnNoContent()
        {
            // arrange
            _httpRequestMessageHelper.Setup(x => x.GetDssTouchpointId(It.IsAny<HttpRequest>())).Returns("0000000001");
            _httpRequestMessageHelper.Setup(x => x.GetDssApimUrl(It.IsAny<HttpRequest>())).Returns("http://aurlvalue.com");
            _httpRequestMessageHelper.Setup(x => x.GetResourceFromRequest<Models.LearningProgressionPatch>(It.IsAny<HttpRequest>())).Returns(Task.FromResult(new Models.LearningProgressionPatch()));
            _learningProgressionPatchTriggerService.Setup(x => x.PatchLearningProgressionAsync(It.IsAny<string>(), It.IsAny<Models.LearningProgressionPatch>())).Returns("AString");
            _learningProgressionPatchTriggerService.Setup(x=>x.DoesLearningProgressionExistForCustomer(It.IsAny<Guid>())).Returns(true);
            _learningProgressionPatchTriggerService.Setup(x=>x.GetLearningProgressionForCustomerToPatchAsync(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(Task.FromResult((string)null));
            _resourceHelper.Setup(x => x.DoesCustomerExist(It.IsAny<Guid>())).Returns(Task.FromResult(true));

            // Act
            var response = await RunFunction(CustomerId, LearningProgressionId);

            //Assert
            Assert.AreEqual(HttpStatusCode.NoContent, response.StatusCode);
        }

        [Test]
        public async Task Get_ValidationFailed_ReturnUnprocessableEntity()
        {
            // arrange
            _httpRequestMessageHelper.Setup(x => x.GetDssTouchpointId(It.IsAny<HttpRequest>())).Returns("0000000001");
            _httpRequestMessageHelper.Setup(x => x.GetDssApimUrl(It.IsAny<HttpRequest>())).Returns("http://aurlvalue.com");
            _httpRequestMessageHelper.Setup(x => x.GetResourceFromRequest<Models.LearningProgressionPatch>(It.IsAny<HttpRequest>())).Returns(Task.FromResult(new Models.LearningProgressionPatch()));
            _learningProgressionPatchTriggerService.Setup(x => x.DoesLearningProgressionExistForCustomer(It.IsAny<Guid>())).Returns(true);
            _learningProgressionPatchTriggerService.Setup(x => x.PatchLearningProgressionAsync(It.IsAny<string>(), It.IsAny<Models.LearningProgressionPatch>())).Returns(JsonConvert.SerializeObject(new LearningProgression.Models.LearningProgression() { CustomerId = new Guid(CustomerId) }));
            _resourceHelper.Setup(x => x.IsCustomerReadOnly(It.IsAny<Guid>())).Returns(Task.FromResult(false));
            _resourceHelper.Setup(x => x.DoesCustomerExist(It.IsAny<Guid>())).Returns(Task.FromResult(true));
            _learningProgressionPatchTriggerService.Setup(x => x.GetLearningProgressionForCustomerToPatchAsync(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(Task.FromResult("some string"));
            var validate = new Mock<IValidate>();
            var ErrorResults = new List<ValidationResult>();
            var validationResult = new ValidationResult("Error MEssage");
            ErrorResults.Add(validationResult);
            validate.Setup(x=>x.ValidateResource(It.IsAny<Models.LearningProgressionPatch>())).Returns(ErrorResults);
            _function = new LearningProgressionPatchTrigger(
                _httpResponseMessageHelper,
                _httpRequestMessageHelper.Object,
                _learningProgressionPatchTriggerService.Object,
                _jsonHelper,
                _resourceHelper.Object,
                validate.Object,
                _loggerHelper.Object);

            // Act
            var response = await RunFunction(CustomerId, LearningProgressionId);

            //Assert
            Assert.AreEqual(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        }

        [Test]
        public async Task Get_FailedGetRequest_ReturnNoContent()
        {
            // arrange            
            _httpRequestMessageHelper.Setup(x => x.GetDssTouchpointId(It.IsAny<HttpRequest>())).Returns("0000000001");
            _httpRequestMessageHelper.Setup(x => x.GetDssApimUrl(It.IsAny<HttpRequest>())).Returns("http://aurlvalue.com");
            _httpRequestMessageHelper.Setup(x => x.GetResourceFromRequest<Models.LearningProgressionPatch>(It.IsAny<HttpRequest>())).Returns(Task.FromResult(new Models.LearningProgressionPatch()));
            var patch = new LearningProgressionPatch();
            _learningProgressionPatchTriggerService.Setup(x=>x.PatchLearningProgressionAsync(It.IsAny<string>(), It.IsAny<Models.LearningProgressionPatch>())).Returns(JsonConvert.SerializeObject(patch));
            _learningProgressionPatchTriggerService.Setup(x=>x.DoesLearningProgressionExistForCustomer(It.IsAny<Guid>())).Returns(true);
            _resourceHelper.Setup(x => x.IsCustomerReadOnly(It.IsAny<Guid>())).Returns(Task.FromResult(false));
            _resourceHelper.Setup(x => x.DoesCustomerExist(It.IsAny<Guid>())).Returns(Task.FromResult(true));

            // Act
            var response = await RunFunction(CustomerId, LearningProgressionId);

            //Assert
            Assert.AreEqual(HttpStatusCode.NoContent, response.StatusCode);
        }

        [Test]
        public async Task Get_SuccessfulRequest_ReturnOk()
        {
            // arrange                     
            _httpRequestMessageHelper.Setup(x => x.GetDssTouchpointId(It.IsAny<HttpRequest>())).Returns("0000000001");
            _httpRequestMessageHelper.Setup(x => x.GetDssApimUrl(It.IsAny<HttpRequest>())).Returns("http://aurlvalue.com");
            _httpRequestMessageHelper.Setup(x => x.GetResourceFromRequest<Models.LearningProgressionPatch>(It.IsAny<HttpRequest>())).Returns(Task.FromResult(new Models.LearningProgressionPatch() { CustomerId = new Guid(CustomerId) }));
            _learningProgressionPatchTriggerService.Setup(x=>x.PatchLearningProgressionAsync(It.IsAny<string>(), It.IsAny<Models.LearningProgressionPatch>())).Returns(JsonConvert.SerializeObject(new LearningProgression.Models.LearningProgression() { CustomerId = new Guid(CustomerId) }));
            _learningProgressionPatchTriggerService.Setup(x=>x.DoesLearningProgressionExistForCustomer(It.IsAny<Guid>())).Returns(true);
            _learningProgressionPatchTriggerService.Setup(x=>x.UpdateCosmosAsync(It.IsAny<string>(), It.IsAny<Guid>())).Returns(Task.FromResult(new Models.LearningProgression()));
            _resourceHelper.Setup(x => x.IsCustomerReadOnly(It.IsAny<Guid>())).Returns(Task.FromResult(false));
            _resourceHelper.Setup(x => x.DoesCustomerExist(It.IsAny<Guid>())).Returns(Task.FromResult(true));
            _learningProgressionPatchTriggerService.Setup(x => x.GetLearningProgressionForCustomerToPatchAsync(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(Task.FromResult("some string"));

            // Act
            var response = await RunFunction(CustomerId, LearningProgressionId);

            //Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }

        private async Task<HttpResponseMessage> RunFunction(string customerId, string learningProgressionId)
        {
            return await _function.Run(_request, _log.Object, customerId, learningProgressionId).ConfigureAwait(false);
        }
    }
}
