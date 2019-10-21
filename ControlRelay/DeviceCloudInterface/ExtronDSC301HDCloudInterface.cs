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
        }

        private Task<MethodResponse> GetFirmware(MethodRequest methodRequest, object userContext)
        {
            var result = _device.GetFirmware();

            if (result != null)
            {
                string json = JsonConvert.SerializeObject(result);
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
            };

            var payload = JsonConvert.DeserializeAnonymousType(methodRequest.DataAsJson, payloadDefintion);

            if(payload.ScaleType.Valid() && payload.PositionType.Valid())
            {
                success = _device.Scale(payload.ScaleType, payload.PositionType);
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

    }
}