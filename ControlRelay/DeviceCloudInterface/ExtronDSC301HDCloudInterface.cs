using System.Net;
using System.Text;
using System.Threading.Tasks;
using ControllableDevice;
using ControllableDeviceTypes.ExtronDSC301HDTypes;
using Microsoft.Azure.Devices.Client;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

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
            deviceClient.SetMethodHandlerAsync("ScalerGetFirmware", GetFirmware, null).Wait();
            deviceClient.SetMethodHandlerAsync("ScalerGetAvailable", GetAvailable, null).Wait();
            deviceClient.SetMethodHandlerAsync("ScalerSetPixelPerfectAndCentered", SetPixelPerfectAndCentered, null).Wait();
            deviceClient.SetMethodHandlerAsync("ScalerSetOutputRate", SetOutputRate, null).Wait();
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

        private Task<MethodResponse> SetPixelPerfectAndCentered(MethodRequest methodRequest, object userContext)
        {
            _device.SetPixelPerfectAndCentered();
            return Task.FromResult(new MethodResponse((int)HttpStatusCode.OK));
        }

        private Task<MethodResponse> SetOutputRate(MethodRequest methodRequest, object userContext)
        {
            bool success = false;
            var payloadDefintion = new
            {
                Id = 0,
            };

            var payload = JsonConvert.DeserializeAnonymousType(methodRequest.DataAsJson, payloadDefintion);

            var edid = Edid.GetEdid(payload.Id);
            if(edid != null)
            {
                _device.OutputRate = edid;
                success = true;
            }

            return Task.FromResult(GetMethodResponse(methodRequest, success));
        }

    }
}