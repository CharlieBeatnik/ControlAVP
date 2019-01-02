using ControllableDevice;
using ControllableDeviceTypes.AtenVS0801HTypes;
using Microsoft.Azure.Devices.Client;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.IO;
using System.Net;
using NLog;
using Newtonsoft.Json.Linq;

namespace ControlRelay
{
    class CloudInterface
    {
        private DeviceClient _deviceClient;
        private List<AtenVS0801H> _hdmiSwitches = new List<AtenVS0801H>();
        private ApcAP8959EU3 _pdu;
        private ExtronDSC301HD _scaler;

        private dynamic _settings;
        private readonly string _settingsFile = "settings.json";

        private static Logger _logger = LogManager.GetCurrentClassLogger();


        public CloudInterface()
        {
            _logger.Debug("");

            using (StreamReader r = new StreamReader(_settingsFile))
            {
                string json = r.ReadToEnd();

                _settings = JObject.Parse(json);
            }

            _hdmiSwitches.Add(new AtenVS0801H((string)_settings.SelectToken("AtenVS0801H[0].SerialID")));

            _pdu = new ApcAP8959EU3(
                (string)_settings.SelectToken("ApcAP8959EU3.Host"),
                int.Parse((string)_settings.SelectToken("ApcAP8959EU3.Port")),
                (string)_settings.SelectToken("ApcAP8959EU3.Username"),
                (string)_settings.SelectToken("ApcAP8959EU3.Password"));

            _scaler = new ExtronDSC301HD((string)_settings.SelectToken("ExtronDSC301HD.SerialID"));

            CreateDeviceClient();
        }

        ~CloudInterface()
        {
            _logger.Debug("");
        }

        private void CreateDeviceClient()
        {
            _logger.Debug("");

            // Connect to the IoT hub using the MQTT protocol
            _deviceClient = DeviceClient.CreateFromConnectionString((string)_settings.SelectToken("Azure.IoTHub.ConnectionString"), TransportType.Mqtt);

            _deviceClient.SetConnectionStatusChangesHandler(DeviceClientConnectionStatusChanged);
            _deviceClient.SetMethodHandlerAsync("Close", DeviceClientClose, null).Wait();

            // Create handlers for the direct method calls
            _deviceClient.SetMethodHandlerAsync("HDMISwitchGoToNextInput", HDMISwitchGoToNextInput, null).Wait();
            _deviceClient.SetMethodHandlerAsync("HDMISwitchGoToPreviousInput", HDMISwitchGoToPreviousInput, null).Wait();
            _deviceClient.SetMethodHandlerAsync("HDMISwitchGetState", HDMISwitchGetState, null).Wait();
            _deviceClient.SetMethodHandlerAsync("HDMISwitchSetInput", HDMISwitchSetInput, null).Wait();

            _deviceClient.SetMethodHandlerAsync("PDUGetOutlets", PDUGetOutlets, null).Wait();
            _deviceClient.SetMethodHandlerAsync("PDUGetOutletsWaitForPending", PDUGetOutletsWaitForPending, null).Wait();
            _deviceClient.SetMethodHandlerAsync("PDUTurnOutletOn", PDUTurnOutletOn, null).Wait();
            _deviceClient.SetMethodHandlerAsync("PDUTurnOutletOff", PDUTurnOutletOff, null).Wait();
        }

        private void DeviceClientConnectionStatusChanged(ConnectionStatus status, ConnectionStatusChangeReason reason)
        {
            _logger.Debug($"Status: {status.ToString()}, Reason: {reason.ToString()}");

            if(status == ConnectionStatus.Disconnected)
            {
                ResetConnection();
            }
        }

        private Task<MethodResponse> DeviceClientClose(MethodRequest methodRequest, object userContext)
        {
            _logger.Debug("");
            //_deviceClient.CloseAsync();

            ResetConnection();
            return Task.FromResult(GetMethodResponse(methodRequest, true));
        }

        private Task<MethodResponse> HDMISwitchGoToNextInput(MethodRequest methodRequest, object userContext)
        {
            _logger.Debug("");

            var payloadDefintion = new { _hdmiSwitchId = -1 };

            var payload = JsonConvert.DeserializeAnonymousType(methodRequest.DataAsJson, payloadDefintion);
            bool success = _hdmiSwitches[payload._hdmiSwitchId].GoToNextInput();
            return Task.FromResult(GetMethodResponse(methodRequest, success));
        }

        private Task<MethodResponse> HDMISwitchGoToPreviousInput(MethodRequest methodRequest, object userContext)
        {
            _logger.Debug("");

            var payloadDefintion = new { _hdmiSwitchId = -1 };

            var payload = JsonConvert.DeserializeAnonymousType(methodRequest.DataAsJson, payloadDefintion);
            bool success = _hdmiSwitches[payload._hdmiSwitchId].GoToPreviousInput();
            return Task.FromResult(GetMethodResponse(methodRequest, success));
        }

        private Task<MethodResponse> HDMISwitchGetState(MethodRequest methodRequest, object userContext)
        {
            _logger.Debug("");

            var payloadDefintion = new { _hdmiSwitchId = -1 };

            var payload = JsonConvert.DeserializeAnonymousType(methodRequest.DataAsJson, payloadDefintion);

            var result = _hdmiSwitches[payload._hdmiSwitchId].GetState();
            if (result != null)
            {
                string json = JsonConvert.SerializeObject(result);
                var response = new MethodResponse(Encoding.UTF8.GetBytes(json), (int)HttpStatusCode.OK);
                return Task.FromResult(response);
            }
            else
            {
                return Task.FromResult(GetMethodResponse(methodRequest, false));
            }
        }

        private Task<MethodResponse> HDMISwitchSetInput(MethodRequest methodRequest, object userContext)
        {
            _logger.Debug("");

            var payloadDefintion = new
            {
                _hdmiSwitchId = -1,
                inputPort = InputPort.Port1
            };

            var payload = JsonConvert.DeserializeAnonymousType(methodRequest.DataAsJson, payloadDefintion);
            bool success = _hdmiSwitches[payload._hdmiSwitchId].SetInput(payload.inputPort);
            return Task.FromResult(GetMethodResponse(methodRequest, success));
        }

        private Task<MethodResponse> PDUGetOutlets(MethodRequest methodRequest, object userContext)
        {
            _logger.Debug("");

            var result = _pdu.GetOutlets();

            if (result != null)
            {
                string json = JsonConvert.SerializeObject(result);
                var response = new MethodResponse(Encoding.UTF8.GetBytes(json), (int)HttpStatusCode.OK);
                return Task.FromResult(response);
            }
            else
            {
                return Task.FromResult(GetMethodResponse(methodRequest, false));
            }
        }

        private Task<MethodResponse> PDUGetOutletsWaitForPending(MethodRequest methodRequest, object userContext)
        {
            _logger.Debug("");

            var result = _pdu.GetOutletsWaitForPending();

            if (result != null)
            {
                string json = JsonConvert.SerializeObject(result);
                var response = new MethodResponse(Encoding.UTF8.GetBytes(json), (int)HttpStatusCode.OK);
                return Task.FromResult(response);
            }
            else
            {
                return Task.FromResult(GetMethodResponse(methodRequest, false));
            }
        }

        private Task<MethodResponse> PDUTurnOutletOn(MethodRequest methodRequest, object userContext)
        {
            _logger.Debug("");

            var payloadDefintion = new
            {
                outletId = -1
            };

            var payload = JsonConvert.DeserializeAnonymousType(methodRequest.DataAsJson, payloadDefintion);
            _pdu.TurnOutletOn(payload.outletId);
            //ANDREWDENN_TODO: No way of determining outlet change succeded or failed
            bool success = true;
            return Task.FromResult(GetMethodResponse(methodRequest, success));
        }

        private Task<MethodResponse> PDUTurnOutletOff(MethodRequest methodRequest, object userContext)
        {
            _logger.Debug("");

            var payloadDefintion = new
            {
                outletId = -1
            };

            var payload = JsonConvert.DeserializeAnonymousType(methodRequest.DataAsJson, payloadDefintion);
            _pdu.TurnOutletOff(payload.outletId);
            //ANDREWDENN_TODO: No way of determining outlet change succeded or failed
            bool success = true;
            return Task.FromResult(GetMethodResponse(methodRequest, success));
        }

        private MethodResponse GetMethodResponse(MethodRequest methodRequest, bool success)
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

        private void ResetConnection()
        {
            _logger.Debug("");

            // Attempt to close any existing connections before creating a new one
            if (_deviceClient != null)
            {
                _deviceClient.CloseAsync().Wait();
            }

            // Create new connection
            CreateDeviceClient();
        }
    }
}
