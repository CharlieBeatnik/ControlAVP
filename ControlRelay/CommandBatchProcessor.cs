using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ControlRelay
{
    public sealed class CommandBatchResult
    {
        public string FunctionName { get; set; }
        public object Result { get; set; }
    }

    public static class CommandBatchProcessor
    { 
        public static IEnumerable<CommandBatchResult> ProcessBatch(IEnumerable<object> devices, string json)
        {
            if (devices == null)
            {
                throw new ArgumentNullException(nameof(devices));
            }

            dynamic commandBatch = JObject.Parse(json);

            foreach (dynamic command in commandBatch.Commands)
            {
                Type deviceType = Type.GetType($"ControllableDevice.{(string)command.DeviceType}, ControllableDevice");
                List<object> filteredDevices = devices.Where(i => i.GetType() == deviceType).ToList();

                var device = filteredDevices[(int)command.DeviceIndex];
                MethodInfo methodInfo = deviceType.GetMethod((string)command.Function);

                var parameters = methodInfo.GetParameters()
                        .Select(p => {
                            string paramValue = (string)command.Parameters[p.Name];
                            Type paramType = p.ParameterType;
                            return paramType.IsEnum ? Enum.Parse(paramType, paramValue) : Convert.ChangeType(paramValue, paramType);
                        })
                        .ToArray();

                var result = methodInfo.Invoke(device, parameters);
                yield return new CommandBatchResult() { FunctionName = command.Function, Result = result };
            }

        }
    }
}
