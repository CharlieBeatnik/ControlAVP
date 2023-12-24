using Microsoft.Azure.Devices;
using Newtonsoft.Json;
using ControllableDeviceTypes.RetroTink4KTypes;
using System;

namespace AVPCloudToDevice
{
    public class RetroTink4K(ServiceClient serviceClient, string deviceId)
    {
        private readonly ServiceClient _serviceClient = serviceClient;
        private readonly string _deviceId = deviceId;

        public bool SendCommand(CommandName commandName)
        {
            try
            {
                var payload = new { commandName };
                var response = Utilities.InvokeMethodWithObjectPayload(_serviceClient, _deviceId, "RetroTink4KSendCommand", payload);
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
                var response = Utilities.InvokeMethodWithObjectPayload(_serviceClient, _deviceId, "RetroTink4KGetAvailable", null);
                string json = response.GetPayloadAsJson();
                return JsonConvert.DeserializeObject<bool>(json);
            }
            catch
            {
                return false;
            }
        }

        public bool LoadProfile(ProfileName profileName)
        {
            try
            {
                var payload = new { profileName };
                var response = Utilities.InvokeMethodWithObjectPayload(_serviceClient, _deviceId, "RetroTink4KLoadProfile", payload);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool TogglePower()
        {
            try
            {
                var response = Utilities.InvokeMethodWithObjectPayload(_serviceClient, _deviceId, "RetroTink4KTogglePower", null);
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
