using Microsoft.Azure.Devices;
using Newtonsoft.Json;
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
                var response = Utilities.InvokeMethod(_serviceClient, _deviceId, "VGAMatrixGetFirmware", null);
                string json = response.GetPayloadAsJson();
                return JsonConvert.DeserializeObject<Version>(json);
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
                var response = Utilities.InvokeMethod(_serviceClient, _deviceId, "VGAMatrixGetAvailable", null);
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