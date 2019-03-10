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
        private ExtronMVX44VGA _device;

        public struct Settings
        {
            public string PortId { get; set; }
        }

        private Settings _settings;

        public ExtronMVX44VGACloudInterface(Settings settings)
        {
            _settings = settings;
            _device = new ExtronMVX44VGA(_settings.PortId);
        }

        public override void SetMethodHandlers(DeviceClient deviceClient)
        {
            deviceClient.SetMethodHandlerAsync("VGAMatrixGetFirmware", GetFirmware, null).Wait();
            deviceClient.SetMethodHandlerAsync("VGAMatrixGetAvailable", GetAvailable, null).Wait();
            deviceClient.SetMethodHandlerAsync("VGAMatrixGetTieState", GetTieState, null).Wait();
            deviceClient.SetMethodHandlerAsync("VGAMatrixTieInputPortToAllOutputPorts", TieInputPortToAllOutputPorts, null).Wait();
            deviceClient.SetMethodHandlerAsync("VGAMatrixTieInputPortToOutputPort", TieInputPortToOutputPort, null).Wait();
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

        private Task<MethodResponse> GetTieState(MethodRequest methodRequest, object userContext)
        {
            return Get(methodRequest, () => _device.GetTieState());
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

            return GetMethodResponse(methodRequest, success);
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

            return GetMethodResponse(methodRequest, success);
        }

    }
}