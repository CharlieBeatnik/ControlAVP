using Microsoft.Azure.Devices.Client;
using Newtonsoft.Json;
using System;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using NLog;
using System.IO;
using NLog.Config;
using NLog.Targets;
using System.Collections.Generic;

//using Windows.UI.Core;
//using Windows.ApplicationModel.Core;
// The Background Application template is documented at http://go.microsoft.com/fwlink/?LinkID=533884&clcid=0x409

namespace ControlRelay
{
    public sealed class StartupTask : IBackgroundTask
    {
        private static BackgroundTaskDeferral _Deferral = null;

        private DeviceCloudInterfaceManager _deviceCloudInterfaceManager;
        private Logger _logger;

        public StartupTask()
        {
            //Windows.Storage.StorageFolder localFolder = Windows.Storage.ApplicationData.Current.LocalFolder;
            //var config = new LoggingConfiguration();
            //var logfile = new FileTarget("logfile")
            //{
            //        FileName = Path.Combine(localFolder.Path, "log.txt"),
            //        Layout = "${longdate}|${level:uppercase=true}|${callsite:className=True:fileName=False:includeSourcePath=False:methodName=True}|${message} ${exception:format=tostring}"
            //};
            //config.AddRule(LogLevel.Debug, LogLevel.Fatal, logfile);
            //LogManager.Configuration = config;
            _logger = LogManager.GetCurrentClassLogger();

            //Need to ensure System.Version types get serialised as a string in JSON, this ensure correct deserialisation.
            JsonConvert.DefaultSettings = () => new JsonSerializerSettings
            {
                Converters = new List<JsonConverter> { new Newtonsoft.Json.Converters.VersionConverter() }
            };
        }

        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            _Deferral = taskInstance.GetDeferral();

            await Windows.System.Threading.ThreadPool.RunAsync(workItem =>
            {
                _deviceCloudInterfaceManager = new DeviceCloudInterfaceManager("settings.json");
            });
        }

        ~StartupTask()
        {
        }
    }
}
