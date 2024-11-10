using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using SynapseHealth.Interface;
using Microsoft.Extensions.Configuration;

namespace SynapseHealth.Service
{
    public class ApiService : IApiService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<ApiService> _logger;
        private string _ordersApiUrl = string.Empty;
        private string _alertApiUrl = string.Empty;
        private string _updateApiUrl = string.Empty;

        public ApiService(HttpClient httpClient, IConfiguration configuration, ILogger<ApiService> logger)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _logger = logger;

            _ordersApiUrl = _configuration.GetValue<string>("OrdersApiURL") ?? "";
            _alertApiUrl = _configuration.GetValue<string>("AlertApiUrl") ?? "";
            _updateApiUrl = _configuration.GetValue<string>("UpdateApiUrl") ?? "";

            _logger.LogDebug("API Service Initialized");
        }

        public async Task<JObject[]> FetchMedicalEquipmentOrders()
        {
            try
            {
                var response = await _httpClient.GetAsync(_ordersApiUrl);
                if (response.IsSuccessStatusCode)
                {
                    var ordersData = await response.Content.ReadAsStringAsync();
                    var objects = JObject.Parse(ordersData);
                    return objects["items"].ToObject<JObject[]>();
                }
                else
                {
                    _logger.LogError($"Failed to fetch orders from API with status: {response.StatusCode}");
                    return new JObject[0];
                }
            }
            finally
            {
                _httpClient.Dispose();
            }
        }

        /// <summary>
        /// Delivery alert
        /// </summary>
        /// <param name="orderId">The order id for the alert</param>
        public void SendAlertMessage(JToken item, string orderId)
        {
            try
            {
                var alertData = new
                {
                    Message = $"Alert for delivered item: Order {orderId}, Item: {item["Description"]}, " +
                              $"Delivery Notifications: {item["deliveryNotification"]}"
                };
                var content = new StringContent(JObject.FromObject(alertData).ToString(), System.Text.Encoding.UTF8, "application/json");
                var response = _httpClient.PostAsync(_alertApiUrl, content).Result;

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation($"Alert sent for delivered item: {item["Description"]}");
                }
                else
                {
                    _logger.LogError($"Failed to send alert for delivered item: {item["Description"]}, with status: {response.StatusCode}");
                }
            }
            finally
            { 
                _httpClient.Dispose(); 
            }
        }

        public async Task SendAlertAndUpdateOrder(JObject order)
        {
            try
            {
                var content = new StringContent(order.ToString(), System.Text.Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync(_updateApiUrl, content);

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation($"Updated order sent for processing: OrderId {order["OrderId"]}");
                }
                else
                {
                    _logger.LogError($"Failed to send updated order for processing: OrderId {order["OrderId"]}, with status: {response.StatusCode}");
                }
            }
            finally
            {
                _httpClient.Dispose();
            }
        }
    }
}
