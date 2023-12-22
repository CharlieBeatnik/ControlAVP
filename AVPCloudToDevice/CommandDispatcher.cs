using Microsoft.Azure.Devices;
using Newtonsoft.Json;
using ControllableDeviceTypes.OSSCTypes;
using System;
using Newtonsoft.Json.Linq;

namespace AVPCloudToDevice
{
    public class CommandDispatcher(ServiceClient serviceClient, string deviceId)
    {
        private readonly ServiceClient _serviceClient = serviceClient;
        private readonly string _deviceId = deviceId;

        public bool Dispatch(string json)
        {
            return Dispatch(json, Guid.NewGuid());
        }

        public bool Dispatch(string json, Guid id)
        {
            var payload = new
            {
                Id = id,
                Commands = json
            };

            try
            {
                var response = Utilities.InvokeMethodWithJsonPayload(_serviceClient, _deviceId, "CommandProcessorExecute", JsonConvert.SerializeObject(payload));
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
