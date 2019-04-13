using System.Collections.Generic;
using System.Net;
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

        public struct Settings
        {
            public string PortId { get; set; }
        }

        private Settings _settings;

        public ExtronDSC301HDCloudInterface(Settings settings)
        {
            _settings = settings;
            _device = new ExtronDSC301HD(_settings.PortId);
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
        }

        private Task<MethodResponse> GetFirmware(MethodRequest methodRequest, object userContext)
        {
            var result = _device.GetFirmware();

            if (result != null)
            {
                string json = JsonConvert.SerializeObject(result, new VersionConverter());
                return GetMethodResponse(methodRequest, true, json);
            }
            else
            {
                return GetMethodResponse(methodRequest, false);
            }
        }

        private Task<MethodResponse> GetAvailable(MethodRequest methodRequest, object userContext)
        {
            var result = _device.GetAvailable();
            return GetMethodResponseSerialize(methodRequest, true, result);
        }

        private Task<MethodResponse> SetScale(MethodRequest methodRequest, object userContext)
        {
            bool success = false;
            var payloadDefintion = new
            {
                ScaleType = (ScaleType)(-1),
                PositionType = (PositionType)(-1),
            };

            var payload = JsonConvert.DeserializeAnonymousType(methodRequest.DataAsJson, payloadDefintion);

            if(payload.ScaleType.Valid() && payload.PositionType.Valid())
            {
                success = _device.Scale(payload.ScaleType, payload.PositionType);
            }

            return GetMethodResponse(methodRequest, success);
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

            return GetMethodResponse(methodRequest, success);
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
            return GetMethodResponse(methodRequest, success);
        }

        private Task<MethodResponse> GetInputPort(MethodRequest methodRequest, object userContext)
        {
            return Get(methodRequest, () => _device.GetInputPort());
        }

        private Task<MethodResponse> GetTemperature(MethodRequest methodRequest, object userContext)
        {
            return Get(methodRequest, () => _device.GetTemperature());
        }

    }
}