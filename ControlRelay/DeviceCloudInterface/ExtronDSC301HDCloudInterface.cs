using System.Collections.Generic;
using System.Net;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using ControllableDevice;
using ControllableDeviceTypes.ExtronDSC301HDTypes;
using Microsoft.Azure.Devices.Client;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace ControlRelay
{
    class ExtronDSC301HDCloudInterface : DeviceCloudInterface
    {
        private ExtronDSC301HD _device;

        public ExtronDSC301HDCloudInterface(ExtronDSC301HD device)
        {
            _device = device;
        }

        public override IEnumerable<MethodHandlerInfo> GetMethodHandlerInfos(DeviceClient deviceClient)
        {
            yield return new MethodHandlerInfo("ScalerGetFirmware", GetFirmware);
            yield return new MethodHandlerInfo("ScalerGetAvailable", GetAvailable);
            yield return new MethodHandlerInfo("ScalerSetScale", SetScale);
            yield return new MethodHandlerInfo("ScalerSetOutputRate", SetOutputRate);
            yield return new MethodHandlerInfo("ScalerGetInputPort", GetInputPort);
            yield return new MethodHandlerInfo("ScalerSetInputPort", SetInputPort);
            yield return new MethodHandlerInfo("ScalerGetTemperature", GetTemperature);
            yield return new MethodHandlerInfo("ScalerSetDetailFilter", SetDetailFilter);
            yield return new MethodHandlerInfo("ScalerGetDetailFilter", GetDetailFilter);
            yield return new MethodHandlerInfo("ScalerSetBrightness", SetBrightness);
            yield return new MethodHandlerInfo("ScalerGetBrightness", GetBrightness);
            yield return new MethodHandlerInfo("ScalerSetContrast", SetContrast);
            yield return new MethodHandlerInfo("ScalerGetContrast", GetContrast);
            yield return new MethodHandlerInfo("ScalerSetFreeze", SetFreeze);
            yield return new MethodHandlerInfo("ScalerGetFreeze", GetFreeze);
            yield return new MethodHandlerInfo("ScalerGetInputResolution", GetInputResolution);
        }

        private Task<MethodResponse> GetFirmware(MethodRequest methodRequest, object userContext)
        {
            var result = _device.GetFirmware();

            if (result != null)
            {
                string json = JsonConvert.SerializeObject(result, new VersionConverter());
                return methodRequest.GetMethodResponse(true, json);
            }
            else
            {
                return methodRequest.GetMethodResponse(false);
            }
        }

        private Task<MethodResponse> GetAvailable(MethodRequest methodRequest, object userContext)
        {
            var result = _device.GetAvailable();
            return methodRequest.GetMethodResponseSerialize(true, result);
        }

        private Task<MethodResponse> SetScale(MethodRequest methodRequest, object userContext)
        {
            bool success = false;
            var payloadDefintion = new
            {
                ScaleType = (ScaleType)(-1),
                PositionType = (PositionType)(-1),
                AspectRatio = (AspectRatio)(-1),
                Padding = new Vector2(-1)
            };

            var payload = JsonConvert.DeserializeAnonymousType(methodRequest.DataAsJson, payloadDefintion);

            if(payload.ScaleType.Valid() && payload.PositionType.Valid())
            {
                success = _device.Scale(payload.ScaleType, payload.PositionType, payload.AspectRatio, payload.Padding);
            }

            return methodRequest.GetMethodResponse(success);
        }

        private Task<MethodResponse> SetOutputRate(MethodRequest methodRequest, object userContext)
        {
            bool success = false;
            var payloadDefintion = new
            {
                Id = 0,
            };

            var payload = JsonConvert.DeserializeAnonymousType(methodRequest.DataAsJson, payloadDefintion);

            var edid = Edid.GetEdid(payload.Id);
            if(edid != null)
            {
                success = _device.SetOutputRate(edid);
            }

            return methodRequest.GetMethodResponse(success);
        }

        private Task<MethodResponse> SetInputPort(MethodRequest methodRequest, object userContext)
        {
            bool success = false;
            var payloadDefintion = new
            {
                inputPort = (InputPort)(-1),
            };

            var payload = JsonConvert.DeserializeAnonymousType(methodRequest.DataAsJson, payloadDefintion);

            if (payload.inputPort.Valid())
            {
                success = _device.SetInputPort(payload.inputPort);
            }
            return methodRequest.GetMethodResponse(success);
        }

        private Task<MethodResponse> GetInputPort(MethodRequest methodRequest, object userContext)
        {
            return methodRequest.Get(() => _device.GetInputPort());
        }

        private Task<MethodResponse> GetTemperature(MethodRequest methodRequest, object userContext)
        {
            return methodRequest.Get(() => _device.GetTemperature());
        }

        private Task<MethodResponse> GetInputResolution(MethodRequest methodRequest, object userContext)
        {
            var resolution = new Vector2();

            resolution.X = (float)_device.GetActivePixels();
            resolution.Y = (float)_device.GetActiveLines();

            return methodRequest.Get(() => resolution);
        }

        private Task<MethodResponse> SetDetailFilter(MethodRequest methodRequest, object userContext)
        {
            bool success = false;
            var payloadDefintion = new
            {
                Value = -1,
            };

            var payload = JsonConvert.DeserializeAnonymousType(methodRequest.DataAsJson, payloadDefintion);
            success = _device.SetDetailFilter(payload.Value);

            return methodRequest.GetMethodResponse(success);
        }

        private Task<MethodResponse> GetDetailFilter(MethodRequest methodRequest, object userContext)
        {
            return methodRequest.Get(() => _device.GetDetailFilter());
        }

        private Task<MethodResponse> SetBrightness(MethodRequest methodRequest, object userContext)
        {
            bool success = false;
            var payloadDefintion = new
            {
                Value = -1,
            };

            var payload = JsonConvert.DeserializeAnonymousType(methodRequest.DataAsJson, payloadDefintion);
            success = _device.SetBrightness(payload.Value);

            return methodRequest.GetMethodResponse(success);
        }

        private Task<MethodResponse> GetBrightness(MethodRequest methodRequest, object userContext)
        {
            return methodRequest.Get(() => _device.GetBrightness());
        }

        private Task<MethodResponse> SetContrast(MethodRequest methodRequest, object userContext)
        {
            bool success = false;
            var payloadDefintion = new
            {
                Value = -1,
            };

            var payload = JsonConvert.DeserializeAnonymousType(methodRequest.DataAsJson, payloadDefintion);
            success = _device.SetContrast(payload.Value);

            return methodRequest.GetMethodResponse(success);
        }

        private Task<MethodResponse> GetContrast(MethodRequest methodRequest, object userContext)
        {
            return methodRequest.Get(() => _device.GetContrast());
        }

        private Task<MethodResponse> SetFreeze(MethodRequest methodRequest, object userContext)
        {
            bool success = false;
            var payloadDefintion = new
            {
                Freeze = false,
            };

            var payload = JsonConvert.DeserializeAnonymousType(methodRequest.DataAsJson, payloadDefintion);
            success = _device.SetFreeze(payload.Freeze);

            return methodRequest.GetMethodResponse(success);
        }

        private Task<MethodResponse> GetFreeze(MethodRequest methodRequest, object userContext)
        {
            return methodRequest.Get(() => _device.GetFreeze());
        }
    }
}