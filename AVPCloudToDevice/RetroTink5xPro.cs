using Microsoft.Azure.Devices;
using Newtonsoft.Json;
using ControllableDeviceTypes.RetroTink5xProTypes;
using System;

namespace AVPCloudToDevice
{
    public class RetroTink5xPro(ServiceClient serviceClient, string deviceId)
    {
        private readonly ServiceClient _serviceClient = serviceClient;
        private readonly string _deviceId = deviceId;

        public bool SendCommand(CommandName commandName)
        {
            try
            {
                var payload = new { commandName };
                var response = Utilities.InvokeMethodWithObjectPayload(_serviceClient, _deviceId, "RetroTink5xProSendCommand", payload);
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
                var response = Utilities.InvokeMethodWithObjectPayload(_serviceClient, _deviceId, "RetroTink5xProGetAvailable", null);
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
                var response = Utilities.InvokeMethodWithObjectPayload(_serviceClient, _deviceId, "RetroTink5xProLoadProfile", payload);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool SendCountOfCommandWithDelay(CommandName commandName, int count, TimeSpan postSendDelay)
        {
            try
            {
                var payload = new { commandName, count, postSendDelay };
                var response = Utilities.InvokeMethodWithObjectPayload(_serviceClient, _deviceId, "RetroTink5xProSendCountOfCommandWithDelay", payload);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
