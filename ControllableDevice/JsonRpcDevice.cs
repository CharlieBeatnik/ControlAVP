using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ControllableDevice
{
    class JsonRpcDevice : IDisposable
    {
        private bool _disposed = false;
        private IPAddress _host;
        private string _preSharedKey;
        private WebClient _webClient;

        public JsonRpcDevice(IPAddress host, string preSharedKey)
        {
            _host = host;
            _preSharedKey = preSharedKey;
            _webClient = new WebClient();
            _webClient.Headers.Add("X-Auth-PSK", _preSharedKey);
        }

        public JObject Post(JObject json, string path)
        {
            string address = $@"http://{_host.ToString()}/{path}";
            string data = json.ToString(Formatting.None);

            try
            {
                var response = _webClient.UploadString(address, "POST", data);
                return JObject.Parse(response);
            }
            catch(WebException we)
            {
                //The URI formed by combining BaseAddress and address is invalid.
                // or
                //There was no response from the server hosting the resource.
                return null;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
            }

            _disposed = true;
        }
    }
}
