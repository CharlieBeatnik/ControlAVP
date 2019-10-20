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
    internal static class MethodRequestExtensions
    {
        public static Task<MethodResponse> GetMethodResponse(this MethodRequest methodRequest, bool success, string payload = null)
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
                    // Acknowledge the direct method call with a 200 success message
                    result = "{\"result\":\"Executed direct method: " + methodRequest.Name + "\"}";
                }
                else
                {
                    // Acknowledge the direct method call with a 400 error message
                    result = "{\"result\":\"Failure to execute direct method: " + methodRequest.Name + "\"}";
                }
            }

            int status = success ? (int)HttpStatusCode.OK : (int)HttpStatusCode.BadRequest;
            var methodResponse = new MethodResponse(Encoding.UTF8.GetBytes(result), status);
            return Task.FromResult(methodResponse);
        }

        public static Task<MethodResponse> GetMethodResponseSerialize(this MethodRequest methodRequest, bool success, object payloadToSerialize)
        {
            string result = JsonConvert.SerializeObject(payloadToSerialize);
            return methodRequest.GetMethodResponse(success, result);
        }

        public static Task<MethodResponse> Get<T>(this MethodRequest methodRequest, Func<T> deviceFunction)
        {
            var result = deviceFunction();
            if (result != null)
            {
                return methodRequest.GetMethodResponseSerialize(true, result);
            }
            else
            {
                return methodRequest.GetMethodResponse(false);
            }
        }
    }
}
