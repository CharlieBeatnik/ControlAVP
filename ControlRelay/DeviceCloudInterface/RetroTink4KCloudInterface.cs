using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ControllableDevice;
using ControllableDeviceTypes.RetroTink4KTypes;
using Microsoft.Azure.Devices.Client;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace ControlRelay
{
    class RetroTink4KCloudInterface : DeviceCloudInterface
    {
        private readonly RetroTink4K _device;

        public RetroTink4KCloudInterface(RetroTink4K device)
        {
            _device = device;
        }

        public override IEnumerable<MethodHandlerInfo> GetMethodHandlerInfos(DeviceClient deviceClient)
        {
            yield return new MethodHandlerInfo("RetroTink4KGetAvailable", GetAvailable);
            yield return new MethodHandlerInfo("RetroTink4KSendCommand", SendCommand);
            yield return new MethodHandlerInfo("RetroTink4KLoadProfileQuick", LoadProfileQuick);
            yield return new MethodHandlerInfo("RetroTink4KLoadProfile", LoadProfile);
            yield return new MethodHandlerInfo("RetroTink4KTogglePower", TogglePower);
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

        private Task<MethodResponse> LoadProfileQuick(MethodRequest methodRequest, object userContext)
        {
            bool success = false;
            var payloadDefinition = new
            {
                profileName = (ProfileName)(-1),
            };

            var payload = JsonConvert.DeserializeAnonymousType(methodRequest.DataAsJson, payloadDefinition);

            if (payload.profileName.Valid())
            {
                success = _device.LoadProfileQuick(payload.profileName);
            }

            return methodRequest.GetMethodResponse(success);
        }

        private Task<MethodResponse> LoadProfile(MethodRequest methodRequest, object userContext)
        {
            bool success = false;
            var payloadDefinition = new
            {
                directoryIndex = (uint)(0),
                profileIndex = (uint)(0)
            };

            var payload = JsonConvert.DeserializeAnonymousType(methodRequest.DataAsJson, payloadDefinition);

            success = _device.LoadProfile(payload.directoryIndex, payload.profileIndex);

            return methodRequest.GetMethodResponse(success);
        }

        private Task<MethodResponse> TogglePower(MethodRequest methodRequest, object userContext)
        {
            var success = _device.ToggerPower();

            return methodRequest.GetMethodResponse(success);
        }
    }
}