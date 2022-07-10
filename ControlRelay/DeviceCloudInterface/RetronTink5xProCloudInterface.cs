using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ControllableDevice;
using ControllableDeviceTypes.RetroTink5xProTypes;
using Microsoft.Azure.Devices.Client;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace ControlRelay
{
    class RetroTink5xProCloudInterface : DeviceCloudInterface
    {
        private RetroTink5xPro _device;

        public RetroTink5xProCloudInterface(RetroTink5xPro device)
        {
            _device = device;
        }

        public override IEnumerable<MethodHandlerInfo> GetMethodHandlerInfos(DeviceClient deviceClient)
        {
            yield return new MethodHandlerInfo("RetroTink5xProGetAvailable", GetAvailable);
            yield return new MethodHandlerInfo("RetroTink5xProSendCommand", SendCommand);
            yield return new MethodHandlerInfo("RetroTink5xProLoadProfile", LoadProfile);
            yield return new MethodHandlerInfo("RetroTink5xProSendCountOfCommandWithDelay", SendCountOfCommandWithDelay);
        }

        private Task<MethodResponse> GetAvailable(MethodRequest methodRequest, object userContext)
        {
            var result = _device.GetAvailable();

            return methodRequest.GetMethodResponseSerialize(true, result);
        }

        private Task<MethodResponse> SendCommand(MethodRequest methodRequest, object userContext)
        {
            bool success = false;
            var payloadDefintion = new
            {
                commandName = (CommandName)(-1),
                repeats = (uint)(0),
            };

            var payload = JsonConvert.DeserializeAnonymousType(methodRequest.DataAsJson, payloadDefintion);

            if (payload.commandName.Valid())
            {
                success = _device.SendCommand(payload.commandName, payload.repeats);
            }

            return methodRequest.GetMethodResponse(success);
        }

        private Task<MethodResponse> LoadProfile(MethodRequest methodRequest, object userContext)
        {
            bool success = false;
            var payloadDefintion = new
            {
                profileName = (ProfileName)(-1),
            };

            var payload = JsonConvert.DeserializeAnonymousType(methodRequest.DataAsJson, payloadDefintion);

            if (payload.profileName.Valid())
            {
                success = _device.LoadProfile(payload.profileName);
            }

            return methodRequest.GetMethodResponse(success);
        }

        private Task<MethodResponse> SendCountOfCommandWithDelay(MethodRequest methodRequest, object userContext)
        {
            bool success = false;
            var payloadDefintion = new
            {
                commandName = (CommandName)(-1),
                count = 0,
                postSendDelay = TimeSpan.FromSeconds(0),
                repeats = (uint)(0),
            };

            var payload = JsonConvert.DeserializeAnonymousType(methodRequest.DataAsJson, payloadDefintion);

            if (payload.commandName.Valid())
            {
                success = _device.SendCountOfCommandWithDelay(payload.commandName, payload.count, payload.postSendDelay, payload.repeats);
            }

            return methodRequest.GetMethodResponse(success);
        }
    }
}