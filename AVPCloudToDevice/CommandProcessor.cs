using Microsoft.Azure.Devices;
using Newtonsoft.Json;
using ControllableDeviceTypes.OSSCTypes;


namespace AVPCloudToDevice
{
    public class CommandProcessor
    {
        private ServiceClient _serviceClient;
        private string _deviceId;

        public CommandProcessor(ServiceClient serviceClient, string deviceId)
        {
            _serviceClient = serviceClient;
            _deviceId = deviceId;
        }

        public bool Execute(string json)
        {
            try
            {
                var response = Utilities.InvokeMethodWithJsonPayload(_serviceClient, _deviceId, "CommandProcessorExecute", json);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
