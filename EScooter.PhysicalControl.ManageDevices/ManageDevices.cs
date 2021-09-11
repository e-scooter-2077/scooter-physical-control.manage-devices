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
        [Function("add-to-iot-hub")]
        public static async Task Run([ServiceBusTrigger("dev~service-events", "add-to-iot-hub-function", Connection = "ServiceBusConnectionString")] string mySbMsg, FunctionContext context)
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

        private static async Task<(Device, bool)> AddOrGetDeviceAsync(Guid id, RegistryManager registryManager)
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
