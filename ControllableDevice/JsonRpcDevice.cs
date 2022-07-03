using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NLog;
using Polly;

namespace ControllableDevice
{
    class JsonRpcDevice : IDisposable
    {
        private bool _disposed;
        private IPAddress _host;
        private string _preSharedKey;
        private WebClientEx _webClient;

        private static readonly int _jsonPostRetryCount = 3;
        private static readonly TimeSpan _jsonPostWaitBeforeRetry = TimeSpan.FromSeconds(1);

        private static Logger _logger = LogManager.GetCurrentClassLogger();

        public JsonRpcDevice(IPAddress host, string preSharedKey, TimeSpan webRequestTimeout)
        {
            _host = host;
            _preSharedKey = preSharedKey;
            _webClient = new WebClientEx();
            _webClient.Headers.Add("X-Auth-PSK", _preSharedKey);
            _webClient.WebRequestTimeout = webRequestTimeout;
        }

        public JObject Post(JObject json, string path)
        {
            lock (_webClient)
            {
                string address = $@"http://{_host.ToString()}/{path}";
                string data = json.ToString(Formatting.None);
                string response = string.Empty;

                var policy = Policy<JObject>
                    .Handle<WebException>()
                    .WaitAndRetry(retryCount: _jsonPostRetryCount,
                                  sleepDurationProvider: _ => _jsonPostWaitBeforeRetry,
                                  onRetry: (exception, sleepDuration, attemptNumber, context) =>
                                  {
                                      _logger.Debug($"attemptNumber: {attemptNumber}");
                                      _logger.Debug($"address: {address}");
                                      _logger.Debug($"data: {data}");
                                      _logger.Debug($"response: {response}");
                                      _logger.Debug(exception.ToString());
                                  });

                try
                {
                    return policy.Execute(() =>
                    {
                        response = _webClient.UploadString(address, "POST", data);
                        return JObject.Parse(response);
                    });
                }
                catch(WebException)
                {
                    return null;
                }

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
                _webClient.Dispose();
            }

            _disposed = true;
        }
    }
}
