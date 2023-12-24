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
        private readonly OSSC _device;

        public OSSCCloudInterface(OSSC device)
        {
            _device = device;
        }

        public override IEnumerable<MethodHandlerInfo> GetMethodHandlerInfos(DeviceClient deviceClient)
        {
            yield return new MethodHandlerInfo("OSSCGetAvailable", GetAvailable);
            yield return new MethodHandlerInfo("OSSCSendCommand", SendCommand);
            yield return new MethodHandlerInfo("OSSCLoadProfile", LoadProfile);
        }

        private Task<MethodResponse> GetAvailable(MethodRequest methodRequest, object userContext)
        {
            var result = _device.GetAvailable();

            return methodRequest.GetMethodResponseSerialize(true, result);
        }

        private Task<MethodResponse> SendCommand(MethodRequest methodRequest, object userContext)
        {
            bool success = false;
            var payloadDefinition = new
            {
                commandName = (CommandName)(-1),
                repeats = (uint)(0),
            };

            var payload = JsonConvert.DeserializeAnonymousType(methodRequest.DataAsJson, payloadDefinition);

            if (payload.commandName.Valid())
            {
                success = _device.SendCommand(payload.commandName, payload.repeats);
            }

            return methodRequest.GetMethodResponse(success);
        }

        private Task<MethodResponse> LoadProfile(MethodRequest methodRequest, object userContext)
        {
            bool success = false;
            var payloadDefinition = new
            {
                profileName = (ProfileName)(-1),
            };

            var payload = JsonConvert.DeserializeAnonymousType(methodRequest.DataAsJson, payloadDefinition);

            if (payload.profileName.Valid())
            {
                success = _device.LoadProfile(payload.profileName);
            }

            return methodRequest.GetMethodResponse(success);
        }
    }
}