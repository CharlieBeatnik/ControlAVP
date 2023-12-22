using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using ControllableDevice;
using ControllableDeviceTypes.ExtronMVX44VGATypes;
using Microsoft.Azure.Devices.Client;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace ControlRelay
{
    class ExtronMVX44VGACloudInterface : DeviceCloudInterface
    {
        private readonly ExtronMVX44VGA _device;

        public ExtronMVX44VGACloudInterface(ExtronMVX44VGA device)
        {
            _device = device;
        }

        public override IEnumerable<MethodHandlerInfo> GetMethodHandlerInfos(DeviceClient deviceClient)
        {
            yield return new MethodHandlerInfo("VGAMatrixGetFirmware", GetFirmware);
            yield return new MethodHandlerInfo("VGAMatrixGetAvailable", GetAvailable);
            yield return new MethodHandlerInfo("VGAMatrixGetTieState", GetTieState);
            yield return new MethodHandlerInfo("VGAMatrixTieInputPortToAllOutputPorts", TieInputPortToAllOutputPorts);
            yield return new MethodHandlerInfo("VGAMatrixTieInputPortToOutputPort", TieInputPortToOutputPort);
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

        private Task<MethodResponse> GetTieState(MethodRequest methodRequest, object userContext)
        {
            return methodRequest.Get(() => _device.GetTieState());
        }

        private Task<MethodResponse> TieInputPortToAllOutputPorts(MethodRequest methodRequest, object userContext)
        {
            bool success = false;
            var payloadDefintion = new
            {
                inputPort = (InputPort)(-1),
                tieType = (TieType)(-1),
            };

            var payload = JsonConvert.DeserializeAnonymousType(methodRequest.DataAsJson, payloadDefintion);

            if (payload.inputPort.Valid() && payload.tieType.Valid())
            {
                success = _device.TieInputPortToAllOutputPorts(payload.inputPort, payload.tieType);
            }

            return methodRequest.GetMethodResponse(success);
        }

        private Task<MethodResponse> TieInputPortToOutputPort(MethodRequest methodRequest, object userContext)
        {
            bool success = false;
            var payloadDefintion = new
            {
                inputPort = (InputPort)(-1),
                outputPort = (OutputPort)(-1),
                tieType = (TieType)(-1),
            };

            var payload = JsonConvert.DeserializeAnonymousType(methodRequest.DataAsJson, payloadDefintion);

            if (payload.inputPort.Valid() && payload.outputPort.Valid() && payload.tieType.Valid())
            {
                success = _device.TieInputPortToOutputPort(payload.inputPort, payload.outputPort, payload.tieType);
            }

            return methodRequest.GetMethodResponse(success);
        }

    }
}