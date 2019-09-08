using Microsoft.Azure.Devices;
using Newtonsoft.Json;
using ControllableDeviceTypes.OSSCTypes;


namespace AVPCloudToDevice
{
    public class OSSC
    {
        private ServiceClient _serviceClient;
        private string _deviceId;

        public OSSC(ServiceClient serviceClient, string deviceId)
        {
            _serviceClient = serviceClient;
            _deviceId = deviceId;
        }

        public bool SendCommand(CommandName commandName)
        {
            try
            {
                var payload = new { commandName };
                var response = Utilities.InvokeMethod(_serviceClient, _deviceId, "OSSCSendCommand", payload);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
