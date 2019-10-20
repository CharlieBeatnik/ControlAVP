using Microsoft.Azure.Devices.Client;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ControlRelay
{
    abstract class DeviceCloudInterface
    {
        public class MethodHandlerInfo
        {
            public MethodHandlerInfo(string name, MethodCallback handler)
            {
                Name = name;
                Handler = handler;
            }

            public string Name { get; }
            public MethodCallback Handler { get; }
        }

        public abstract IEnumerable<MethodHandlerInfo> GetMethodHandlerInfos(DeviceClient deviceClient);
    }
}
