using Moq.Protected;
using Moq;
using System.Net;
using Xunit;
using SynapseHealth.Service;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using static System.Net.WebRequestMethods;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace SynapseHealthTests
{
    public class ApiServiceTests
    {
        private readonly IConfiguration _configuration;

        public ApiServiceTests()
        {
            var settings = new Dictionary<string, string> {
                {"OrdersApiURL", "https://orders-api.com/orders"},
                {"AlertApiUrl", "https://alert-api.com/alerts"},
                {"UpdateApiUrl", "https://update-api.com/update"}
            };

            _configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(settings)
                .Build();
        }

        [Fact]
        public async void FetchMedicalEquipmentOrders_ReturnsOKResponse_WithValidData()
        {
            var apiResponse = TestHelpers.ReadFile("ApiResponse.txt");

            var handlerMock = new Mock<HttpMessageHandler>();
            handlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(apiResponse)
                });
            var httpClient = new HttpClient(handlerMock.Object);
            var serviceLogger = new Mock<ILogger<ApiService>>();
            var service = new ApiService(httpClient, _configuration, serviceLogger.Object);

            var response = await service.FetchMedicalEquipmentOrders();

            var firstOrder = response[0];
            Assert.Equal("1", firstOrder["OrderId"].ToString());
            Assert.Equal("Delivered", firstOrder["Status"].ToString());
            Assert.Equal("0", firstOrder["deliveryNotification"].ToString());

            var secondOrder = response[1];
            Assert.Equal("2", secondOrder["OrderId"].ToString());
            Assert.Equal("Not Delivered", secondOrder["Status"].ToString());
            Assert.Equal("0", secondOrder["deliveryNotification"].ToString());
        }
    }
}
