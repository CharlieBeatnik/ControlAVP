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

        public ApcAP8959EU3CloudInterface(ApcAP8959EU3 device)
        {
            _device = device;
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
            var payloadDefintion = new
            {
                getPower = false,
                getCurrent = false
            };

            var payload = JsonConvert.DeserializeAnonymousType(methodRequest.DataAsJson, payloadDefintion);
            var result = _device.GetOutlets(payload.getPower, payload.getCurrent);

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
            var payloadDefintion = new
            {
                getPower = false,
                getCurrent = false
            };

            var payload = JsonConvert.DeserializeAnonymousType(methodRequest.DataAsJson, payloadDefintion);
            var result = _device.GetOutletsWaitForPending(payload.getPower, payload.getCurrent);

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