using ControllableDeviceTypes.ExtronDSC301HDTypes;
using Microsoft.Azure.Devices;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
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
                var response = Utilities.InvokeMethod(_serviceClient, _deviceId, "ScalerGetAvailable", null);
                string json = response.GetPayloadAsJson();
                return JsonConvert.DeserializeObject<bool>(json);
            }
            catch
            {
                return false;
            }
        }
        public bool SetPixelPerfectAndCentered()
        {
            try
            {
                var response = Utilities.InvokeMethod(_serviceClient, _deviceId, "ScalerSetPixelPerfectAndCentered", null);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool SetOutputRate(Edid outputRate)
        {
            try
            {
                var payload = new { outputRate.Id };
                var response = Utilities.InvokeMethod(_serviceClient, _deviceId, "ScalerSetOutputRate", payload);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}