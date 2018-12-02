using Microsoft.Azure.Devices;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PduDevice.ApcAP8959EU3Types;

namespace AVPCloudToDevice
{
    public class ApcAP8959EU3
    {
        private ServiceClient _serviceClient;
        private string _deviceId;

        public ApcAP8959EU3(ServiceClient serviceClient, string deviceId)
        {
            _serviceClient = serviceClient;
            _deviceId = deviceId;
        }

        public IEnumerable<Outlet> GetOutlets()
        {
            try
            {
                var payload = new
                {
                };

                var response = Utilities.InvokeMethod(_serviceClient, _deviceId, "PDUGetOutlets", payload);
                string json = response.GetPayloadAsJson();
                return JsonConvert.DeserializeObject<List<Outlet>>(json);
            }
            catch
            {
                return null;
            }
        }
    }
}
