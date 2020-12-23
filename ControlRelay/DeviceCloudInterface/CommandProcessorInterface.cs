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
using Newtonsoft.Json.Linq;

namespace ControlRelay
{
    class CommandProcessorInterface : DeviceCloudInterface
    {
        private List<object> _devices;

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
            var jsonParsed = JObject.Parse(methodRequest.DataAsJson);
            Guid id = (Guid)jsonParsed["Id"];
            string jsonCommands = (string)jsonParsed["Commands"];

            bool jsonValid = CommandProcessorUtils.Valid(jsonCommands);

            Task.Run(() => Execute(_devices, jsonCommands, id));

            return methodRequest.GetMethodResponse(jsonValid); 
        }

        private void Execute(IEnumerable<object> devices, string json, Guid id)
        {
            foreach (var commandResult in CommandProcessorUtils.Execute(devices, json, id))
            {
                if(_deviceClient != null)
                {
                    var serializeData = JsonConvert.SerializeObject(commandResult);
                    var commandMessage = new Message(Encoding.ASCII.GetBytes(serializeData));

                    _deviceClient.SendEventAsync(commandMessage).Wait();
                }
            }
        }
    }
}
