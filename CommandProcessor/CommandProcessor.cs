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
        public string ErrorMessage { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public TimeSpan ExecutionTime => EndTime - StartTime;
        public bool Last { get; set; }
        public Guid Id { get; set; }
    }

    public static class CommandProcessorUtils
    {
        public static bool Valid(string jsonCommands, out JObject parsedJsonCommands)
        {
            //Pull the json schema out of the assembly resources and created it
            var jsonSchemaBytes = Properties.Resources.command_processor_schema;
            string jsonSchemaString = Encoding.UTF8.GetString(jsonSchemaBytes, 0, jsonSchemaBytes.Length);
            JSchema schema = JSchema.Parse(jsonSchemaString);

            //Parse the json passed in and validate it
            IList<string> errorMessages;
            parsedJsonCommands = JObject.Parse(jsonCommands);
            return parsedJsonCommands.IsValid(schema, out errorMessages);
        }

        public static bool Valid(string jsonCommands)
        {
            return Valid(jsonCommands, out _);
        }

        public static IEnumerable<CommandResult> Execute(IEnumerable<object> devices, string json)
        {
            return Execute(devices, json, Guid.NewGuid());
        }

        public static IEnumerable<CommandResult> Execute(IEnumerable<object> devices, string json, Guid id)
        {
            if (devices is null) throw new ArgumentNullException(nameof(devices));
            if (json is null) throw new ArgumentNullException(nameof(json));

            var sw = new Stopwatch();
            sw.Start();

            JObject commandBatch;
            bool jsonValid = Valid(json, out commandBatch);
            if (!jsonValid) throw new ArgumentException("JSON commands failed schema validation.");

            int? defaultDeviceIndex = (int?)commandBatch["DefaultDeviceIndex"];
            string defaultAssembly = (string)commandBatch["DefaultAssembly"];
            string displayName = (string)commandBatch["DisplatName"];

            foreach (JObject command in commandBatch["Commands"])
            {
                TimeSpan startTime = sw.Elapsed;

                double? executeAfterSeconds = (double?)command["ExecuteAfter"];

                //Wait if ExecuteAfter is specified and the time to start execution after hasn't yet been reached
                if((executeAfterSeconds != null) && (executeAfterSeconds > startTime.TotalSeconds))
                {
                    int timeToWaitMilliseconds = (int)((executeAfterSeconds - startTime.TotalSeconds) * 1000);
                    Thread.Sleep(timeToWaitMilliseconds);
                    startTime = sw.Elapsed;
                }

                //Create default CommandResult with common properties
                var commandResult = new CommandResult()
                {
                    Function = (string)command["Function"],
                    Description = (string)command["Description"],
                    StartTime = startTime,
                    Id = id
                };

                string assembly = (command["Assembly"] == null) ? defaultAssembly : (string)command["Assembly"];
                int? deviceIndex = (command["DeviceIndex"] == null) ? defaultDeviceIndex : (int?)command["DeviceIndex"];

                //If assembly/defaultAssembly or deviceIndex/defaultDeviceIndex are not provided
                //then return a failure for this command
                if(assembly == null || deviceIndex == null)
                {
                    commandResult.EndTime = sw.Elapsed;
                    commandResult.ErrorMessage = "Assembly or Device Index is missing.";
                    commandResult.Last = true;
                    yield return commandResult;
                    break;
                }

                Type deviceType = Type.GetType($"{assembly}.{(string)command["DeviceType"]}, {assembly}");
                List<object> filteredDevices = devices.Where(i => i.GetType() == deviceType).ToList();

                var device = filteredDevices[(int)deviceIndex];
                MethodInfo methodInfo = deviceType.GetMethod((string)command["Function"]);

                if (methodInfo == null)
                {
                    commandResult.EndTime = sw.Elapsed;
                    commandResult.ErrorMessage = $"Method {(string)command["Function"]} could not be found.";
                    commandResult.Last = true;
                    yield return commandResult;
                    break;
                }

                ParameterInfo[] parameterInfos = methodInfo.GetParameters();

                //If no parameters are provided, but parameters are required
                //Or, if parameter count does not match required parameter count
                if((command["Parameters"] == null && parameterInfos.Length != 0) ||
                   (command["Parameters"] != null && (command["Parameters"].Count() != parameterInfos.Length)))
                {
                    commandResult.EndTime = sw.Elapsed;
                    commandResult.ErrorMessage = "The wrong number of parameters have been provided.";
                    commandResult.Last = true;
                    yield return commandResult;
                    break;
                }

                //Build parameter array if necessary
                object[] parameters = null;
                bool allParametersProvided = true;
                if (parameterInfos.Length > 0)
                {
                    parameters = parameterInfos.Select(p =>
                    {
                        switch (command["Parameters"][p.Name])
                        {
                            case JValue v:
                                {
                                    string paramValue = (string)command["Parameters"][p.Name];
                                    Type paramType = p.ParameterType;
                                    return paramType.IsEnum ? Enum.Parse(paramType, paramValue) : Convert.ChangeType(paramValue, paramType);
                                }

                            case JObject o:
                                {
                                    Type paramType = p.ParameterType;
                                    object param = Activator.CreateInstance(paramType);

                                    #if DEBUG
                                    var fields = paramType.GetFields();
                                    var properties = paramType.GetProperties();
                                    #endif

                                    foreach (JProperty jProperty in command["Parameters"][p.Name])
                                    {

                                        var field = paramType.GetField(jProperty.Name);
                                        if (field != null)
                                        {
                                            object value = field.FieldType.IsEnum ? Enum.Parse(field.FieldType, (string)jProperty.Value) : Convert.ChangeType(jProperty.Value, field.FieldType);
                                            field.SetValue(param, value);
                                        }
                                        else
                                        {
                                            var property = paramType.GetProperty(jProperty.Name);
                                            if (property != null)
                                            {
                                                object value = property.PropertyType.IsEnum ? Enum.Parse(property.PropertyType, (string)jProperty.Value) : Convert.ChangeType(jProperty.Value, property.PropertyType);
                                                property.SetValue(param, value);
                                            }
                                        }
                                    }

                                    return param;
                                }

                            default:
                            case null:
                                allParametersProvided = false;
                                return null;
                        }
                    })
                    .ToArray();
                }

                if(!allParametersProvided)
                {
                    commandResult.EndTime = sw.Elapsed;
                    commandResult.ErrorMessage = "The correct number of parameters were provides but there was a problem with at least 1, is the name correct?";
                    commandResult.Last = true;
                    yield return commandResult;
                    break;
                }

                Type returnType = methodInfo.ReturnType;
                if(returnType == typeof(bool))
                {
                    bool resultBool = (bool)methodInfo.Invoke(device, parameters);
                    commandResult.Result = resultBool;
                    commandResult.Success = resultBool;
                }
                else if (Nullable.GetUnderlyingType(returnType) != null) //Type is nullable
                {
                    object nullableResult = methodInfo.Invoke(device, parameters);
                    commandResult.Result = nullableResult;
                    commandResult.Success = nullableResult != null;
                }
                else
                {
                    commandResult.ErrorMessage = $"Return type of function {(string)command["Function"]} must be bool or nullable";
                }

                if(command["PostWait"] != null)
                {
                    Thread.Sleep(TimeSpan.FromSeconds((double)command["PostWait"]));
                }

                commandResult.EndTime = sw.Elapsed;
                commandResult.Last = (command == commandBatch["Commands"].Last());
                yield return commandResult;
            }

        }
    }
}
