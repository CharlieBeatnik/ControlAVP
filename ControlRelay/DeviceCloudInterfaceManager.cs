using Microsoft.Azure.Devices.Client;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NLog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ControlRelay
{
    class DeviceCloudInterfaceManager
    {
        private List<DeviceCloudInterface> _deviceCloudInterfaces = new List<DeviceCloudInterface>();
        private DeviceClient _deviceClient = null;
        private ConnectionStatus? _latestStatus = null;
        private ConnectionStatusChangeReason? _latestReason = null;
        private readonly TimeSpan _createDeviceTimeout = TimeSpan.FromSeconds(5);

        private string _connectionString;
        private List<AtenVS0801HCloudInterface.Settings> _settingsAtenVS0801H;
        private ApcAP8959EU3CloudInterface.Settings _settingsApcAP8959EU3;
        private ExtronDSC301HDCloudInterface.Settings _settingsExtronDSC301HD;
        private SonyKDL60W855CloudInterface.Settings _settingsSonyKDL60W855;
        private ExtronMVX44VGACloudInterface.Settings _settingsExtronMVX44VGA;

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
            AddCloudInterface(_settingsAtenVS0801H, (x) => new AtenVS0801HCloudInterface(x));

            // ApcAP8959EU Settings 
            _settingsApcAP8959EU3 = JsonConvert.DeserializeObject<ApcAP8959EU3CloudInterface.Settings>(jsonParsed["ApcAP8959EU3"].ToString());
            AddCloudInterface(_settingsApcAP8959EU3, (x) => new ApcAP8959EU3CloudInterface(x));

            //ExtronDSC301HD Settings 
            _settingsExtronDSC301HD = JsonConvert.DeserializeObject<ExtronDSC301HDCloudInterface.Settings>(jsonParsed["ExtronDSC301HD"].ToString());
            AddCloudInterface(_settingsExtronDSC301HD, (x) => new ExtronDSC301HDCloudInterface(x));

            //SonyKDL60W855 Settings 
            _settingsSonyKDL60W855 = JsonConvert.DeserializeObject<SonyKDL60W855CloudInterface.Settings>(jsonParsed["SonyKDL60W855"].ToString());
            AddCloudInterface(_settingsSonyKDL60W855, (x) => new SonyKDL60W855CloudInterface(x));

            //ExtronMVX44VGA Settings 
            _settingsExtronMVX44VGA = JsonConvert.DeserializeObject<ExtronMVX44VGACloudInterface.Settings>(jsonParsed["ExtronMVX44VGA"].ToString());
            AddCloudInterface(_settingsExtronMVX44VGA, (x) => new ExtronMVX44VGACloudInterface(x));

            CreateDeviceClient();
        }

        private void AddCloudInterface<T, U>(U settings, Func<U, T> creator) where T : DeviceCloudInterface
        {
            try
            {
                T cloudInterface = creator(settings);
                _deviceCloudInterfaces.Add(cloudInterface);
            }
            catch (Exception)
            {

            }
        }

        private void CreateDeviceClient()
        {
            _logger.Debug(string.Empty);

            _deviceClient = null;
            _latestStatus = null;
            _latestReason = null;

            try
            {
                _logger.Debug("Pos: DeviceClient.CreateFromConnectionString");
                _deviceClient = DeviceClient.CreateFromConnectionString(_connectionString, TransportType.Mqtt);

                if (_deviceClient == null)
                {
                    _logger.Debug("_deviceClient is null after CreateFromConnectionString()");
                }

                // Wait for the DeviceClient to connect
                _logger.Debug("Wait for Status: Connnected, Reason: Connection_Ok");
                bool connected = false;
                var sw = new Stopwatch();
                sw.Start();

                do
                {
                    if ((_latestStatus == ConnectionStatus.Connected) && (_latestReason == ConnectionStatusChangeReason.Connection_Ok))
                    {
                        connected = true;
                    }
                }
                while (!connected && (sw.Elapsed < _createDeviceTimeout));

                if (!connected)
                {
                    throw new Exception($"Unable to connect DeviceClient within {sw.Elapsed.TotalSeconds} seconds. Aborting CreateDeviceClient()");
                }

                _logger.Debug("Successfully connected");
                sw.Stop();

                _logger.Debug("Pos: SetConnectionStatusChangesHandler");
                _deviceClient.SetConnectionStatusChangesHandler(DeviceClientConnectionStatusChanged);

                foreach (var device in _deviceCloudInterfaces)
                {
                    _logger.Debug("Pos: SetMethodHandlers");
                    device.SetMethodHandlers(_deviceClient);
                }
            }
            catch(Exception exp)
            {
                _logger.Error(exp);
            }
        }

        private void DeviceClientConnectionStatusChanged(ConnectionStatus status, ConnectionStatusChangeReason reason)
        {
            _logger.Debug($"Status: {status.ToString()}, Reason: {reason.ToString()}");
            _latestStatus = status;
            _latestReason = reason;

            switch (status)
            {
                case ConnectionStatus.Disabled:
                case ConnectionStatus.Disconnected:
                    ResetConnection(status, reason);
                    break;
                default:
                    break;
            }
        }

        private void ResetConnection(ConnectionStatus status, ConnectionStatusChangeReason reason)
        {
            _logger.Debug("Resetting Connection");

            // Attempt to close any existing connections before creating a new one
            if (_deviceClient != null)
            {
                try
                {
                    if (reason == ConnectionStatusChangeReason.Retry_Expired)
                    {
                        // It has been observed that closing the device client with reason 'Retry_Expired' 
                        // results in an assert, so don't call close and log the action taken.
                        // Exception:
                        //     DotNetty.Transport.Channels.ClosedChannelException: I/O error occurred.
                        //
                        _logger.Debug($"As reason was {reason.ToString()} skipping '_deviceClient.CloseAsync().Wait()'");
                    }
                    else
                    {
                        _deviceClient.CloseAsync().Wait();
                    }
                }
                catch (AggregateException ae)
                {
                    //ANDREWDENN_TODO: Once "IotHubClientException" is public then make this catch 
                    //explicitly look for IotHubClientException and SocketException
                    //https://docs.microsoft.com/en-us/dotnet/api/system.aggregateexception.flatten?view=netframework-4.7.2
                    _logger.Debug(ae, "_deviceClient.CloseAsync().Wait()");
                }
            }

            // Create new connection
            CreateDeviceClient();
        }
    }
}
