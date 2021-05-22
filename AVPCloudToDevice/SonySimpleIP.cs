using Microsoft.Azure.Devices;
using ControllableDeviceTypes.SonySimpleIPTypes;
using Newtonsoft.Json;
using System;

namespace AVPCloudToDevice
{
    public class SonySimpleIP
    {
        private ServiceClient _serviceClient;
        private string _deviceId;

        public SonySimpleIP(ServiceClient serviceClient, string deviceId)
        {
            _serviceClient = serviceClient;
            _deviceId = deviceId;
        }

        public bool TurnOn()
        {
            try
            {
                // The timeout for this particular call uses a longer timeout as the TV takes a while to boot. The theory with the APIs is
                // that a user can make a call and guarantee that the device is now in that state when it returns, hence we increase the 
                // timeout to make sure it can succeed.
                var response = Utilities.InvokeMethodWithObjectPayload(_serviceClient, _deviceId, "TVTurnOn", null, TimeSpan.FromSeconds(60));
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
                var response = Utilities.InvokeMethodWithObjectPayload(_serviceClient, _deviceId, "TVTurnOff", null);
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
                var response = Utilities.InvokeMethodWithObjectPayload(_serviceClient, _deviceId, "TVGetInputPort", null);
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
                var response = Utilities.InvokeMethodWithObjectPayload(_serviceClient, _deviceId, "TVSetInputPort", payload);
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
                var response = Utilities.InvokeMethodWithObjectPayload(_serviceClient, _deviceId, "TVGetPowerStatus", null);
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
