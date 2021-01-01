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
using System.Threading;

namespace ControlRelay
{
    class CommandProcessorInterface : DeviceCloudInterface
    {
        private List<object> _devices;

        public CommandProcessorInterface(List<object> devices)
        {
            _devices = devices;
        }

        public TimeSpan CancelExecuteAfter { get; set; } = TimeSpan.FromMinutes(2);

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
            if (jsonValid)
            {
                //Setup a timed cancellation token to ensure that the execute task always terminates
                CancellationTokenSource source = new CancellationTokenSource();
                source.CancelAfter((int)CancelExecuteAfter.TotalMilliseconds);
                CancellationToken token = source.Token;
                token.Register(() => OnExecuteCancellation(id));

                //The potentially long running Execute function is run on its own thread 
                Task.Run(() => Execute(_devices, jsonCommands, id), token);
            }

            //Retrun success to indicate the json was valid, but not that the commands have been processed
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
                    commandMessage.Properties.Add("user-id", id.ToString());
                    commandMessage.Properties.Add("user-success", commandResult.Success.ToString());
                    commandMessage.Properties.Add("user-command-count", commandResult.Count.ToString());
                    commandMessage.Properties.Add("user-command-index", commandResult.Index.ToString());
                    commandMessage.Properties.Add("user-error-message", commandResult.ErrorMessage);

                    _deviceClient.SendEventAsync(commandMessage).Wait();
                }
            }
        }

        private void OnExecuteCancellation(Guid id)
        {
            var commandMessage = new Message();
            commandMessage.Properties.Add("user-id", id.ToString());
            commandMessage.Properties.Add("user-success", false.ToString());
            commandMessage.Properties.Add("user-error-message", "Execute task timed out.");

            //Send error message to indicate Execute has timed out
            _deviceClient.SendEventAsync(commandMessage).Wait();
        }
    }
}
