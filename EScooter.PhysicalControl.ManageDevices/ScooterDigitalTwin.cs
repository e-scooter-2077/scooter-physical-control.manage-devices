using Azure.DigitalTwins.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace EScooter.DigitalTwins.Commons
{
    public class ScooterDigitalTwin
    {
        [JsonPropertyName(DigitalTwinsJsonPropertyNames.DigitalTwinId)]
        public string Id { get; }

        [JsonPropertyName(DigitalTwinsJsonPropertyNames.DigitalTwinETag)]
        public string ETag { get; }

        [JsonPropertyName(DigitalTwinsJsonPropertyNames.DigitalTwinMetadata)]
        public ScooterDigitalTwinMetadata Metadata { get; }

        [JsonPropertyName("UpdateFrequency")]
        public int UpdateFrequency { get; }

        [JsonPropertyName("Locked")]
        public bool Locked { get; }

        [JsonPropertyName("Enabled")]
        public bool Enabled { get; }

        [JsonPropertyName("MaxSpeed")]
        public double MaxSpeed { get; }

        [JsonPropertyName("Standby")]
        public bool Standby { get; }

        [JsonPropertyName("Connected")]
        public bool Connected { get; }

        [JsonPropertyName("BatteryLevel")]
        public double BatteryLevel { get; }

        [JsonPropertyName("Latitude")]
        public double Latitude { get; }

        [JsonPropertyName("Longitude")]
        public double Longitude { get; }

        [JsonPropertyName("Speed")]
        public double Speed { get; }

        public ScooterDigitalTwin(Guid id)
        {
            Id = id.ToString();
            Connected = false;
            Locked = true;
            BatteryLevel = 0;
            Enabled = false;
            Standby = true;
            UpdateFrequency = 30;
            MaxSpeed = 30;
            Metadata = new ScooterDigitalTwinMetadata();
        }
    }

    public class ScooterDigitalTwinMetadata
    {
        [JsonPropertyName(DigitalTwinsJsonPropertyNames.MetadataModel)]
        public string ModelId { get; }

        public ScooterDigitalTwinMetadata()
        {
            ModelId = "dtmi:com:escooter:EScooter;1";
        }
    }
}
