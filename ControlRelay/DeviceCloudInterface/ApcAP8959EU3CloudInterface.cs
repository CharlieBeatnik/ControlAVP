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
            deviceClient.SetMethodHandlerAsync("PDUGetOutlets", PDUGetOutlets, null).Wait();
            deviceClient.SetMethodHandlerAsync("PDUGetOutletsWaitForPending", PDUGetOutletsWaitForPending, null).Wait();
            deviceClient.SetMethodHandlerAsync("PDUTurnOutletOn", PDUTurnOutletOn, null).Wait();
            deviceClient.SetMethodHandlerAsync("PDUTurnOutletOff", PDUTurnOutletOff, null).Wait();
        }

        private Task<MethodResponse> PDUGetOutlets(MethodRequest methodRequest, object userContext)
        {
            var result = _device.GetOutlets();

            if (result != null)
            {
                string json = JsonConvert.SerializeObject(result);
                var response = new MethodResponse(Encoding.UTF8.GetBytes(json), (int)HttpStatusCode.OK);
                return Task.FromResult(response);
            }
            else
            {
                return Task.FromResult(GetMethodResponse(methodRequest, false));
            }
        }

        private Task<MethodResponse> PDUGetOutletsWaitForPending(MethodRequest methodRequest, object userContext)
        {
            var result = _device.GetOutletsWaitForPending();

            if (result != null)
            {
                string json = JsonConvert.SerializeObject(result);
                var response = new MethodResponse(Encoding.UTF8.GetBytes(json), (int)HttpStatusCode.OK);
                return Task.FromResult(response);
            }
            else
            {
                return Task.FromResult(GetMethodResponse(methodRequest, false));
            }
        }

        private Task<MethodResponse> PDUTurnOutletOn(MethodRequest methodRequest, object userContext)
        {
            var payloadDefintion = new
            {
                outletId = -1
            };

            var payload = JsonConvert.DeserializeAnonymousType(methodRequest.DataAsJson, payloadDefintion);
            _device.TurnOutletOn(payload.outletId);
            //ANDREWDENN_TODO: No way of determining outlet change succeded or failed
            bool success = true;
            return Task.FromResult(GetMethodResponse(methodRequest, success));
        }

        private Task<MethodResponse> PDUTurnOutletOff(MethodRequest methodRequest, object userContext)
        {
            var payloadDefintion = new
            {
                outletId = -1
            };

            var payload = JsonConvert.DeserializeAnonymousType(methodRequest.DataAsJson, payloadDefintion);
            _device.TurnOutletOff(payload.outletId);
            //ANDREWDENN_TODO: No way of determining outlet change succeded or failed
            bool success = true;
            return Task.FromResult(GetMethodResponse(methodRequest, success));
        }
    }
}