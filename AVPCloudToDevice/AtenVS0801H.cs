using Microsoft.Azure.Devices;
using Newtonsoft.Json;
using ControllableDeviceTypes.AtenVS0801HTypes;


namespace AVPCloudToDevice
{
    public class AtenVS0801H(ServiceClient serviceClient, string deviceId, uint deviceIndex)
    {
        private readonly ServiceClient _serviceClient = serviceClient;
        private readonly string _deviceId = deviceId;
        private readonly uint _deviceIndex = deviceIndex;

        public bool GoToNextInput()
        {
            try
            {
                var payload = new { _deviceIndex };

                var response = Utilities.InvokeMethodWithObjectPayload(_serviceClient, _deviceId, "AtenVS0801HGoToNextInput", payload);
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

                var response = Utilities.InvokeMethodWithObjectPayload(_serviceClient, _deviceId, "AtenVS0801HGoToPreviousInput", payload);
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

                var response = Utilities.InvokeMethodWithObjectPayload(_serviceClient, _deviceId, "AtenVS0801HGetState", payload);
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

                var response = Utilities.InvokeMethodWithObjectPayload(_serviceClient, _deviceId, "AtenVS0801HSetInputPort", payload);
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

                var response = Utilities.InvokeMethodWithObjectPayload(_serviceClient, _deviceId, "AtenVS0801HGetAvailable", payload);
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
