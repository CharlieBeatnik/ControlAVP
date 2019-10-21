using ControllableDevice;
using Microsoft.Azure.Devices.Client;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NLog;
using Polly;
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

        private static Logger _logger = LogManager.GetCurrentClassLogger();

        public DeviceCloudInterfaceManager(string connectionString, List<DeviceCloudInterface> deviceCloudInterfaces)
        {
            _connectionString = connectionString;
            _deviceCloudInterfaces = deviceCloudInterfaces;

            CreateDeviceClient();
        }

        private void AddCloudInterface<T>(Func<T> creator) where T : DeviceCloudInterface
        {
            try
            {
                T cloudInterface = creator();
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

            try
            {
                _logger.Debug("Pos: DeviceClient.CreateFromConnectionString");
                _deviceClient = DeviceClient.CreateFromConnectionString(_connectionString, TransportType.Mqtt);

                if (_deviceClient == null)
                {
                    _logger.Debug("_deviceClient is null after CreateFromConnectionString()");
                }

                _logger.Debug("Pos: SetConnectionStatusChangesHandler");
                _deviceClient.SetConnectionStatusChangesHandler(DeviceClientConnectionStatusChanged);

                foreach (var device in _deviceCloudInterfaces)
                {
                    _logger.Debug("Pos: SetMethodHandlers");

                    // SetMethodHandlerAsync has been observed to throw "System.TimeoutException: Operation timeout expired"
                    // SetMethodHandlerAsync has been observed to throw "Microsoft.Azure.Devices.Client.Exceptions.UnauthorizedException: CONNECT failed: RefusedServerUnavailable"
                    // This allows this function to contain centralised logic to catch one of theses exceptions and retry.
                    foreach (var methodHandlerInfo in device.GetMethodHandlerInfos(_deviceClient))
                    {
                        // Use a retry as we know it's possible to encounter serveral different exceptions
                        var policy = Policy
                                        .Handle<TimeoutException>()
                                        .Or<Microsoft.Azure.Devices.Client.Exceptions.UnauthorizedException>()
                                        .Retry(onRetry: (exception, retryCount) =>
                                        {
                                            _logger.Debug($"Exception of type '{exception.GetType().Name}' caught during SetMethodHandlerAsync()");
                                            _logger.Debug($"Full exception details will now be logged.");
                                            _logger.Debug(exception);
                                            _logger.Debug($"Retry #{retryCount}: SetMethodHandlerAsync({methodHandlerInfo.Name})");
                                        });

                        policy.Execute(() => _deviceClient.SetMethodHandlerAsync(methodHandlerInfo.Name, methodHandlerInfo.Handler, null).Wait());
                    }
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

            switch(status)
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
