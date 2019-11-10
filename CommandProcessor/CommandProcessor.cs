using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace CommandProcessor
{
    public static class CommandProcessorUtils
    {
        static public void ProcessBatch(IEnumerable<object> devices, string json)
        {
            if (devices == null)
            {
                throw new ArgumentNullException(nameof(devices));
            }

            dynamic commandBatch = JObject.Parse(json);

            foreach (dynamic command in commandBatch.Commands)
            {
                Type deviceType = Type.GetType($"ControllableDevice.{command.DeviceType}, ControllableDevice");
                var filteredDevices = devices.Where(i => i.GetType() == deviceType);
            }

        }
    }
}
