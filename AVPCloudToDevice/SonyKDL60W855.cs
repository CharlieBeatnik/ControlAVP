using Microsoft.Azure.Devices;

namespace AVPCloudToDevice
{
    public class SonyKDL60W855
    {
        private ServiceClient _serviceClient;
        private string _deviceId;

        public SonyKDL60W855(ServiceClient serviceClient, string deviceId)
        {
            _serviceClient = serviceClient;
            _deviceId = deviceId;
        }

        public bool TurnOn()
        {
            try
            {
                var response = Utilities.InvokeMethod(_serviceClient, _deviceId, "TVTurnOn", null);
                return true;
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
                var response = Utilities.InvokeMethod(_serviceClient, _deviceId, "TVTurnOff", null);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
