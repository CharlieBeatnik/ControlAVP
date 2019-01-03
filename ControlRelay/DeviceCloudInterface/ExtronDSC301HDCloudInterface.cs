using System.Collections.Generic;
using System.Diagnostics;
using ControllableDevice;
using Microsoft.Azure.Devices.Client;

namespace ControlRelay
{
    class ExtronDSC301HDCloudInterface : DeviceCloudInterface
    {
        private ExtronDSC301HD _device;

        public struct Settings
        {
            public string PortId { get; set; }
        }

        private Settings _settings;

        public ExtronDSC301HDCloudInterface(Settings settings)
        {
            _settings = settings;
            _device = new ExtronDSC301HD(_settings.PortId);
        }

        public override void SetMethodHandlers(DeviceClient deviceClient)
        {
        }
     
    }
}