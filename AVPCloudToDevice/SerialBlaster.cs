using Microsoft.Azure.Devices;
using ControllableDeviceTypes.SerialBlasterTypes;

namespace AVPCloudToDevice
{
    public class SerialBlaster
    {
        private ServiceClient _serviceClient;
        private string _deviceId;
        private uint _deviceIndex;

        public SerialBlaster(ServiceClient serviceClient, string deviceId, uint deviceIndex)
        {
            _serviceClient = serviceClient;
            _deviceId = deviceId;
            _deviceIndex = deviceIndex;
        }

        public bool GetAvailable()
        {
            try
            {
                var payload = new { _deviceIndex };

                var response = Utilities.InvokeMethodWithObjectPayload(_serviceClient, _deviceId, "SerialBlasterGetAvailable", payload);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool SendMessage(string message)
        {
            try
            {
                var payload = new{ _deviceIndex, message};

                var response = Utilities.InvokeMethodWithObjectPayload(_serviceClient, _deviceId, "SerialBlasterSendMessage", payload);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool SendCommand(Protocol protocol, uint command, uint repeats)
        {
            try
            {
                var payload = new { _deviceIndex, protocol, command, repeats };

                var response = Utilities.InvokeMethodWithObjectPayload(_serviceClient, _deviceId, "SerialBlasterSendCommand", payload);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
