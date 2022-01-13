using System;
using System.Threading.Tasks;
using Azure.Identity;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace EScooter.PhysicalControl.ManageDevices
{
    public record ScooterCreated(Guid Id);

    public record ScooterDeleted(Guid Id);

    /// <summary>
    /// A function that adds a device to the IoTHub when the event is received.
    /// </summary>
    public static class ManageDevices
    {
        /// <summary>
        /// When triggered, adds a device to the IoT hub with the given Id and default properties.
        /// If the Id is already in use it does nothing.
        /// </summary>
        /// <param name="mySbMsg">The message received with the event.</param>
        /// <param name="context">The function execution context.</param>
        /// <returns>An empty task.</returns>
        [Function("add-device-and-twin")]
        public static async Task AddDevice([ServiceBusTrigger("%TopicName%", "%AddSubscription%", Connection = "ServiceBusConnectionString")] string mySbMsg, FunctionContext context)
        {
            var logger = context.GetLogger("Function");
            string iotHubString = Environment.GetEnvironmentVariable("HubRegistryConnectionString");
            string digitalTwinUrl = "https://" + Environment.GetEnvironmentVariable("AzureDTHostname");
            var credential = new DefaultAzureCredential();

            var dtManager = new DigitalTwinManager(new Uri(digitalTwinUrl), credential);
            var iotHubManager = new IoTHubManager(iotHubString);

            var message = JsonConvert.DeserializeObject<ScooterCreated>(mySbMsg);

            // Add Digital Twin first
            await dtManager.AddDigitalTwin(message.Id);

            // Then add IoTHub Device
            var (device, exists) = await iotHubManager.AddOrGetDeviceAsync(message.Id);
            if (exists)
            {
                logger.LogInformation($"Device with id {device.Id} already existing");
            }
            else
            {
                logger.LogInformation($"New device registered with id {device.Id}");
                await iotHubManager.SetDefaultProperties(message.Id);
                logger.LogInformation($"Update device twin default properties");
            }
        }

        /// <summary>
        /// When triggered, removes the device with the given Id from the IoT Hub.
        /// If there is no device corresponding to that Id it does nothing.
        /// </summary>
        /// <param name="mySbMsg">The message received with the event.</param>
        /// <param name="context">The function execution context.</param>
        /// <returns>An empty task.</returns>
        [Function("remove-device-and-twin")]
        public static async Task RemoveDevice([ServiceBusTrigger("%TopicName%", "%RemoveSubscription%", Connection = "ServiceBusConnectionString")] string mySbMsg, FunctionContext context)
        {
            var logger = context.GetLogger("Function");
            string iotHubString = Environment.GetEnvironmentVariable("HubRegistryConnectionString");
            string digitalTwinUrl = "https://" + Environment.GetEnvironmentVariable("AzureDTHostname");
            var credential = new DefaultAzureCredential();

            var dtManager = new DigitalTwinManager(new Uri(digitalTwinUrl), credential);
            var iotHubManager = new IoTHubManager(iotHubString);

            var message = JsonConvert.DeserializeObject<ScooterCreated>(mySbMsg);
            await dtManager.RemoveDigitalTwin(message.Id);
            await iotHubManager.RemoveDevice(message.Id);
            logger.LogInformation($"Device with id {message.Id} was removed");
        }
    }
}
