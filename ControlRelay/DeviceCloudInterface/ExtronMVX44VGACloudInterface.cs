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
            _device = ExtronMVX44VGA.Create(_settings.PortId);
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
                var response = new MethodResponse(Encoding.UTF8.GetBytes(json), (int)HttpStatusCode.OK);
                return Task.FromResult(response);
            }
            else
            {
                return Task.FromResult(GetMethodResponse(methodRequest, false));
            }
        }

        private Task<MethodResponse> GetAvailable(MethodRequest methodRequest, object userContext)
        {
            var result = _device.GetAvailable();

            string json = JsonConvert.SerializeObject(result);
            var response = new MethodResponse(Encoding.UTF8.GetBytes(json), (int)HttpStatusCode.OK);
            return Task.FromResult(response);
        }

    }
}