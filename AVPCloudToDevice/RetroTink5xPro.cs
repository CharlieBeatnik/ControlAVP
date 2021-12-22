using Microsoft.Azure.Devices;
using Newtonsoft.Json;
using ControllableDeviceTypes.RetroTink5xProTypes;


namespace AVPCloudToDevice
{
    public class RetroTink5xPro
    {
        private ServiceClient _serviceClient;
        private string _deviceId;

        public RetroTink5xPro(ServiceClient serviceClient, string deviceId)
        {
            _serviceClient = serviceClient;
            _deviceId = deviceId;
        }

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
    }
}
