using Microsoft.Azure.Devices.Client;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ControlRelay
{
    class DeviceCloudInterfaceManager
    {
        private List<DeviceCloudInterface> _deviceCloudInterfaces = new List<DeviceCloudInterface>();
        private DeviceClient _deviceClient;

        private string _connectionString;
        private List<AtenVS0801HCloudInterface.Settings> _settingsAtenVS0801H;
        private ApcAP8959EU3CloudInterface.Settings _settingsApcAP8959EU3;
        private ExtronDSC301HDCloudInterface.Settings _settingsExtronDSC301HD;
        private SonyKDL60W855CloudInterface.Settings _settingsSonyKDL60W855;

        private static Logger _logger = LogManager.GetCurrentClassLogger();

        public DeviceCloudInterfaceManager(string settingsFile)
        {
            JObject jsonParsed;
            using (StreamReader r = new StreamReader(settingsFile))
            {
                string json = r.ReadToEnd();
                jsonParsed = JObject.Parse(json);
            }

            //Azure Settings
            _connectionString = jsonParsed["Azure"]["IoTHub"]["ConnectionString"].ToString();

            //AtenVS0801H Settings 
            _settingsAtenVS0801H = JsonConvert.DeserializeObject<List<AtenVS0801HCloudInterface.Settings>>(jsonParsed["AtenVS0801H"].ToString());
            _deviceCloudInterfaces.Add(new AtenVS0801HCloudInterface(_settingsAtenVS0801H));

            //AtenVS0801H Settings 
            _settingsApcAP8959EU3 = JsonConvert.DeserializeObject<ApcAP8959EU3CloudInterface.Settings>(jsonParsed["ApcAP8959EU3"].ToString());
            _deviceCloudInterfaces.Add(new ApcAP8959EU3CloudInterface(_settingsApcAP8959EU3));

            //ExtronDSC301HD Settings 
            _settingsExtronDSC301HD = JsonConvert.DeserializeObject<ExtronDSC301HDCloudInterface.Settings>(jsonParsed["ExtronDSC301HD"].ToString());
            _deviceCloudInterfaces.Add(new ExtronDSC301HDCloudInterface(_settingsExtronDSC301HD));

            //SonyKDL60W855 Settings 
            _settingsSonyKDL60W855 = JsonConvert.DeserializeObject<SonyKDL60W855CloudInterface.Settings>(jsonParsed["SonyKDL60W855"].ToString());
            _deviceCloudInterfaces.Add(new SonyKDL60W855CloudInterface(_settingsSonyKDL60W855));

            CreateDeviceClient();
        }

        private void CreateDeviceClient()
        {
            _deviceClient = DeviceClient.CreateFromConnectionString(_connectionString, TransportType.Mqtt);
            _deviceClient.SetConnectionStatusChangesHandler(DeviceClientConnectionStatusChanged);

            foreach (var device in _deviceCloudInterfaces)
            {
                device.SetMethodHandlers(_deviceClient);
            }
        }

        private void DeviceClientConnectionStatusChanged(ConnectionStatus status, ConnectionStatusChangeReason reason)
        {
            _logger.Debug($"Status: {status.ToString()}, Reason: {reason.ToString()}");

            switch(status)
            {
                case ConnectionStatus.Disabled:
                case ConnectionStatus.Disconnected:
                    ResetConnection();
                    break;
                default:
                    break;
            }
        }

        private void ResetConnection()
        {
            _logger.Debug(string.Empty);

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
