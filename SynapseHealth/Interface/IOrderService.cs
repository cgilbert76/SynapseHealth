using Newtonsoft.Json.Linq;

namespace SynapseHealth.Interface
{
    public interface IOrderService
    {
        JObject ProcessOrder(JObject order);
    }
}
