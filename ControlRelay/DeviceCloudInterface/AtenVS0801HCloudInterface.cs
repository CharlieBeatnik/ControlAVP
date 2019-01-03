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
            deviceClient.SetMethodHandlerAsync("HDMISwitchGoToNextInput", HDMISwitchGoToNextInput, null).Wait();
            deviceClient.SetMethodHandlerAsync("HDMISwitchGoToPreviousInput", HDMISwitchGoToPreviousInput, null).Wait();
            deviceClient.SetMethodHandlerAsync("HDMISwitchGetState", HDMISwitchGetState, null).Wait();
            deviceClient.SetMethodHandlerAsync("HDMISwitchSetInput", HDMISwitchSetInput, null).Wait();
        }

        private Task<MethodResponse> HDMISwitchGoToNextInput(MethodRequest methodRequest, object userContext)
        {
            var payloadDefintion = new { _hdmiSwitchId = -1 };

            var payload = JsonConvert.DeserializeAnonymousType(methodRequest.DataAsJson, payloadDefintion);
            bool success = _devices[payload._hdmiSwitchId].GoToNextInput();
            return Task.FromResult(GetMethodResponse(methodRequest, success));
        }

        private Task<MethodResponse> HDMISwitchGoToPreviousInput(MethodRequest methodRequest, object userContext)
        {
            var payloadDefintion = new { _hdmiSwitchId = -1 };

            var payload = JsonConvert.DeserializeAnonymousType(methodRequest.DataAsJson, payloadDefintion);
            bool success = _devices[payload._hdmiSwitchId].GoToPreviousInput();
            return Task.FromResult(GetMethodResponse(methodRequest, success));
        }

        private Task<MethodResponse> HDMISwitchGetState(MethodRequest methodRequest, object userContext)
        {
            var payloadDefintion = new { _hdmiSwitchId = -1 };

            var payload = JsonConvert.DeserializeAnonymousType(methodRequest.DataAsJson, payloadDefintion);

            var result = _devices[payload._hdmiSwitchId].GetState();
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

        private Task<MethodResponse> HDMISwitchSetInput(MethodRequest methodRequest, object userContext)
        {
            var payloadDefintion = new
            {
                _hdmiSwitchId = -1,
                inputPort = InputPort.Port1
            };

            var payload = JsonConvert.DeserializeAnonymousType(methodRequest.DataAsJson, payloadDefintion);
            bool success = _devices[payload._hdmiSwitchId].SetInput(payload.inputPort);
            return Task.FromResult(GetMethodResponse(methodRequest, success));
        }
    }
}
