using Microsoft.Azure.Devices;
using Newtonsoft.Json;
using ControllableDeviceTypes.OSSCTypes;
using System;
using Newtonsoft.Json.Linq;

namespace AVPCloudToDevice
{
    public class CommandProcessor
    {
        private ServiceClient _serviceClient;
        private string _deviceId;

        public CommandProcessor(ServiceClient serviceClient, string deviceId)
        {
            _serviceClient = serviceClient;
            _deviceId = deviceId;
        }

        public bool Execute(string json)
        {
            return Execute(json, Guid.NewGuid());
        }

        public bool Execute(string json, Guid id)
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
