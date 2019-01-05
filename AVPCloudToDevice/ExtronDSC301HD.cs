using Microsoft.Azure.Devices;
using Newtonsoft.Json;
using System;

namespace AVPCloudToDevice
{
    public class ExtronDSC301HD
    {
        private ServiceClient _serviceClient;
        private string _deviceId;

        public ExtronDSC301HD(ServiceClient serviceClient, string deviceId)
        {
            _serviceClient = serviceClient;
            _deviceId = deviceId;
        }

        public Version GetFirmware()
        {
            try
            {
                var response = Utilities.InvokeMethod(_serviceClient, _deviceId, "ScalerGetFirmware", null);
                string json = response.GetPayloadAsJson();
                return JsonConvert.DeserializeObject<Version>(json);
            }
            catch
            {
                return null;
            }
        }

        public bool Available
        {
            get
            {
                try
                {
                    var response = Utilities.InvokeMethod(_serviceClient, _deviceId, "ScalerAvailable", null);
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
}