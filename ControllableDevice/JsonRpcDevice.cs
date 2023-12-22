using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NLog;
using Polly;

namespace ControllableDevice
{
    public class JsonRpcDevice : IDisposable
    {
        private bool _disposed;
        private readonly IPAddress _host;
        private readonly string _preSharedKey;
        private readonly HttpClient _httpClient;

        private readonly int _jsonPostRetryCount;
        private readonly TimeSpan _jsonPostWaitBeforeRetry;

        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        public JsonRpcDevice(IPAddress host, string preSharedKey, TimeSpan httpRequestTimeout, int retryCount, TimeSpan waitBeforeRetry)
        {
            _host = host;
            _preSharedKey = preSharedKey;

            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Add("X-Auth-PSK", _preSharedKey);
            _httpClient.Timeout = httpRequestTimeout;

            _jsonPostRetryCount = retryCount;
            _jsonPostWaitBeforeRetry = waitBeforeRetry;
        }

        public JObject Post(JObject json, string path)
        {
            lock (_httpClient)
            {
                string address = $@"http://{_host}/{path}";
                string data = json.ToString(Formatting.None);

                var policy = Policy<JObject>
                    .Handle<Exception>()
                    .WaitAndRetry(retryCount: _jsonPostRetryCount,
                                  sleepDurationProvider: _ => _jsonPostWaitBeforeRetry,
                                  onRetry: (exception, sleepDuration, attemptNumber, context) =>
                                  {
                                      _logger.Debug($"attemptNumber: {attemptNumber}");
                                      _logger.Debug($"address: {address}");
                                      _logger.Debug($"data: {data}");
                                  });

                try
                {
                    return policy.Execute(() =>
                    {
                        var httpContent = new StringContent(data, Encoding.UTF8, "application/json");

                        HttpResponseMessage response = Task.Run(async () => await _httpClient.PostAsync(address, httpContent)).Result;
                        response.EnsureSuccessStatusCode();
                        string responseBody = Task.Run(async () => await response.Content.ReadAsStringAsync()).Result;
                        return JObject.Parse(responseBody);
                    });
                }
                catch(Exception)
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
                _httpClient.Dispose();
            }

            _disposed = true;
        }
    }
}
