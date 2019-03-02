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

        public struct Settings
        {
            public string PortId { get; set; }
        }

        private List<Settings> _settings;

        public AtenVS0801HCloudInterface(List<Settings> settings)
        {
            _settings = settings;
            foreach (var setting in settings)
            {
                _devices.Add(new AtenVS0801H(setting.PortId));
            }
        }

        public override void SetMethodHandlers(DeviceClient deviceClient)
        {
            deviceClient.SetMethodHandlerAsync("HDMISwitchGoToNextInput", GoToNextInput, null).Wait();
            deviceClient.SetMethodHandlerAsync("HDMISwitchGoToPreviousInput", GoToPreviousInput, null).Wait();
            deviceClient.SetMethodHandlerAsync("HDMISwitchGetState", GetState, null).Wait();
            deviceClient.SetMethodHandlerAsync("HDMISwitchSetInput", SetInput, null).Wait();
            deviceClient.SetMethodHandlerAsync("HDMISwitchGetAvailable", GetAvailable, null).Wait();
        }

        private Task<MethodResponse> GoToNextInput(MethodRequest methodRequest, object userContext)
        {
            var payloadDefintion = new { _hdmiSwitchId = -1 };

            var payload = JsonConvert.DeserializeAnonymousType(methodRequest.DataAsJson, payloadDefintion);
            if (ValidateHdmiSwitchId(payload._hdmiSwitchId))
            {
                bool success = _devices[payload._hdmiSwitchId].GoToNextInput();
                return GetMethodResponse(methodRequest, success);
            }

            return GetMethodResponse(methodRequest, false);
        }

        private Task<MethodResponse> GoToPreviousInput(MethodRequest methodRequest, object userContext)
        {
            var payloadDefintion = new { _hdmiSwitchId = -1 };

            var payload = JsonConvert.DeserializeAnonymousType(methodRequest.DataAsJson, payloadDefintion);
            if (ValidateHdmiSwitchId(payload._hdmiSwitchId))
            {
                bool success = _devices[payload._hdmiSwitchId].GoToPreviousInput();
                return GetMethodResponse(methodRequest, success);
            }

            return GetMethodResponse(methodRequest, false);
        }

        private Task<MethodResponse> GetState(MethodRequest methodRequest, object userContext)
        {
            var payloadDefintion = new { _hdmiSwitchId = -1 };

            var payload = JsonConvert.DeserializeAnonymousType(methodRequest.DataAsJson, payloadDefintion);
            if (ValidateHdmiSwitchId(payload._hdmiSwitchId))
            {
                var result = _devices[payload._hdmiSwitchId].GetState();
                if (result != null)
                {
                    return GetMethodResponseSerialize(methodRequest, true, result);
                }
            }

            return GetMethodResponse(methodRequest, false);
        }

        private Task<MethodResponse> SetInput(MethodRequest methodRequest, object userContext)
        {
            var payloadDefintion = new
            {
                _hdmiSwitchId = -1,
                inputPort = InputPort.Port1
            };

            var payload = JsonConvert.DeserializeAnonymousType(methodRequest.DataAsJson, payloadDefintion);
            if (ValidateHdmiSwitchId(payload._hdmiSwitchId) && ValidateInputPort(payload.inputPort))
            {
                bool success = _devices[payload._hdmiSwitchId].SetInput(payload.inputPort);
                return GetMethodResponse(methodRequest, success);
            }

            return GetMethodResponse(methodRequest, false);
        }

        private Task<MethodResponse> GetAvailable(MethodRequest methodRequest, object userContext)
        {
            var payloadDefintion = new { _hdmiSwitchId = -1 };

            var payload = JsonConvert.DeserializeAnonymousType(methodRequest.DataAsJson, payloadDefintion);
            if (ValidateHdmiSwitchId(payload._hdmiSwitchId))
            {
                var result = _devices[payload._hdmiSwitchId].GetAvailable();
                return GetMethodResponseSerialize(methodRequest, true, result);
            }

            return GetMethodResponse(methodRequest, false);
        }

        private bool ValidateHdmiSwitchId(int hdmiSwitchId)
        {
            return (hdmiSwitchId >= 0 && hdmiSwitchId < _devices.Count);
        }

        private bool ValidateInputPort(InputPort inputPort)
        {
            return (inputPort >= InputPort.Port1 && inputPort <= InputPort.Port8);
        }
    }
}
