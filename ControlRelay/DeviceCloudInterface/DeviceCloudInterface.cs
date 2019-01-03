using Microsoft.Azure.Devices.Client;
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

        protected MethodResponse GetMethodResponse(MethodRequest methodRequest, bool success)
        {
            if (success)
            {
                // Acknowlege the direct method call with a 200 success message
                string result = "{\"result\":\"Executed direct method: " + methodRequest.Name + "\"}";
                return new MethodResponse(Encoding.UTF8.GetBytes(result), (int)HttpStatusCode.OK);
            }
            else
            {
                // Acknowlege the direct method call with a 400 error message
                string result = "{\"result\":\"Invalid parameter\"}";
                return new MethodResponse(Encoding.UTF8.GetBytes(result), (int)HttpStatusCode.BadRequest);
            }
        }
    }
}
