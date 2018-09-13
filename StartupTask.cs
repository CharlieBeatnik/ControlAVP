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

            SerialTest();

            var webserver = new WebServer();

            await Windows.System.Threading.ThreadPool.RunAsync(workItem =>
            {
                webserver.Start();
            });
        }

        private void SerialTest()
        {
            var hdmiSwitch1 = new AtenVS0801H("AK05UVF8A");

            hdmiSwitch1.SetStateGoTo(true);
            var state = hdmiSwitch1.GetState();

            hdmiSwitch1.SetStateGoTo(false);
            state = hdmiSwitch1.GetState();

            hdmiSwitch1.SetStateOutput(true);
            state = hdmiSwitch1.GetState();

            hdmiSwitch1.SetStateOutput(false);
            state = hdmiSwitch1.GetState();

            hdmiSwitch1.SetStateMode(AtenVS0801H.SwitchMode.Auto, 1);
            state = hdmiSwitch1.GetState();

            hdmiSwitch1.SetStateMode(AtenVS0801H.SwitchMode.Default);
            state = hdmiSwitch1.GetState();

            hdmiSwitch1.SetStateMode(AtenVS0801H.SwitchMode.Next);
            state = hdmiSwitch1.GetState();



            bool result;

            while (true)
            {
                for (int i = 1; i <= 8; i++)
                {
                    result = hdmiSwitch1.SetStateInput(i);
                    Debug.WriteLine(result);
                    Thread.Sleep(500);
                }
            }
        }
    }
}
