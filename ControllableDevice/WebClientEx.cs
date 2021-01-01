using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ControllableDevice
{
    //Visual Studio tries to navigate to a Design View if you inherit from WebClient, this prevents it <shrug>
    [System.ComponentModel.DesignerCategory("")] 
    public class WebClientEx : WebClient
    {
        public TimeSpan WebRequestTimeout { get; set; }

        protected override WebRequest GetWebRequest(Uri address)
        {
            var request = base.GetWebRequest(address);

            if (WebRequestTimeout == TimeSpan.MaxValue)
            {
                request.Timeout = Timeout.Infinite;
            }
            else
            {
                request.Timeout = (int)WebRequestTimeout.TotalMilliseconds;
            }

            return request;
        }
    }
}
