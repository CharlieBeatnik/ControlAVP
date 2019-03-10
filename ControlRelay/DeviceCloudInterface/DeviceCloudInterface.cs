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
        public abstract void SetMethodHandlers(DeviceClient deviceClient);

        protected Task<MethodResponse> GetMethodResponse(MethodRequest methodRequest, bool success, string payload = null)
        {
            string result;

            if (payload != null)
            {
                result = payload;
            }
            else
            {
                if (success)
                {
                    // Acknowlege the direct method call with a 200 success message
                    result = "{\"result\":\"Executed direct method: " + methodRequest.Name + "\"}";
                }
                else
                {
                    // Acknowlege the direct method call with a 400 error message
                    result = "{\"result\":\"Failure to execute direct method: " + methodRequest.Name + "\"}";
                }
            }

            int status = success ? (int)HttpStatusCode.OK : (int)HttpStatusCode.BadRequest;
            var methodResponse = new MethodResponse(Encoding.UTF8.GetBytes(result), status);
            return Task.FromResult(methodResponse);
        }

        protected Task<MethodResponse> GetMethodResponseSerialize(MethodRequest methodRequest, bool success, object payloadToSerialize)
        {
            string result = JsonConvert.SerializeObject(payloadToSerialize);
            return GetMethodResponse(methodRequest, success, result);
        }

        protected Task<MethodResponse> Get<T>(MethodRequest methodRequest, Func<T> deviceFunction)
        {
            var result = deviceFunction();
            if (result != null)
            {
                return GetMethodResponseSerialize(methodRequest, true, result);
            }
            else
            {
                return GetMethodResponse(methodRequest, false);
            }
        }
    }
}
