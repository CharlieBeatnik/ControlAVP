using Microsoft.Azure.Devices;
using ControllableDeviceTypes.SonyKDL60W855Types;
using Newtonsoft.Json;

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

        public InputPort? GetInputPort()
        {
            try
            {
                var response = Utilities.InvokeMethod(_serviceClient, _deviceId, "TVGetInputPort", null);
                string json = response.GetPayloadAsJson();
                return JsonConvert.DeserializeObject<InputPort>(json);
            }
            catch
            {
                return null;
            }
        }

        public bool SetInputPort(InputPort inputPort)
        {
            try
            {
                var payload = new { inputPort };
                var response = Utilities.InvokeMethod(_serviceClient, _deviceId, "TVSetInputPort", payload);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public PowerStatus? GetPowerStatus()
        {
            try
            {
                var response = Utilities.InvokeMethod(_serviceClient, _deviceId, "TVGetPowerStatus", null);
                string json = response.GetPayloadAsJson();
                return JsonConvert.DeserializeObject<PowerStatus>(json);
            }
            catch
            {
                return null;
            }
        }
    }
}
