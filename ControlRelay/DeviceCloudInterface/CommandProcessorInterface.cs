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
            bool jsonValid = CommandProcessorUtils.Valid(methodRequest.DataAsJson);

            Task.Run(() => Execute(_devices, methodRequest.DataAsJson));

            return methodRequest.GetMethodResponse(jsonValid); 
        }

        private void Execute(IEnumerable<object> devices, string jsonCommands)
        {
            foreach (var commandResult in CommandProcessorUtils.Execute(devices, jsonCommands))
            {
                //ANDREWDE_TODO: Would like to send a partial method reponse to update on progress
            }
        }
    }
}
