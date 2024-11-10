using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using SynapseHealth.Interface;

namespace SynapseHealth.Service
{
    public class OrderService : IOrderService
    {
        private readonly IApiService _apiService;
        private readonly ILogger<OrderService> _logger;

        public OrderService(IApiService apiService, ILogger<OrderService> logger)
        {
            _apiService = apiService;
            _logger = logger;

            _logger.LogDebug("Order Service Initialized");
        }

        public JObject ProcessOrder(JObject order)
        {
            var items = order["Items"].ToObject<JArray>();
            foreach (var item in items)
            {
                if (IsItemDelivered(item))
                {
                    _apiService.SendAlertMessage(item, order["OrderId"].ToString());
                    IncrementDeliveryNotification(item);
                }
            }

            return order;
        }

        private bool IsItemDelivered(JToken item)
        {
            return item["Status"].ToString().Equals("Delivered", StringComparison.OrdinalIgnoreCase);
        }

        private void IncrementDeliveryNotification(JToken item)
        {
            item["deliveryNotification"] = item["deliveryNotification"].Value<int>() + 1;
        }
    }
}
