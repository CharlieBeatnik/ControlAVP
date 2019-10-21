using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using ControllableDevice;
using ControllableDeviceTypes.AtenVS0801HTypes;
using Microsoft.Azure.Devices.Client;
using Newtonsoft.Json;

namespace ControlRelay
{
    class AtenVS0801HCloudInterface : DeviceCloudInterface
    {
        private List<AtenVS0801H> _devices = new List<AtenVS0801H>();

        public AtenVS0801HCloudInterface(List<AtenVS0801H> devices)
        {
            _devices = devices;
        }

        public override IEnumerable<MethodHandlerInfo> GetMethodHandlerInfos(DeviceClient deviceClient)
        {
            yield return new MethodHandlerInfo("HDMISwitchGoToNextInput", GoToNextInput);
            yield return new MethodHandlerInfo("HDMISwitchGoToPreviousInput", GoToPreviousInput);
            yield return new MethodHandlerInfo("HDMISwitchGetState", GetState);
            yield return new MethodHandlerInfo("HDMISwitchSetInputPort", SetInputPort);
            yield return new MethodHandlerInfo("HDMISwitchGetAvailable", GetAvailable);
        }

        private Task<MethodResponse> GoToNextInput(MethodRequest methodRequest, object userContext)
        {
            var payloadDefintion = new { _hdmiSwitchId = -1 };

            var payload = JsonConvert.DeserializeAnonymousType(methodRequest.DataAsJson, payloadDefintion);
            if (HdmiSwitchIdValid(payload._hdmiSwitchId))
            {
                bool success = _devices[payload._hdmiSwitchId].GoToNextInput();
                return methodRequest.GetMethodResponse(success);
            }

            return methodRequest.GetMethodResponse(false);
        }

        private Task<MethodResponse> GoToPreviousInput(MethodRequest methodRequest, object userContext)
        {
            var payloadDefintion = new { _hdmiSwitchId = -1 };

            var payload = JsonConvert.DeserializeAnonymousType(methodRequest.DataAsJson, payloadDefintion);
            if (HdmiSwitchIdValid(payload._hdmiSwitchId))
            {
                bool success = _devices[payload._hdmiSwitchId].GoToPreviousInput();
                return methodRequest.GetMethodResponse(success);
            }

            return methodRequest.GetMethodResponse(false);
        }

        private Task<MethodResponse> GetState(MethodRequest methodRequest, object userContext)
        {
            var payloadDefintion = new { _hdmiSwitchId = -1 };

            var payload = JsonConvert.DeserializeAnonymousType(methodRequest.DataAsJson, payloadDefintion);
            if (HdmiSwitchIdValid(payload._hdmiSwitchId))
            {
                var result = _devices[payload._hdmiSwitchId].GetState();
                if (result != null)
                {
                    return methodRequest.GetMethodResponseSerialize(true, result);
                }
            }

            return methodRequest.GetMethodResponse(false);
        }

        private Task<MethodResponse> SetInputPort(MethodRequest methodRequest, object userContext)
        {
            var payloadDefintion = new
            {
                _hdmiSwitchId = -1,
                inputPort = (InputPort)(-1)
            };

            var payload = JsonConvert.DeserializeAnonymousType(methodRequest.DataAsJson, payloadDefintion);
            if (HdmiSwitchIdValid(payload._hdmiSwitchId) && payload.inputPort.Valid())
            {
                bool success = _devices[payload._hdmiSwitchId].SetInputPort(payload.inputPort);
                return methodRequest.GetMethodResponse(success);
            }

            return methodRequest.GetMethodResponse(false);
        }

        private Task<MethodResponse> GetAvailable(MethodRequest methodRequest, object userContext)
        {
            var payloadDefintion = new { _hdmiSwitchId = -1 };

            var payload = JsonConvert.DeserializeAnonymousType(methodRequest.DataAsJson, payloadDefintion);
            if (HdmiSwitchIdValid(payload._hdmiSwitchId))
            {
                var result = _devices[payload._hdmiSwitchId].GetAvailable();
                return methodRequest.GetMethodResponseSerialize(true, result);
            }

            return methodRequest.GetMethodResponse(false);
        }

        private bool HdmiSwitchIdValid(int hdmiSwitchId)
        {
            return (hdmiSwitchId >= 0 && hdmiSwitchId < _devices.Count);
        }
    }
}
