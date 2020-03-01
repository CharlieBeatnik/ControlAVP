using Microsoft.Azure.Devices;
using Newtonsoft.Json;
using System;
using System.Net;
using System.Threading.Tasks;

namespace AVPCloudToDevice
{
    class Utilities
    {
        public static async Task<CloudToDeviceMethodResult> InvokeDeviceMethodAsync(ServiceClient serviceClient, string deviceId, CloudToDeviceMethod cloudToDeviceMethod)
        {
            try
            {
                var result = await serviceClient.InvokeDeviceMethodAsync(deviceId, cloudToDeviceMethod).ConfigureAwait(false);
                return result;
            }

            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            return null;
        }

        public static CloudToDeviceMethodResult InvokeMethodWithObjectPayload(ServiceClient serviceClient, string deviceId, string methodName, object payload)
        {
            return InvokeMethodWithJsonPayload(serviceClient, deviceId, methodName, JsonConvert.SerializeObject(payload));
        }

        public static CloudToDeviceMethodResult InvokeMethodWithObjectPayload(ServiceClient serviceClient, string deviceId, string methodName, object payload, TimeSpan responseTimeout)
        {
            return InvokeMethodWithJsonPayload(serviceClient, deviceId, methodName, JsonConvert.SerializeObject(payload), responseTimeout);
        }

        public static CloudToDeviceMethodResult InvokeMethodWithJsonPayload(ServiceClient serviceClient, string deviceId, string methodName, string json)
        {
            return InvokeMethodWithJsonPayload(serviceClient, deviceId, methodName, json, TimeSpan.FromSeconds(10));
        }

        public static CloudToDeviceMethodResult InvokeMethodWithJsonPayload(ServiceClient serviceClient, string deviceId, string methodName, string json, TimeSpan responseTimeout)
        {
            if(responseTimeout == null)
            {
                responseTimeout = TimeSpan.FromSeconds(10);
            }

            var methodInvocation = new CloudToDeviceMethod(methodName) { ResponseTimeout = (TimeSpan)responseTimeout };
            methodInvocation.SetPayloadJson(json);

            // Invoke the direct method asynchronously and get the response from the device.
            var response = Task.Run(async () => await InvokeDeviceMethodAsync(serviceClient, deviceId, methodInvocation).ConfigureAwait(false)).Result;

            if (response == null)
            {
                throw new Exception($"Invoking method '{methodName}' resulted in a null response.");
            }
            else
            {
                if (response.Status != (int)HttpStatusCode.OK)
                {
                    throw new Exception($"Invoking method '{methodName}' resulted in a HTPP status code '{response.Status}'.");
                }
            }

            return response;
        }
    }
}
