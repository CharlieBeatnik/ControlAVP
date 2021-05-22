using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using ControllableDevice;
using Microsoft.Azure.Devices.Client;
using Newtonsoft.Json;
using ControllableDeviceTypes.SonySimpleIPTypes;

namespace ControlRelay
{
    class SonySimpleIPCloudInterface : DeviceCloudInterface
    {
        private SonySimpleIP _device;

        public SonySimpleIPCloudInterface(SonySimpleIP device)
        {
            _device = device;
        }

        public override IEnumerable<MethodHandlerInfo> GetMethodHandlerInfos(DeviceClient deviceClient)
        {
            yield return new MethodHandlerInfo("TVTurnOn", TurnOn);
            yield return new MethodHandlerInfo("TVTurnOff", TurnOff);
            yield return new MethodHandlerInfo("TVSetInputPort", SetInputPort);
            yield return new MethodHandlerInfo("TVGetInputPort", GetInputPort);
            yield return new MethodHandlerInfo("TVGetPowerStatus", GetPowerStatus);
        }

        private Task<MethodResponse> TurnOn(MethodRequest methodRequest, object userContext)
        {
            bool success = _device.TurnOn();
            return methodRequest.GetMethodResponse(success);
        }

        private Task<MethodResponse> TurnOff(MethodRequest methodRequest, object userContext)
        {
            bool success = _device.TurnOff();
            return methodRequest.GetMethodResponse(success);
        }

        private Task<MethodResponse> SetInputPort(MethodRequest methodRequest, object userContext)
        {
            var payloadDefintion = new
            {
                inputPort = (InputPort)(-1)
            };

            var payload = JsonConvert.DeserializeAnonymousType(methodRequest.DataAsJson, payloadDefintion);
            if (payload.inputPort.Valid())
            {
                bool success = _device.SetInputPort(payload.inputPort);
                return methodRequest.GetMethodResponse(success);
            }

            return methodRequest.GetMethodResponse(false);
        }

        private Task<MethodResponse> GetInputPort(MethodRequest methodRequest, object userContext)
        {
            return methodRequest.Get(() => _device.GetInputPort());
        }

        private Task<MethodResponse> GetPowerStatus(MethodRequest methodRequest, object userContext)
        {
            return methodRequest.Get(() => _device.GetPowerStatus());
        }

    }
}