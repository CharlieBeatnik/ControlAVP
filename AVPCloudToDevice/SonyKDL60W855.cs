using Microsoft.Azure.Devices;
using System;
using System.Collections.Generic;
using System.Text;

namespace AVPCloudToDevice
{
    public class SonyKDL60W855
    {
        private ServiceClient _serviceClient;
        private string _deviceId;

        public SonyKDL60W855(ServiceClient serviceClient, string deviceId)
        {
            _serviceClient = serviceClient;
            _deviceId = deviceId;
        }

        public bool TurnOn()
        {
            try
            {
                var response = Utilities.InvokeMethod(_serviceClient, _deviceId, "TVTurnOn", null);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
