﻿using Microsoft.Azure.Devices;
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

        public IEnumerable<Outlet> GetOutletsWaitForPending()
        {
            try
            {
                var payload = new
                {
                };

                var response = Utilities.InvokeMethod(_serviceClient, _deviceId, "PDUGetOutletsWaitForPending", payload);
                string json = response.GetPayloadAsJson();
                return JsonConvert.DeserializeObject<List<Outlet>>(json);
            }
            catch
            {
                return null;
            }
        }

        public bool TurnOutletOn(int outletId)
        {
            try
            {
                var payload = new
                {
                    outletId
                };

                var response = Utilities.InvokeMethod(_serviceClient, _deviceId, "PDUTurnOutletOn", payload);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool TurnOutletOff(int outletId)
        {
            try
            {
                var payload = new
                {
                    outletId
                };

                var response = Utilities.InvokeMethod(_serviceClient, _deviceId, "PDUTurnOutletOff", payload);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}