using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ControllableDevice;
using ControllableDeviceTypes.OSSCTypes;
using Microsoft.Azure.Devices.Client;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace ControlRelay
{
    class CommandBatchInterface : DeviceCloudInterface
    {
        public struct Settings
        {
        }

        private Settings _settings;

        public CommandBatchInterface(Settings settings)
        {
            _settings = settings;
        }

        public override IEnumerable<MethodHandlerInfo> GetMethodHandlerInfos(DeviceClient deviceClient)
        {
            yield return new MethodHandlerInfo("ExecuteCommandBatch", ExecuteCommandBatch);
        }

        private Task<MethodResponse> ExecuteCommandBatch(MethodRequest methodRequest, object userContext)
        {
            bool success = false;
            //var payloadDefintion = new
            //{
            //    commandName = (CommandName)(-1),
            //};

            //var payload = JsonConvert.DeserializeAnonymousType(methodRequest.DataAsJson, payloadDefintion);

            success = true;
            return methodRequest.GetMethodResponse(success);
        }
    }
}
