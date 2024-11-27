using DFC.HTTP.Standard;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using NCS.DSS.LearningProgression.Cosmos.Helper;
using NCS.DSS.LearningProgression.GetLearningProgressionById.Function;
using NCS.DSS.LearningProgression.GetLearningProgressionById.Service;
using NUnit.Framework;
using System.Net;

namespace NCS.DSS.LearningProgression.Tests.FunctionTests
{
    [TestFixture]
    public class LearningProgressionGetByIdTriggerTests
    {
        private const string ValidCustomerId = "7E467BDB-213F-407A-B86A-1954053D3C24";
        private const string ValidLearningProgressionId = "1e1a555c-9633-4e12-ab28-09ed60d51cb3";
        private const string InvalidCustomerId = "2323232";

        private Mock<ILogger<LearningProgressionGetByIdTrigger>> _logger;
        private HttpRequest _request;
        private Mock<IResourceHelper> _resourceHelper;
        private Mock<IHttpRequestHelper> _httpRequestMessageHelper;
        private Mock<ILearningProgressionGetByIdService> _learningProgressionGetByIdService;
        
        private LearningProgressionGetByIdTrigger _function;

        [SetUp]
        public void Setup()
        {
            _httpRequestMessageHelper = new Mock<IHttpRequestHelper>();
            _learningProgressionGetByIdService = new Mock<ILearningProgressionGetByIdService>();
            _resourceHelper = new Mock<IResourceHelper>();
            _logger = new Mock<ILogger<LearningProgressionGetByIdTrigger>>();
            _function = new LearningProgressionGetByIdTrigger(
                _learningProgressionGetByIdService.Object,
                _httpRequestMessageHelper.Object,
                _resourceHelper.Object,
                _logger.Object);

            _request = new DefaultHttpContext().Request;
        }

        [Test]
        public async Task Get_WhenTouchPointHeaderIsMission_ReturnBadRequest()
        {
            // Act
            var response = await RunFunction(ValidCustomerId, ValidLearningProgressionId);

            //Assert
            Assert.That(response, Is.InstanceOf<BadRequestResult>());
        }

        [Test]
        public async Task Get_WhenGetDssApimUrlGetDssApimUrlIsEmpty_ReturnBadRequest()
        {
            // Arrange
            _httpRequestMessageHelper.Setup(x => x.GetDssTouchpointId(It.IsAny<HttpRequest>())).Returns("0000000001");

            // Act
            var response = await RunFunction(ValidCustomerId, ValidLearningProgressionId);

            //Assert
            Assert.That(response, Is.InstanceOf<BadRequestResult>());
        }

        [Test]
        public async Task Get_CustomerIdIsNotValidGuid_ReturnBadRequest()
        {
            // arrange
            _httpRequestMessageHelper.Setup(x => x.GetDssTouchpointId(It.IsAny<HttpRequest>())).Returns("0000000001");
            _httpRequestMessageHelper.Setup(x => x.GetDssApimUrl(It.IsAny<HttpRequest>())).Returns("http://aurlvalue.com");
            _learningProgressionGetByIdService.Setup(x => x.GetLearningProgressionForCustomerAsync(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(Task.FromResult(new LearningProgression.Models.LearningProgression()));

            // Act
            var response = await RunFunction(InvalidCustomerId, ValidLearningProgressionId);

            //Assert
            Assert.That(response, Is.InstanceOf<BadRequestObjectResult>());
        }

        [Test]
        public async Task Get_CustomerIdIsValidGuidButCustomerDoesNotExist_ReturnBadRequest()
        {
            // arrange
            _httpRequestMessageHelper.Setup(x => x.GetDssTouchpointId(It.IsAny<HttpRequest>())).Returns("0000000001");
            _httpRequestMessageHelper.Setup(x => x.GetDssApimUrl(It.IsAny<HttpRequest>())).Returns("http://aurlvalue.com");
            _learningProgressionGetByIdService.Setup(x => x.GetLearningProgressionForCustomerAsync(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(Task.FromResult(new LearningProgression.Models.LearningProgression()));
            _resourceHelper.Setup(x => x.DoesCustomerExist(It.IsAny<Guid>())).Returns(Task.FromResult(false));

            // Act
            var response = await RunFunction(ValidCustomerId, ValidLearningProgressionId);

            //Assert
            Assert.That(response, Is.InstanceOf<BadRequestResult>());
        }

        [Test]
        public async Task Get_RequestContainsNoErrors_ReturnOk()
        {
            // arrange
            _httpRequestMessageHelper.Setup(x => x.GetDssTouchpointId(It.IsAny<HttpRequest>())).Returns("0000000001");
            _httpRequestMessageHelper.Setup(x => x.GetDssApimUrl(It.IsAny<HttpRequest>())).Returns("http://aurlvalue.com");
            _learningProgressionGetByIdService.Setup(x => x.GetLearningProgressionForCustomerAsync(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(Task.FromResult(new LearningProgression.Models.LearningProgression()));
            _resourceHelper.Setup(x => x.DoesCustomerExist(It.IsAny<Guid>())).Returns(Task.FromResult(true));

            // Act
            var response = await RunFunction(ValidCustomerId, ValidLearningProgressionId);
            var responseResult = response as JsonResult;

            //Assert
            Assert.That(response, Is.InstanceOf<JsonResult>());
            Assert.That(responseResult.StatusCode, Is.EqualTo((int)HttpStatusCode.OK));
        }

        private async Task<IActionResult> RunFunction(string customerId, string learningProgressionId)
        {
            return await _function.Run(_request, customerId, learningProgressionId).ConfigureAwait(false);
        }
    }
}
