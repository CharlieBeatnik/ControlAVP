using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommandProcessor;
using ControllableDevice;
using ControllableDeviceTypes.OSSCTypes;
using Microsoft.Azure.Devices.Client;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace ControlRelay
{
    class CommandProcessorInterface : DeviceCloudInterface
    {
        List<object> _devices;

        public CommandProcessorInterface(List<object> devices)
        {
            _devices = devices;
        }

        public override IEnumerable<MethodHandlerInfo> GetMethodHandlerInfos(DeviceClient deviceClient)
        {
            yield return new MethodHandlerInfo("CommandProcessorExecute", CommandProcessorExecute);
        }

        private Task<MethodResponse> CommandProcessorExecute(MethodRequest methodRequest, object userContext)
        {
            bool success = false;

            foreach (var commandResult in CommandProcessorUtils.Execute(_devices, methodRequest.DataAsJson))
            {
                //ANDREWDE_TODO: Would like to send a partial method reponse to update on progress
            }

            success = true;
            return methodRequest.GetMethodResponse(success);
        }
    }
}
