using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using ControllableDevice;
using Microsoft.Azure.Devices.Client;
using Newtonsoft.Json;
using ControllableDeviceTypes.SonyKDL60W855Types;

namespace ControlRelay
{
    class SonyKDL60W855CloudInterface : DeviceCloudInterface
    {
        private SonyKDL60W855 _device;

        public struct Settings
        {
            public string Host { get; set; }
            public string PhysicalAddress { get; set; }
            public string PreSharedKey { get; set; }
        }

        private Settings _settings;

        public SonyKDL60W855CloudInterface(Settings settings)
        {
            _settings = settings;
            _device = new SonyKDL60W855(IPAddress.Parse(_settings.Host), PhysicalAddress.Parse(_settings.PhysicalAddress), _settings.PreSharedKey);
        }

        public override IEnumerable<MethodHandlerInfo> GetMethodHandlerInfos(DeviceClient deviceClient)
        {
            yield return new MethodHandlerInfo("TVTurnOn", TurnOn);
            yield return new MethodHandlerInfo("TVTurnOff", TurnOff);
            yield return new MethodHandlerInfo("TVSetInputPort", SetInputPort);
            yield return new MethodHandlerInfo("TVGetInputPort", GetInputPort);
            yield return new MethodHandlerInfo("TVGetPowerStatus", GetPowerStatus);
        }

        private Task<MethodResponse> TurnOn(MethodRequest methodRequest, object userContext)
        {
            bool success = _device.TurnOn();
            return methodRequest.GetMethodResponse(success);
        }

        private Task<MethodResponse> TurnOff(MethodRequest methodRequest, object userContext)
        {
            bool success = _device.TurnOff();
            return methodRequest.GetMethodResponse(success);
        }

        private Task<MethodResponse> SetInputPort(MethodRequest methodRequest, object userContext)
        {
            var payloadDefintion = new
            {
                inputPort = (InputPort)(-1)
            };

            var payload = JsonConvert.DeserializeAnonymousType(methodRequest.DataAsJson, payloadDefintion);
            if (payload.inputPort.Valid())
            {
                bool success = _device.SetInputPort(payload.inputPort);
                return methodRequest.GetMethodResponse(success);
            }

            return methodRequest.GetMethodResponse(false);
        }

        private Task<MethodResponse> GetInputPort(MethodRequest methodRequest, object userContext)
        {
            return methodRequest.Get(() => _device.GetInputPort());
        }

        private Task<MethodResponse> GetPowerStatus(MethodRequest methodRequest, object userContext)
        {
            return methodRequest.Get(() => _device.GetPowerStatus());
        }

    }
}