using System.Collections.Generic;
using System.Threading.Tasks;
using ControllableDevice;
using ControllableDeviceTypes.OSSCTypes;
using Microsoft.Azure.Devices.Client;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace ControlRelay
{
    class OSSCCloudInterface : DeviceCloudInterface
    {
        private OSSC _device;

        public struct Settings
        {
            public string PortId { get; set; }
        }

        private Settings _settings;

        public OSSCCloudInterface(Settings settings)
        {
            _settings = settings;
            _device = new OSSC(_settings.PortId);
        }

        public override IEnumerable<MethodHandlerInfo> GetMethodHandlerInfos(DeviceClient deviceClient)
        {
            yield return new MethodHandlerInfo("OSSCGetAvailable", GetAvailable);
            yield return new MethodHandlerInfo("OSSCSendCommand", SendCommand);
        }

        private Task<MethodResponse> GetAvailable(MethodRequest methodRequest, object userContext)
        {
            var result = _device.GetAvailable();

            return GetMethodResponseSerialize(methodRequest, true, result);
        }

        private Task<MethodResponse> SendCommand(MethodRequest methodRequest, object userContext)
        {
            bool success = false;
            var payloadDefintion = new
            {
                commandName = (CommandName)(-1),
            };

            var payload = JsonConvert.DeserializeAnonymousType(methodRequest.DataAsJson, payloadDefintion);

            if (payload.commandName.Valid())
            {
                success = _device.SendCommand(payload.commandName);
            }

            return GetMethodResponse(methodRequest, success);
        }
    }
}