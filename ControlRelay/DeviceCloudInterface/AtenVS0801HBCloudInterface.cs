﻿using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using ControllableDevice;
using ControllableDeviceTypes.AtenVS0801HBTypes;
using Microsoft.Azure.Devices.Client;
using Newtonsoft.Json;

namespace ControlRelay
{
    class AtenVS0801HBCloudInterface : DeviceCloudInterface
    {
        private List<AtenVS0801HB> _devices = new List<AtenVS0801HB>();
        private static string _deviceApiPrefix = "AtenVS0801HB";

        public AtenVS0801HBCloudInterface(List<AtenVS0801HB> devices)
        {
            _devices = devices;
        }

        public override IEnumerable<MethodHandlerInfo> GetMethodHandlerInfos(DeviceClient deviceClient)
        {
            yield return new MethodHandlerInfo($"{_deviceApiPrefix}{nameof(GetState)}", GetState);
            yield return new MethodHandlerInfo($"{_deviceApiPrefix}{nameof(SetInputPort)}", SetInputPort);
            yield return new MethodHandlerInfo($"{_deviceApiPrefix}{nameof(GetAvailable)}", GetAvailable);
        }

        private Task<MethodResponse> GetState(MethodRequest methodRequest, object userContext)
        {
            var payloadDefintion = new { _deviceIndex = -1 };

            var payload = JsonConvert.DeserializeAnonymousType(methodRequest.DataAsJson, payloadDefintion);
            if (DeviceIndexValid(payload._deviceIndex))
            {
                var result = _devices[payload._deviceIndex].GetState();
                if (result != null)
                {
                    return methodRequest.GetMethodResponseSerialize(true, result);
                }
            }

            return methodRequest.GetMethodResponse(false);
        }

        private Task<MethodResponse> SetInputPort(MethodRequest methodRequest, object userContext)
        {
            var payloadDefintion = new
            {
                _deviceIndex = -1,
                inputPort = (InputPort)(-1)
            };

            var payload = JsonConvert.DeserializeAnonymousType(methodRequest.DataAsJson, payloadDefintion);
            if (DeviceIndexValid(payload._deviceIndex) && payload.inputPort.Valid())
            {
                bool success = _devices[payload._deviceIndex].SetInputPort(payload.inputPort);
                return methodRequest.GetMethodResponse(success);
            }

            return methodRequest.GetMethodResponse(false);
        }

        private Task<MethodResponse> GetAvailable(MethodRequest methodRequest, object userContext)
        {
            var payloadDefintion = new { _deviceIndex = -1 };

            var payload = JsonConvert.DeserializeAnonymousType(methodRequest.DataAsJson, payloadDefintion);
            if (DeviceIndexValid(payload._deviceIndex))
            {
                var result = _devices[payload._deviceIndex].GetAvailable();
                return methodRequest.GetMethodResponseSerialize(true, result);
            }

            return methodRequest.GetMethodResponse(false);
        }

        private bool DeviceIndexValid(int deviceIndex)
        {
            return (deviceIndex >= 0 && deviceIndex < _devices.Count);
        }
    }
}