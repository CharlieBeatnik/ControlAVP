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
    class RetroTink4KSerialCloudInterface : DeviceCloudInterface
    {
        private readonly RetroTink4KSerial _device;

        public RetroTink4KSerialCloudInterface(RetroTink4KSerial device)
        {
            _device = device;
        }

        public override IEnumerable<MethodHandlerInfo> GetMethodHandlerInfos(DeviceClient deviceClient)
        {
            yield return new MethodHandlerInfo("RetroTink4KSerialGetAvailable", GetAvailable);
            yield return new MethodHandlerInfo("RetroTink4KSerialSendCommand", SendCommand);
            yield return new MethodHandlerInfo("RetroTink4KSerialLoadProfile", LoadProfile);
            yield return new MethodHandlerInfo("RetroTink4KSerialTurnOn", TurnOn);
            yield return new MethodHandlerInfo("RetroTink4KSerialTurnOn", TurnOff);
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
                success = _device.SendCommand(payload.commandName);
            }

            return methodRequest.GetMethodResponse(success);
        }

        private Task<MethodResponse> LoadProfile(MethodRequest methodRequest, object userContext)
        {
            bool success = false;
            var payloadDefinition = new
            {
                profileIndex = (uint)(0)
            };

            var payload = JsonConvert.DeserializeAnonymousType(methodRequest.DataAsJson, payloadDefinition);
            success = _device.LoadProfile(payload.profileIndex);

            return methodRequest.GetMethodResponse(success);
        }

        private Task<MethodResponse> TurnOn(MethodRequest methodRequest, object userContext)
        {
            var success = _device.TurnOn();
            return methodRequest.GetMethodResponse(success);
        }

        private Task<MethodResponse> TurnOff(MethodRequest methodRequest, object userContext)
        {
            var success = _device.TurnOff();
            return methodRequest.GetMethodResponse(success);
        }
    }
}