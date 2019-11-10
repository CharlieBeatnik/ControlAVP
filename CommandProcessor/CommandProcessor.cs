using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Newtonsoft.Json.Linq;

namespace CommandProcessor
{
    public sealed class CommandResult
    {
        public string FunctionName { get; set; }
        public object Result { get; set; }
    }

    public static class CommandProcessorUtils
    {
        public static IEnumerable<CommandResult> ProcessBatch(IEnumerable<object> devices, string jsonCommands)
        {
            if (devices == null)
            {
                throw new ArgumentNullException(nameof(devices));
            }

            //Pull the json schema out of the assembly resources
            var bytes = Properties.Resources.command_processor_schema;
            string jsonSchema = Encoding.UTF8.GetString(bytes, 0, bytes.Length);

            dynamic commandBatch = JObject.Parse(jsonCommands);

            foreach (dynamic command in commandBatch.Commands)
            {
                string assembly = (string)command.Assembly;
                Type deviceType = Type.GetType($"{assembly}.{(string)command.DeviceType}, {assembly}");
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
                yield return new CommandResult() { FunctionName = command.Function, Result = result };
            }

        }
    }
}
