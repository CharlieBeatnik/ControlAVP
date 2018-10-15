using AudioVideoDevice;
using PduDevice;
using Microsoft.Azure.Devices.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.IO;
using Newtonsoft.Json.Linq;
using System.Dynamic;
using Newtonsoft.Json.Converters;

namespace ControlRelay
{
    class CloudInterface
    {
        private DeviceClient _deviceClient;
        private List<AtenVS0801H> _hdmiSwitches = new List<AtenVS0801H>();
        private ApcAP8959EU3 _pdu;

        private dynamic _settings;
        private readonly string _settingsFile = "settings.json";

        public CloudInterface()
        {
            using (StreamReader r = new StreamReader(_settingsFile))
            {
                string json = r.ReadToEnd();
                _settings = JsonConvert.DeserializeObject<ExpandoObject>(json, new ExpandoObjectConverter());
            }

            _hdmiSwitches.Add(new AtenVS0801H(_settings.AtenVS0801H[0].SerialID));

            _pdu = new ApcAP8959EU3(_settings.ApcAP8959EU3.Host, int.Parse(_settings.ApcAP8959EU3.Port), _settings.ApcAP8959EU3.Username, _settings.ApcAP8959EU3.Password);

            // Connect to the IoT hub using the MQTT protocol
            _deviceClient = DeviceClient.CreateFromConnectionString(_settings.Azure.IoTHub.ConnectionString, TransportType.Mqtt);

            // Create handlers for the direct method calls
            _deviceClient.SetMethodHandlerAsync("HDMISwitchGoToNextInput", HDMISwitchGoToNextInput, null).Wait();
            _deviceClient.SetMethodHandlerAsync("HDMISwitchGoToPreviousInput", HDMISwitchGoToPreviousInput, null).Wait();
            _deviceClient.SetMethodHandlerAsync("GetPDUState", GetPDUState, null).Wait();
        }


        private Task<MethodResponse> HDMISwitchGoToNextInput(MethodRequest methodRequest, object userContext)
        {
            var payloadDefintion = new { _hdmiSwitchId = -1 };

            var payload = JsonConvert.DeserializeAnonymousType(methodRequest.DataAsJson, payloadDefintion);
            bool success = _hdmiSwitches[payload._hdmiSwitchId].GoToNextInput();
            return Task.FromResult(GetMethodResponse(methodRequest, success));
        }

        private Task<MethodResponse> HDMISwitchGoToPreviousInput(MethodRequest methodRequest, object userContext)
        {
            var payloadDefintion = new { _hdmiSwitchId = -1 };

            var payload = JsonConvert.DeserializeAnonymousType(methodRequest.DataAsJson, payloadDefintion);
            bool success = _hdmiSwitches[payload._hdmiSwitchId].GoToPreviousInput();
            return Task.FromResult(GetMethodResponse(methodRequest, success));
        }

        private Task<MethodResponse> GetPDUState(MethodRequest methodRequest, object userContext)
        {
            var payloadDefintion = new { };

            string json = JsonConvert.SerializeObject(_pdu.GetOutlets());

            var response = new MethodResponse(Encoding.UTF8.GetBytes(json), 200);
            return Task.FromResult(response);
        }

        private MethodResponse GetMethodResponse(MethodRequest methodRequest, bool success)
        {
            if (success)
            {
                // Acknowlege the direct method call with a 200 success message
                string result = "{\"result\":\"Executed direct method: " + methodRequest.Name + "\"}";
                return new MethodResponse(Encoding.UTF8.GetBytes(result), 200);
            }
            else
            {
                // Acknowlege the direct method call with a 400 error message
                string result = "{\"result\":\"Invalid parameter\"}";
                return new MethodResponse(Encoding.UTF8.GetBytes(result), 400);
            }
        }
    }
}
