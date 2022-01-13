using Azure.DigitalTwins.Core;
using EScooter.DigitalTwins.Commons;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EScooter.PhysicalControl.ManageDevices
{
    public class DigitalTwinManager
    {
        private readonly DigitalTwinsClient _dtClient;

        public DigitalTwinManager(Uri uri, Azure.Core.TokenCredential token)
        {
            _dtClient = new DigitalTwinsClient(uri, token);
        }

        public async Task AddDigitalTwin(Guid id)
        {
            var twinData = new ScooterDigitalTwin(id);
            await _dtClient.CreateOrReplaceDigitalTwinAsync(twinData.Id, twinData);
        }

        public async Task RemoveDigitalTwin(Guid id)
        {
            await _dtClient.DeleteDigitalTwinAsync(id.ToString());
        }
    }
}
