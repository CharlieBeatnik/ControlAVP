using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using ControllableDevice;
using Microsoft.Azure.Devices.Client;
using Newtonsoft.Json;

namespace ControlRelay
{
    class SonyKDL60W855CloudInterface : DeviceCloudInterface
    {
        private SonyKDL60W855 _device;

        public struct Settings
        {
            public string Host { get; set; }
            public string PhysicalAddress { get; set; }
            public string PreSharedKey { get; set; }
        }

        private Settings _settings;

        public SonyKDL60W855CloudInterface(Settings settings)
        {
            _settings = settings;
            _device = new SonyKDL60W855(IPAddress.Parse(_settings.Host), PhysicalAddress.Parse(_settings.PhysicalAddress), _settings.PreSharedKey);
        }

        public override IEnumerable<MethodHandlerInfo> GetMethodHandlerInfos(DeviceClient deviceClient)
        {
            yield return new MethodHandlerInfo("TVTurnOn", TurnOn);
        }

        private Task<MethodResponse> TurnOn(MethodRequest methodRequest, object userContext)
        {
            bool success = _device.TurnOn();
            return GetMethodResponse(methodRequest, success);
        }

    }
}