using ControllableDeviceTypes.ExtronMVX44VGATypes;
using Microsoft.Azure.Devices;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;

namespace AVPCloudToDevice
{
    public class ExtronMVX44VGA
    {
        private ServiceClient _serviceClient;
        private string _deviceId;

        public ExtronMVX44VGA(ServiceClient serviceClient, string deviceId)
        {
            _serviceClient = serviceClient;
            _deviceId = deviceId;
        }

        public Version GetFirmware()
        {
            try
            {
                var response = Utilities.InvokeMethodWithObjectPayload(_serviceClient, _deviceId, "VGAMatrixGetFirmware", null);
                string json = response.GetPayloadAsJson();
                
                return JsonConvert.DeserializeObject<Version>(json, new VersionConverter());
            }
            catch
            {
                return null;
            }
        }

        public bool GetAvailable()
        {
            try
            {
                var response = Utilities.InvokeMethodWithObjectPayload(_serviceClient, _deviceId, "VGAMatrixGetAvailable", null);
                string json = response.GetPayloadAsJson();
                return JsonConvert.DeserializeObject<bool>(json);
            }
            catch
            {
                return false;
            }
        }

        public TieState GetTieState()
        {
            try
            {
                var response = Utilities.InvokeMethodWithObjectPayload(_serviceClient, _deviceId, "VGAMatrixGetTieState", null);
                string json = response.GetPayloadAsJson();
                return JsonConvert.DeserializeObject<TieState>(json);
            }
            catch
            {
                return null;
            }
        }

        public bool TieInputPortToAllOutputPorts(InputPort inputPort, TieType tieType)
        {
            try
            {
                var payload = new { inputPort, tieType };
                var response = Utilities.InvokeMethodWithObjectPayload(_serviceClient, _deviceId, "VGAMatrixTieInputPortToAllOutputPorts", payload);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool TieInputPortToOutputPort(InputPort inputPort, OutputPort outputPort, TieType tieType)
        {
            try
            {
                var payload = new { inputPort, outputPort, tieType };
                var response = Utilities.InvokeMethodWithObjectPayload(_serviceClient, _deviceId, "VGAMatrixTieInputPortToOutputPort", payload);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}