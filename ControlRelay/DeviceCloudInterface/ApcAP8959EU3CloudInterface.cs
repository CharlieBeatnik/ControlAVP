using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using ControllableDevice;
using Microsoft.Azure.Devices.Client;
using Newtonsoft.Json;

namespace ControlRelay
{
    class ApcAP8959EU3CloudInterface : DeviceCloudInterface
    {
        private ApcAP8959EU3 _device;

        public struct Settings
        {
            public string Host { get; set; }
            public int Port { get; set; }
            public string Username { get; set; }
            public string Password { get; set; }
        }

        private Settings _settings;

        public ApcAP8959EU3CloudInterface(Settings settings)
        {
            _settings = settings;
            _device = new ApcAP8959EU3(_settings.Host, _settings.Port, _settings.Username, _settings.Password);
        }

        public override void SetMethodHandlers(DeviceClient deviceClient)
        {
            deviceClient.SetMethodHandlerAsync("PDUGetOutlets", GetOutlets, null).Wait();
            deviceClient.SetMethodHandlerAsync("PDUGetOutletsWaitForPending", GetOutletsWaitForPending, null).Wait();
            deviceClient.SetMethodHandlerAsync("PDUTurnOutletOn", TurnOutletOn, null).Wait();
            deviceClient.SetMethodHandlerAsync("PDUTurnOutletOff", TurnOutletOff, null).Wait();
            deviceClient.SetMethodHandlerAsync("PDUGetAvailable", GetAvailable, null).Wait();
        }

        private Task<MethodResponse> GetOutlets(MethodRequest methodRequest, object userContext)
        {
            var result = _device.GetOutlets();

            if (result != null)
            {
                return GetMethodResponseSerialize(methodRequest, true, result);
            }
            else
            {
                return GetMethodResponse(methodRequest, false);
            }
        }

        private Task<MethodResponse> GetOutletsWaitForPending(MethodRequest methodRequest, object userContext)
        {
            var result = _device.GetOutletsWaitForPending();

            if (result != null)
            {
                return GetMethodResponseSerialize(methodRequest, true, result);
            }
            else
            {
                return GetMethodResponse(methodRequest, false);
            }
        }

        private Task<MethodResponse> TurnOutletOn(MethodRequest methodRequest, object userContext)
        {
            bool success = false;
            var payloadDefintion = new
            {
                outletId = -1
            };

            var payload = JsonConvert.DeserializeAnonymousType(methodRequest.DataAsJson, payloadDefintion);
            if (ApcAP8959EU3.OutletIdValid(payload.outletId))
            {
                success = _device.TurnOutletOn(payload.outletId);
            }

            return GetMethodResponse(methodRequest, success);
        }

        private Task<MethodResponse> TurnOutletOff(MethodRequest methodRequest, object userContext)
        {
            bool success = false;
            var payloadDefintion = new
            {
                outletId = -1
            };

            var payload = JsonConvert.DeserializeAnonymousType(methodRequest.DataAsJson, payloadDefintion);
            if (ApcAP8959EU3.OutletIdValid(payload.outletId))
            {
                success = _device.TurnOutletOff(payload.outletId);
            }
            return GetMethodResponse(methodRequest, success);
        }

        private Task<MethodResponse> GetAvailable(MethodRequest methodRequest, object userContext)
        {
            var result = _device.GetAvailable();
            return GetMethodResponseSerialize(methodRequest, true, result);
        }
    }
}