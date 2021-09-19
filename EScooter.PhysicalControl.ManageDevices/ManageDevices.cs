using System;
using System.Threading.Tasks;
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
            string connectionString = Environment.GetEnvironmentVariable("HubRegistryConnectionString");
            var registryManager = RegistryManager.CreateFromConnectionString(connectionString);

            var message = JsonConvert.DeserializeObject<ScooterCreated>(mySbMsg);
            var (device, exists) = await AddOrGetDeviceAsync(message.Id, registryManager);
            if (exists)
            {
                logger.LogInformation($"Device with id {device.Id} already existing");
            }
            else
            {
                logger.LogInformation($"New device registered with id {device.Id}");
                var twin = await SetDefaultProperties(message.Id, registryManager);
                logger.LogInformation($"Update device twin: ${twin.ToJson()}");
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

            var message = JsonConvert.DeserializeObject<ScooterCreated>(mySbMsg);
            var id = message.Id.ToString();
            try
            {
                await registryManager.RemoveDeviceAsync(id);
                logger.LogInformation($"Device with id {id} was removed");
            }
            catch (DeviceNotFoundException)
            {
                logger.LogInformation($"Device with id {id} not found");
            }
        }

        private static async Task<(Device Device, bool Exists)> AddOrGetDeviceAsync(Guid id, RegistryManager registryManager)
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

        private static async Task<Twin> SetDefaultProperties(Guid id, RegistryManager registryManager)
        {
            var twin = await registryManager.GetTwinAsync(id.ToString());

            // TODO: change this when we know how
            var patch =
                @"{
                    tags: {
                        type: 'EScooter'
                    }
                    properties: {
                        desired: {
                            updateFrequency: '5m',
                            maxSpeed: 25,
                            powerSavingThreshold: 50,
                            standbyThreshold: 10
                        }
                    }
                }";
            return await registryManager.UpdateTwinAsync(twin.DeviceId, patch, twin.ETag);
        }
    }
}
