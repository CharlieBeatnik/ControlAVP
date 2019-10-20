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

        public override IEnumerable<MethodHandlerInfo> GetMethodHandlerInfos(DeviceClient deviceClient)
        {
            yield return new MethodHandlerInfo("PDUGetOutlets", GetOutlets);
            yield return new MethodHandlerInfo("PDUGetOutletsWaitForPending", GetOutletsWaitForPending);
            yield return new MethodHandlerInfo("PDUTurnOutletOn", TurnOutletOn);
            yield return new MethodHandlerInfo("PDUTurnOutletOff", TurnOutletOff);
            yield return new MethodHandlerInfo("PDUGetAvailable", GetAvailable);
        }

        private Task<MethodResponse> GetOutlets(MethodRequest methodRequest, object userContext)
        {
            var result = _device.GetOutlets();

            if (result != null)
            {
                return methodRequest.GetMethodResponseSerialize(true, result);
            }
            else
            {
                return methodRequest.GetMethodResponse(false);
            }
        }

        private Task<MethodResponse> GetOutletsWaitForPending(MethodRequest methodRequest, object userContext)
        {
            var result = _device.GetOutletsWaitForPending();

            if (result != null)
            {
                return methodRequest.GetMethodResponseSerialize(true, result);
            }
            else
            {
                return methodRequest.GetMethodResponse(false);
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

            return methodRequest.GetMethodResponse(success);
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
            return methodRequest.GetMethodResponse(success);
        }

        private Task<MethodResponse> GetAvailable(MethodRequest methodRequest, object userContext)
        {
            var result = _device.GetAvailable();
            return methodRequest.GetMethodResponseSerialize(true, result);
        }
    }
}