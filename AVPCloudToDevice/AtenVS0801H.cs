using Microsoft.Azure.Devices;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net;
using AudioVideoDevice.AtenVS0801HTypes;


namespace AVPCloudToDevice
{
    public class AtenVS0801H
    {
        private ServiceClient _serviceClient;
        private string _deviceId;
        private int _hdmiSwitchId;

        public AtenVS0801H(ServiceClient serviceClient, string deviceId, int hdmiSwitchId)
        {
            _serviceClient = serviceClient;
            _deviceId = deviceId;
            _hdmiSwitchId = hdmiSwitchId;
        }

        public bool GoToNextInput()
        {
            try
            {
                var payload = new { _hdmiSwitchId };

                var response = Utilities.InvokeMethod(_serviceClient, _deviceId, "HDMISwitchGoToNextInput", payload);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool GoToPreviousInput()
        {
            try
            {
                var payload = new { _hdmiSwitchId };

                var response = Utilities.InvokeMethod(_serviceClient, _deviceId, "HDMISwitchGoToPreviousInput", payload);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public State GetState()
        {
            try
            {
                var payload = new { _hdmiSwitchId };

                var response = Utilities.InvokeMethod(_serviceClient, _deviceId, "HDMISwitchGetState", payload);
                string json = response.GetPayloadAsJson();
                return JsonConvert.DeserializeObject<State>(json);
            }
            catch
            {
                return null;
            }
        }

        public bool SetInput(InputPort inputPort)
        {
            try
            {
                var payload = new
                {
                    _hdmiSwitchId,
                    inputPort
                };

                var response = Utilities.InvokeMethod(_serviceClient, _deviceId, "HDMISwitchSetInput", payload);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
