using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ControlRelay
{
    public static class CommandBatchProcessor
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
