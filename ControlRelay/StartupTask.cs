using Microsoft.Azure.Devices.Client;
using Newtonsoft.Json;
using System;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
//using Windows.UI.Core;
//using Windows.ApplicationModel.Core;
// The Background Application template is documented at http://go.microsoft.com/fwlink/?LinkID=533884&clcid=0x409

namespace ControlRelay
{
    public sealed class StartupTask : IBackgroundTask
    {
        private static BackgroundTaskDeferral _Deferral = null;

        private CloudInterface _cloudInterface;

        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            _Deferral = taskInstance.GetDeferral();

            await Windows.System.Threading.ThreadPool.RunAsync(workItem =>
            {
                _cloudInterface = new CloudInterface();
            });
        }
    }
}
