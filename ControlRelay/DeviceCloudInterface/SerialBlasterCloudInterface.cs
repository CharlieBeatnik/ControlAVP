using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using ControllableDevice;
using ControllableDeviceTypes.SerialBlasterTypes;
using Microsoft.Azure.Devices.Client;
using Newtonsoft.Json;

namespace ControlRelay
{
    class SerialBlasterCloudInterface : DeviceCloudInterface
    {
        private List<SerialBlaster> _devices = new List<SerialBlaster>();

        public SerialBlasterCloudInterface(List<SerialBlaster> devices)
        {
            _devices = devices;
        }

        public override IEnumerable<MethodHandlerInfo> GetMethodHandlerInfos(DeviceClient deviceClient)
        {
            yield return new MethodHandlerInfo("SerialBlasterGetAvailable", GetAvailable);
            yield return new MethodHandlerInfo("SerialBlasterSendCommand", SendCommand);
            yield return new MethodHandlerInfo("SerialBlasterSendMessage", SendMessage);
        }

        private Task<MethodResponse> GetAvailable(MethodRequest methodRequest, object userContext)
        {
            var payloadDefintion = new { _deviceIndex = -1 };

            var payload = JsonConvert.DeserializeAnonymousType(methodRequest.DataAsJson, payloadDefintion);
            if (DeviceIndexValid(payload._deviceIndex))
            {
                bool success = _devices[payload._deviceIndex].GetAvailable();
                return methodRequest.GetMethodResponse(success);
            }

            return methodRequest.GetMethodResponse(false);
        }

        private Task<MethodResponse> SendCommand(MethodRequest methodRequest, object userContext)
        {
            var payloadDefintion = new
            {
                _deviceIndex = -1,
                protocol = (Protocol)(-1),
                command = (uint)(0),
                repeats = (uint)(0),
            };

            var payload = JsonConvert.DeserializeAnonymousType(methodRequest.DataAsJson, payloadDefintion);
            if (DeviceIndexValid(payload._deviceIndex))
            {
                bool success = _devices[payload._deviceIndex].SendCommand(payload.protocol, payload.command, payload.repeats);
                return methodRequest.GetMethodResponse(success);
            }

            return methodRequest.GetMethodResponse(false);
        }

        private Task<MethodResponse> SendMessage(MethodRequest methodRequest, object userContext)
        {
            var payloadDefintion = new
            {
                _deviceIndex = -1,
                message = string.Empty
            };

            var payload = JsonConvert.DeserializeAnonymousType(methodRequest.DataAsJson, payloadDefintion);
            if (DeviceIndexValid(payload._deviceIndex))
            {
                bool success = _devices[payload._deviceIndex].SendMessage(payload.message);
                return methodRequest.GetMethodResponse(success);
            }

            return methodRequest.GetMethodResponse(false);
        }

        private bool DeviceIndexValid(int deviceIndex)
        {
            return (deviceIndex >= 0 && deviceIndex < _devices.Count);
        }
    }
}
