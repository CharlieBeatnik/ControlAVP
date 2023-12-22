using ControllableDeviceTypes.ExtronDSC301HDTypes;
using Microsoft.Azure.Devices;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Numerics;

namespace AVPCloudToDevice
{
    public class ExtronDSC301HD(ServiceClient serviceClient, string deviceId)
    {
        private readonly ServiceClient _serviceClient = serviceClient;
        private readonly string _deviceId = deviceId;

        public Version GetFirmware()
        {
            try
            {
                var response = Utilities.InvokeMethodWithObjectPayload(_serviceClient, _deviceId, "ScalerGetFirmware", null);
                string json = response.GetPayloadAsJson();
                return JsonConvert.DeserializeObject<Version>(json, new VersionConverter());
            }
            catch
            {
                return null;
            }
        }

        public Vector2? GetInputResolution()
        {
            try
            {
                var response = Utilities.InvokeMethodWithObjectPayload(_serviceClient, _deviceId, "ScalerGetInputResolution", null);
                string json = response.GetPayloadAsJson();
                return JsonConvert.DeserializeObject<Vector2>(json);
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
                var response = Utilities.InvokeMethodWithObjectPayload(_serviceClient, _deviceId, "ScalerGetAvailable", null);
                string json = response.GetPayloadAsJson();
                return JsonConvert.DeserializeObject<bool>(json);
            }
            catch
            {
                return false;
            }
        }

        public bool SetScale(ScaleType scaleType, PositionType positionType, AspectRatio aspectRatio, Vector2 padding)
        {
            try
            {
                var payload = new { scaleType, positionType, aspectRatio, padding};
                var response = Utilities.InvokeMethodWithObjectPayload(_serviceClient, _deviceId, "ScalerSetScale", payload);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool SetOutputRate(Edid outputRate)
        {
            ArgumentNullException.ThrowIfNull(outputRate);

            try
            {
                var payload = new { outputRate.Id };
                var response = Utilities.InvokeMethodWithObjectPayload(_serviceClient, _deviceId, "ScalerSetOutputRate", payload);
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
                var response = Utilities.InvokeMethodWithObjectPayload(_serviceClient, _deviceId, "ScalerGetInputPort", null);
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
                var response = Utilities.InvokeMethodWithObjectPayload(_serviceClient, _deviceId, "ScalerSetInputPort", payload);
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
                var response = Utilities.InvokeMethodWithObjectPayload(_serviceClient, _deviceId, "ScalerGetTemperature", null);
                string json = response.GetPayloadAsJson();
                return JsonConvert.DeserializeObject<float>(json);
            }
            catch
            {
                return null;
            }
        }

        public int? GetDetailFilter()
        {
            try
            {
                var response = Utilities.InvokeMethodWithObjectPayload(_serviceClient, _deviceId, "ScalerGetDetailFilter", null);
                string json = response.GetPayloadAsJson();
                return JsonConvert.DeserializeObject<int>(json);
            }
            catch
            {
                return null;
            }
        }

        public bool SetDetailFilter(int value)
        {
            try
            {
                var payload = new { value };
                var response = Utilities.InvokeMethodWithObjectPayload(_serviceClient, _deviceId, "ScalerSetDetailFilter", payload);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public int? GetBrightness()
        {
            try
            {
                var response = Utilities.InvokeMethodWithObjectPayload(_serviceClient, _deviceId, "ScalerGetBrightness", null);
                string json = response.GetPayloadAsJson();
                return JsonConvert.DeserializeObject<int>(json);
            }
            catch
            {
                return null;
            }
        }

        public bool SetBrightness(int value)
        {
            try
            {
                var payload = new { value };
                var response = Utilities.InvokeMethodWithObjectPayload(_serviceClient, _deviceId, "ScalerSetBrightness", payload);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public int? GetContrast()
        {
            try
            {
                var response = Utilities.InvokeMethodWithObjectPayload(_serviceClient, _deviceId, "ScalerGetContrast", null);
                string json = response.GetPayloadAsJson();
                return JsonConvert.DeserializeObject<int>(json);
            }
            catch
            {
                return null;
            }
        }

        public bool SetContrast(int value)
        {
            try
            {
                var payload = new { value };
                var response = Utilities.InvokeMethodWithObjectPayload(_serviceClient, _deviceId, "ScalerSetContrast", payload);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool? GetFreeze()
        {
            try
            {
                var response = Utilities.InvokeMethodWithObjectPayload(_serviceClient, _deviceId, "ScalerGetFreeze", null);
                string json = response.GetPayloadAsJson();
                return JsonConvert.DeserializeObject<bool>(json);
            }
            catch
            {
                return null;
            }
        }

        public bool SetFreeze(bool freeze)
        {
            try
            {
                var payload = new { freeze };
                var response = Utilities.InvokeMethodWithObjectPayload(_serviceClient, _deviceId, "ScalerSetFreeze", payload);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}