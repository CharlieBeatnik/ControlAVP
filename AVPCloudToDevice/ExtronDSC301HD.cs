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
        public bool SetScale(ScaleType scaleType, PositionType positionType)
        {
            try
            {
                var payload = new { scaleType, positionType };
                var response = Utilities.InvokeMethod(_serviceClient, _deviceId, "ScalerSetScale", payload);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool SetOutputRate(Edid outputRate)
        {
            if(outputRate == null)
            {
                throw new ArgumentNullException(nameof(outputRate));
            }
            
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

        public InputPort? GetInputPort()
        {
            try
            {
                var response = Utilities.InvokeMethod(_serviceClient, _deviceId, "ScalerGetInputPort", null);
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
                var response = Utilities.InvokeMethod(_serviceClient, _deviceId, "ScalerSetInputPort", payload);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public float? GetTemperature()
        {
            try
            {
                var response = Utilities.InvokeMethod(_serviceClient, _deviceId, "ScalerGetTemperature", null);
                string json = response.GetPayloadAsJson();
                return JsonConvert.DeserializeObject<float>(json);
            }
            catch
            {
                return null;
            }
        }
    }
}