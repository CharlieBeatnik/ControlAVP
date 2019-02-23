using System.Net;
using System.Text;
using System.Threading.Tasks;
using ControllableDevice;
using Microsoft.Azure.Devices.Client;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace ControlRelay
{
    class ExtronMVX44VGACloudInterface : DeviceCloudInterface
    {
        private ExtronMVX44VGA _device;

        public struct Settings
        {
            public string PortId { get; set; }
        }

        private Settings _settings;

        public ExtronMVX44VGACloudInterface(Settings settings)
        {
            _settings = settings;
            _device = new ExtronMVX44VGA(_settings.PortId);
        }

        public override void SetMethodHandlers(DeviceClient deviceClient)
        {
            deviceClient.SetMethodHandlerAsync("VGAMatrixGetFirmware", GetFirmware, null).Wait();
            deviceClient.SetMethodHandlerAsync("VGAMatrixGetAvailable", GetAvailable, null).Wait();
        }

        private Task<MethodResponse> GetFirmware(MethodRequest methodRequest, object userContext)
        {
            var result = _device.GetFirmware();

            if (result != null)
            {
                string json = JsonConvert.SerializeObject(result, new VersionConverter());
                return GetMethodResponse(methodRequest, true, json);
            }
            else
            {
                return GetMethodResponse(methodRequest, false);
            }
        }

        private Task<MethodResponse> GetAvailable(MethodRequest methodRequest, object userContext)
        {
            var result = _device.GetAvailable();

            return GetMethodResponseSerialize(methodRequest, true, result);
        }

    }
}