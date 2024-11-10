using Newtonsoft.Json.Linq;

namespace SynapseHealth.Interface
{
    public interface IApiService
    {
        Task<JObject[]> FetchMedicalEquipmentOrders();
        void SendAlertMessage(JToken item, string orderId);
        Task SendAlertAndUpdateOrder(JObject order);
    }
}
