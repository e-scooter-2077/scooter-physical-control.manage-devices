using System;
using System.Threading.Tasks;
using Azure;
using Azure.DigitalTwins.Core;
using Azure.Identity;
using Microsoft.Azure.Devices;
using Microsoft.Azure.Devices.Common.Exceptions;
using Microsoft.Azure.Devices.Shared;
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
            var registryManager = RegistryManager.CreateFromConnectionString(iotHubString);

            string digitalTwinUrl = "https://" + Environment.GetEnvironmentVariable("AzureDTHostname");
            var credential = new DefaultAzureCredential();
            var digitalTwinsClient = new DigitalTwinsClient(new Uri(digitalTwinUrl), credential);

            var message = JsonConvert.DeserializeObject<ScooterCreated>(mySbMsg);

            // Add Digital Twin first
            try
            {
                await DTUtils.AddDigitalTwin(message.Id, digitalTwinsClient);
            }
            catch (RequestFailedException e)
            {
                logger.LogError($"Create twin error: {e.Status}: {e.Message}");
            }

            // Then add IoTHub Device
            var (device, exists) = await IoTHubUtils.AddOrGetDeviceAsync(message.Id, registryManager);
            if (exists)
            {
                logger.LogInformation($"Device with id {device.Id} already existing");
            }
            else
            {
                logger.LogInformation($"New device registered with id {device.Id}");
                var twin = await IoTHubUtils.SetDefaultProperties(message.Id, registryManager);
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
            string connectionString = Environment.GetEnvironmentVariable("HubRegistryConnectionString");
            var registryManager = RegistryManager.CreateFromConnectionString(connectionString);

            string digitalTwinUrl = "https://" + Environment.GetEnvironmentVariable("AzureDTHostname");
            var credential = new ManagedIdentityCredential("https://digitaltwins.azure.net");
            var digitalTwinsClient = new DigitalTwinsClient(new Uri(digitalTwinUrl), credential);

            var message = JsonConvert.DeserializeObject<ScooterCreated>(mySbMsg);
            await DTUtils.RemoveDigitalTwin(message.Id, digitalTwinsClient);
            try
            {
                await IoTHubUtils.RemoveDevice(message.Id, registryManager);
                logger.LogInformation($"Device with id {message.Id} was removed");
            }
            catch (DeviceNotFoundException)
            {
                logger.LogInformation($"Device with id {message.Id} not found");
            }
        }
    }

    internal static class IoTHubUtils
    {
        public static async Task<(Device Device, bool Exists)> AddOrGetDeviceAsync(Guid id, RegistryManager registryManager)
        {
            Device device;
            bool exists = false;
            try
            {
                device = await registryManager.AddDeviceAsync(new Device(id.ToString()));
            }
            catch (DeviceAlreadyExistsException)
            {
                device = await registryManager.GetDeviceAsync(id.ToString());
                exists = true;
            }

            return (device, exists);
        }

        public static async Task RemoveDevice(Guid id, RegistryManager registryManager)
        {
            await registryManager.RemoveDeviceAsync(id.ToString());
        }

        public static async Task<Twin> SetDefaultProperties(Guid id, RegistryManager registryManager)
        {
            var twin = await registryManager.GetTwinAsync(id.ToString());
            var patch =
                @"{
                    tags: {
                        type: 'EScooter'
                    },
                    properties: {
                        desired: {
                            updateFrequency: '00:00:30',
                            maxSpeed: 25,
                            powerSavingThreshold: 50,
                            standbyThreshold: 10
                        }
                    }
                }";
            return await registryManager.UpdateTwinAsync(twin.DeviceId, patch, twin.ETag);
        }
    }

    internal static class DTUtils
    {
        public static async Task AddDigitalTwin(Guid id, DigitalTwinsClient digitalTwinsClient)
        {
            var twinData = new ScooterDigitalTwin();
            twinData.Id = id.ToString();
            twinData.Metadata.ModelId = "dtmi:com:escooter:EScooter;1";
            await digitalTwinsClient.CreateOrReplaceDigitalTwinAsync(twinData.Id, twinData);
        }

        public static async Task RemoveDigitalTwin(Guid id, DigitalTwinsClient digitalTwinsClient)
        {
            await digitalTwinsClient.DeleteDigitalTwinAsync(id.ToString());
        }
    }
}
