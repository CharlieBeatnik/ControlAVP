using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NLog;
using Polly;
using Polly.Fallback;
using Polly.Retry;

namespace ControllableDevice
{
    public class JsonRpcDevice : IDisposable
    {
        private bool _disposed;
        private readonly IPAddress _host;
        private readonly string _preSharedKey;
        private readonly HttpClient _httpClient;

        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        // General Exceptions
        public int RetryCountOnException { get; set; } = 2;
        public TimeSpan WaitBeforeRetryOnException { get; set; } = TimeSpan.FromSeconds(3);

        // HttpRequestExceptions
        public int RetryCountOnHttpRequestException { get; set; } = 2;
        public TimeSpan WaitBeforeRetryOnHttpRequestException { get; set; } = TimeSpan.FromSeconds(3);

        public JsonRpcDevice(IPAddress host, string preSharedKey, TimeSpan httpRequestTimeout)
        {
            _host = host;
            _preSharedKey = preSharedKey;

            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Add("X-Auth-PSK", _preSharedKey);
            _httpClient.Timeout = httpRequestTimeout;
        }

        public JObject Post(JObject json, string path)
        {
            lock (_httpClient)
            {
                string address = $@"http://{_host}/{path}";
                string data = json.ToString(Formatting.None);

                HttpResponseMessage response = null;

                var policyException = Policy<JObject>
                    .Handle<Exception>(ex => !(ex is HttpRequestException))
                    .WaitAndRetry(retryCount: RetryCountOnException,
                                  sleepDurationProvider: _ => WaitBeforeRetryOnException,
                                  onRetry: (exception, sleepDuration, attemptNumber, context) =>
                                  {
                                      _logger.Debug("Handle<Exception>");
                                      _logger.Debug($"attemptNumber: {attemptNumber}");
                                      _logger.Debug($"address: {address}");
                                      _logger.Debug($"data: {data}");
                                      _logger.Debug($"exception: {exception.Exception.Message}");
                                  });

                var policyHttpRequestException = Policy<JObject>
                    .Handle<HttpRequestException>()
                    .WaitAndRetry(retryCount: RetryCountOnHttpRequestException,
                                  sleepDurationProvider: _ => WaitBeforeRetryOnHttpRequestException,
                                  onRetry: (exception, sleepDuration, attemptNumber, context) =>
                                  {
                                      _logger.Debug("Handle<HttpRequestException>");
                                      _logger.Debug($"attemptNumber: {attemptNumber}");
                                      _logger.Debug($"address: {address}");
                                      _logger.Debug($"data: {data}");
                                      _logger.Debug($"exception: {exception.Exception.Message}");
                                  });

                var policyWrap = Policy.Wrap(policyException, policyHttpRequestException);

                try
                {
                    return policyWrap.Execute(() =>
                    {
                        response = null;
                        var httpContent = new StringContent(data, Encoding.UTF8, "application/json");

                        response = Task.Run(async () => await _httpClient.PostAsync(address, httpContent)).Result;
                        response.EnsureSuccessStatusCode();
                        string responseBody = Task.Run(async () => await response.Content.ReadAsStringAsync()).Result;
                        return JObject.Parse(responseBody);
                    });
                }
                catch(Exception)
                {
                    //This catch is reached if the final policy.Execute fails.
                    //It is never reached if one of the policy.Execute attempts succeeds.
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
