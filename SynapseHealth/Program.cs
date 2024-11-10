using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SynapseHealth.Interface;
using SynapseHealth.Service;

namespace SynapseHealth
{
    /// <summary>
    /// I Get a list of orders from the API
    /// I check if the order is in a delviered state, If yes then send a delivery alert and add one to deliveryNotification
    /// I then update the order.   
    /// </summary>
    class Program
    {
        static int Main(string[] args)
        {
            //point config to appsettings.json file
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false)
                .Build();

            //build up service collection
            var serviceProvider = new ServiceCollection()
                .AddLogging(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Information))
                .AddSingleton<IConfiguration>(config)
                .AddSingleton<IOrderService, OrderService>()
                .AddScoped<IApiService, ApiService>(
                    serviceProvider => new ApiService(
                        httpClient: new HttpClient(), 
                        configuration: serviceProvider.GetRequiredService<IConfiguration>(),
                        logger: serviceProvider.GetRequiredService<ILogger<ApiService>>()))
                .BuildServiceProvider();

            //services
            var apiService = serviceProvider.GetRequiredService<IApiService>();
            var orderService = serviceProvider.GetRequiredService<IOrderService>();
            var logger = serviceProvider.GetRequiredService<ILogger<Program>>();

            logger.LogInformation("Start of App");

            //fetch orders, loop through and update orders, send alerts
            var medicalEquipmentOrders = apiService.FetchMedicalEquipmentOrders().GetAwaiter().GetResult();
            foreach (var order in medicalEquipmentOrders)
            {
                var updatedOrder = orderService.ProcessOrder(order);
                apiService.SendAlertAndUpdateOrder(updatedOrder).GetAwaiter().GetResult();
            }

            logger.LogInformation("Results sent to relevant APIs.");
            return 0;
        }
    }
}