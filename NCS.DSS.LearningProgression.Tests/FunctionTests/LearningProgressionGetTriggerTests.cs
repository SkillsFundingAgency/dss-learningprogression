using DFC.HTTP.Standard;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using NCS.DSS.Contact.Cosmos.Helper;
using NCS.DSS.LearningProgression.GetLearningProgression.Function;
using NCS.DSS.LearningProgression.GetLearningProgression.Service;
using NUnit.Framework;
using System.Net;

namespace NCS.DSS.LearningProgression.Tests.FunctionTests
{
    [TestFixture]
    public class LearningProgressionGetTriggerTests
    {
        private Mock<ILogger<LearningProgressionsGetTrigger>> _logger;
        private HttpRequest _request;
        private Mock<IResourceHelper> _resourceHelper;
        private Mock<IHttpRequestHelper> _httpRequestMessageHelper;
        private Mock<ILearningProgressionsGetTriggerService> _learningProgressionGetByIdService;
        private LearningProgressionsGetTrigger _function;
        private const string ValidCustomerId = "7E467BDB-213F-407A-B86A-1954053D3C24";
        private const string InvalidCustomerId = "2323232";

        [SetUp]
        public void Setup()
        {
            _httpRequestMessageHelper = new Mock<IHttpRequestHelper>();
            _learningProgressionGetByIdService = new Mock<ILearningProgressionsGetTriggerService>();
            _resourceHelper = new Mock<IResourceHelper>();
            _logger = new Mock<ILogger<LearningProgressionsGetTrigger>>();
            _function = new LearningProgressionsGetTrigger(
                _httpRequestMessageHelper.Object,
                _learningProgressionGetByIdService.Object,
                _resourceHelper.Object,
                _logger.Object);

            _request = new DefaultHttpContext().Request;
        }

        [Test]
        public async Task Get_WhenTouchPointHeaderIsMission_ReturnBadRequest()
        {
            // arrange

            // Act
            var response = await RunFunction(ValidCustomerId);

            //Assert
            Assert.That(response, Is.InstanceOf<BadRequestResult>());
        }

        [Test]
        public async Task Get_WhenGetDssApimUrlGetDssApimUrlIsEMpty_ReturnBadRequest()
        {
            // arrange
            _httpRequestMessageHelper.Setup(x => x.GetDssTouchpointId(It.IsAny<HttpRequest>())).Returns("0000000001");

            // Act
            var response = await RunFunction(ValidCustomerId);

            //Assert
            Assert.That(response, Is.InstanceOf<BadRequestResult>());
        }

        [Test]
        public async Task Get_CustomerIdIsNotValidGuid_ReturnBadRequest()
        {
            // arrange
            _httpRequestMessageHelper.Setup(x => x.GetDssTouchpointId(It.IsAny<HttpRequest>())).Returns("0000000001");
            _httpRequestMessageHelper.Setup(x => x.GetDssApimUrl(It.IsAny<HttpRequest>())).Returns("http://aurlvalue.com");

            // Act
            var response = await RunFunction(InvalidCustomerId);

            //Assert
            Assert.That(response, Is.InstanceOf<BadRequestObjectResult>());
        }

        [Test]
        public async Task Get_CustomerIdIsValidGuidButCustomerDoesNotExist_ReturnBadRequest()
        {
            // arrange
            _httpRequestMessageHelper.Setup(x => x.GetDssTouchpointId(It.IsAny<HttpRequest>())).Returns("0000000001");
            _httpRequestMessageHelper.Setup(x => x.GetDssApimUrl(It.IsAny<HttpRequest>())).Returns("http://aurlvalue.com");
            _resourceHelper.Setup(x => x.DoesCustomerExist(It.IsAny<Guid>())).Returns(Task.FromResult(false));

            // Act
            var response = await RunFunction(ValidCustomerId);

            //Assert
            Assert.That(response, Is.InstanceOf<BadRequestResult>());
        }

        [Test]
        public async Task Get_RequestContainsNoErrors_ReturnOk()
        {
            // arrange
            _httpRequestMessageHelper.Setup(x => x.GetDssTouchpointId(It.IsAny<HttpRequest>())).Returns("0000000001");
            _httpRequestMessageHelper.Setup(x => x.GetDssApimUrl(It.IsAny<HttpRequest>())).Returns("http://aurlvalue.com");
            _learningProgressionGetByIdService.Setup(x => x.GetLearningProgressionsForCustomerAsync(It.IsAny<Guid>())).Returns(Task.FromResult(new List<LearningProgression.Models.LearningProgression>()));

            _resourceHelper.Setup(x => x.DoesCustomerExist(It.IsAny<Guid>())).Returns(Task.FromResult(true));

            // Act
            var response = await RunFunction(ValidCustomerId);
            var responseResult = response as JsonResult;

            //Assert
            Assert.That(response, Is.InstanceOf<JsonResult>());
            Assert.That(responseResult.StatusCode, Is.EqualTo((int)HttpStatusCode.OK));
        }

        private async Task<IActionResult> RunFunction(string customerId)
        {
            return await _function.Run(_request, customerId).ConfigureAwait(false);
        }
    }
}
