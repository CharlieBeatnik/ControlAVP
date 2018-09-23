using AudioVideoDevice;
using Microsoft.Azure.Devices.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace ComControl
{
    class CloudInterface
    {
        private DeviceClient _deviceClient;
        private List<AtenVS0801H> _hdmiSwitches = new List<AtenVS0801H>();
        private readonly string _connectionString = "HostName=andydennocoIoTHub.azure-devices.net;DeviceId=gamesroom;SharedAccessKey=k29/EhJNaqnNTmWWk3p3jja30h5gMRSGRaHeSn6e2zo=";

        public CloudInterface()
        {
            _hdmiSwitches.Add(new AtenVS0801H("AK05UVF8A"));

            // Connect to the IoT hub using the MQTT protocol
            _deviceClient = DeviceClient.CreateFromConnectionString(_connectionString, TransportType.Mqtt);

            // Create a handlers for the direct method calls
            _deviceClient.SetMethodHandlerAsync("HDMISwitchGoToNextInput", HDMISwitchGoToNextInput, null).Wait();
            _deviceClient.SetMethodHandlerAsync("HDMISwitchGoToPreviousInput", HDMISwitchGoToPreviousInput, null).Wait();
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
