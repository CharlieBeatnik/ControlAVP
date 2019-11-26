using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;

namespace CommandProcessor
{
    public sealed class CommandResult
    {
        public string Function { get; set; }
        public bool Success { get; set; }
        public object Result { get; set;  }
        public string Description { get; set; }
    }

    public static class CommandProcessorUtils
    {
        public static IEnumerable<CommandResult> Execute(IEnumerable<object> devices, string jsonCommands)
        {
            if (devices == null)
            {
                throw new ArgumentNullException(nameof(devices));
            }

            //Pull the json schema out of the assembly resources and created it
            var jsonSchemaBytes = Properties.Resources.command_processor_schema;
            string jsonSchemaString = Encoding.UTF8.GetString(jsonSchemaBytes, 0, jsonSchemaBytes.Length);
            JSchema schema = JSchema.Parse(jsonSchemaString);

            //Parse the json passed in and validate it, throw exception if it fails
            IList<string> errorMessages;
            JObject commandBatch = JObject.Parse(jsonCommands);
            bool valid = commandBatch.IsValid(schema, out errorMessages);

            if(!valid)
            {
                throw new ArgumentException("JSON commands failed schema validation.");
            }

            int? defaultDeviceIndex = (int?)commandBatch["DefaultDeviceIndex"];
            string defaultAssembly = (string)commandBatch["DefaultAssembly"];

            foreach (JObject command in commandBatch["Commands"])
            {
                string assembly = (command["Assembly"] == null) ? defaultAssembly : (string)command["Assembly"];
                int? deviceIndex = (command["DeviceIndex"] == null) ? defaultDeviceIndex : (int?)command["DeviceIndex"];

                //If assembly/defaultAssembly or deviceIndex/defaultDeviceIndex are not provided
                //then return a failure for this command
                if(assembly == null || deviceIndex == null)
                {
                    yield return new CommandResult()
                    {
                        Function = (string)command["Function"],
                        Success = false,
                        Result = null,
                        Description = (string)command["Description"]
                    };
                    break;
                }

                Type deviceType = Type.GetType($"{assembly}.{(string)command["DeviceType"]}, {assembly}");
                List<object> filteredDevices = devices.Where(i => i.GetType() == deviceType).ToList();

                var device = filteredDevices[(int)deviceIndex];
                MethodInfo methodInfo = deviceType.GetMethod((string)command["Function"]);

                ParameterInfo[] parameterInfos = methodInfo.GetParameters();

                //If no parameters are provided, but parameters are required
                //Or, if parameter count does not match required parameter count
                if((command["Parameters"] == null && parameterInfos.Length != 0) ||
                   (command["Parameters"] != null && (command["Parameters"].Count() != parameterInfos.Length)))
                {
                    yield return new CommandResult()
                    {
                        Function = (string)command["Function"],
                        Success = false,
                        Result = null,
                        Description = (string)command["Description"]
                    };
                    break;
                }

                //Build parameter array if necessary
                object[] parameters = null;
                if (parameterInfos.Length > 0)
                {
                    parameters = parameterInfos.Select(p =>
                    {
                        string paramValue = (string)command["Parameters"][p.Name];
                        Type paramType = p.ParameterType;
                        return paramType.IsEnum ? Enum.Parse(paramType, paramValue) : Convert.ChangeType(paramValue, paramType);
                    })
                    .ToArray();
                }

                Type returnType = methodInfo.ReturnType;
                bool success;
                object result;

                if(returnType == typeof(bool))
                {
                    bool resultBool = (bool)methodInfo.Invoke(device, parameters);
                    result = resultBool;
                    success = resultBool;
                }
                else if (Nullable.GetUnderlyingType(returnType) != null) //Type is nullable
                {
                    object nullableResult = methodInfo.Invoke(device, parameters);
                    result = nullableResult;
                    success = nullableResult != null;
                }
                else
                {
                    Debug.Fail("Return type must be bool or nullable");
                    result = null;
                    success = false;
                }

                if(command["PostWait"] != null)
                {
                    Thread.Sleep(TimeSpan.FromSeconds((double)command["PostWait"]));
                }

                yield return new CommandResult()
                {
                    Function = (string)command["Function"],
                    Success = success,
                    Result = result,
                    Description = (string)command["Description"]
                };
            }

        }
    }
}
