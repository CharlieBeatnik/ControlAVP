using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Http;
using Windows.ApplicationModel.Background;
using Windows.System.Threading;
using System.IO.Ports;
using Windows.Devices.Enumeration;
using WindowsSerialDevice = Windows.Devices.SerialCommunication.SerialDevice;
using Windows.Devices.SerialCommunication;
using System.Threading;
using Windows.Storage.Streams;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Diagnostics;

// The Background Application template is documented at http://go.microsoft.com/fwlink/?LinkID=533884&clcid=0x409



namespace ComControl
{
    public sealed class StartupTask : IBackgroundTask
    {
        private static BackgroundTaskDeferral _Deferral = null;

        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            _Deferral = taskInstance.GetDeferral();

            await SerialTest();

            var webserver = new WebServer();

            await Windows.System.Threading.ThreadPool.RunAsync(workItem =>
            {
                webserver.Start();
            });
        }

        private async Task SerialTest()
        {
            var hdmiSwitch1 = new AtenVS0801H("AK05UVF8A");

            while (true)
            {
                string response = await hdmiSwitch1.WriteStringReadString("sw+");
                Debug.WriteLine(response);
                Thread.Sleep(1000);
            }
        }
    }
}
