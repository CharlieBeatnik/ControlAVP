using Microsoft.Azure.Devices;
using Newtonsoft.Json;
using ControllableDeviceTypes.AtenVS0801HBTypes;


namespace AVPCloudToDevice
{
    public class AtenVS0801HB
    {
        private ServiceClient _serviceClient;
        private string _deviceId;
        private uint _deviceIndex;

        public AtenVS0801HB(ServiceClient serviceClient, string deviceId, uint deviceIndex)
        {
            _serviceClient = serviceClient;
            _deviceId = deviceId;
            _deviceIndex = deviceIndex;
        }

        public bool GoToNextInput()
        {
            try
            {
                var payload = new { _deviceIndex };

                var response = Utilities.InvokeMethodWithObjectPayload(_serviceClient, _deviceId, "AtenVS0801HBGoToNextInput", payload);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool GoToPreviousInput()
        {
            try
            {
                var payload = new { _deviceIndex };

                var response = Utilities.InvokeMethodWithObjectPayload(_serviceClient, _deviceId, "AtenVS0801HBGoToPreviousInput", payload);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public State GetState()
        {
            try
            {
                var payload = new { _deviceIndex };

                var response = Utilities.InvokeMethodWithObjectPayload(_serviceClient, _deviceId, "AtenVS0801HBGetState", payload);
                string json = response.GetPayloadAsJson();
                return JsonConvert.DeserializeObject<State>(json);
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
                var payload = new
                {
                    _deviceIndex,
                    inputPort
                };

                var response = Utilities.InvokeMethodWithObjectPayload(_serviceClient, _deviceId, "AtenVS0801HBSetInputPort", payload);
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
                var payload = new { _deviceIndex };

                var response = Utilities.InvokeMethodWithObjectPayload(_serviceClient, _deviceId, "AtenVS0801HBGetAvailable", payload);
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
