using Microsoft.Azure.Devices;
using Newtonsoft.Json;
using ControllableDeviceTypes.RetroTink4KTypes;
using System;

namespace AVPCloudToDevice
{
    public class RetroTink4KSerial(ServiceClient serviceClient, string deviceId)
    {
        private readonly ServiceClient _serviceClient = serviceClient;
        private readonly string _deviceId = deviceId;

        public bool SendCommand(CommandName commandName)
        {
            try
            {
                var payload = new { commandName };
                var response = Utilities.InvokeMethodWithObjectPayload(_serviceClient, _deviceId, "RetroTink4KSerialSendCommand", payload);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool GetAvailable()
        {
            try
            {
                var response = Utilities.InvokeMethodWithObjectPayload(_serviceClient, _deviceId, "RetroTink4KSerialGetAvailable", null);
                string json = response.GetPayloadAsJson();
                return JsonConvert.DeserializeObject<bool>(json);
            }
            catch
            {
                return false;
            }
        }

        public bool LoadProfile(uint profileIndex)
        {
            try
            {
                var payload = new { profileIndex };
                var response = Utilities.InvokeMethodWithObjectPayload(_serviceClient, _deviceId, "RetroTink4KSerialLoadProfile", payload);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool TurnOn()
        {
            try
            {
                var response = Utilities.InvokeMethodWithObjectPayload(_serviceClient, _deviceId, "RetroTink4KSerialTurnOn", null);
                string json = response.GetPayloadAsJson();
                return JsonConvert.DeserializeObject<bool>(json);
            }
            catch
            {
                return false;
            }
        }

        public bool TurnOff()
        {
            try
            {
                var response = Utilities.InvokeMethodWithObjectPayload(_serviceClient, _deviceId, "RetroTink4KSerialTurnOff", null);
                string json = response.GetPayloadAsJson();
                return JsonConvert.DeserializeObject<bool>(json);
            }
            catch
            {
                return false;
            }
        }
    }
}
