using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ControllableDevice;
using System.IO;
using Newtonsoft.Json.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Diagnostics;

namespace Tests
{
    [TestClass]
    public class TestJsonRpcDevice
    {
        private const string _settingsFile = "settings.json";
        private static JToken _deviceSettings;

        private readonly TimeSpan _jsonRpcDeviceWebRequestTimeout = TimeSpan.FromSeconds(4);

        [ClassInitialize]
        public static void ClassInitialize(TestContext tc)
        {

            if (tc == null)
            {
                throw new ArgumentNullException(nameof(tc));
            }

            JObject jsonParsed;
            using (StreamReader r = new StreamReader(_settingsFile))
            {
                string json = r.ReadToEnd();
                jsonParsed = JObject.Parse(json);
            }

            _deviceSettings = jsonParsed["Devices"]["JsonRpcDevice"][0];
        }

        [ClassCleanup]
        public static void ClassCleanup()
        {
        }

        [TestInitialize()]
        public void TestInitialize()
        {
        }

        [TestCleanup()]
        public void TestCleanup()
        {
        }

        public JsonRpcDevice CreateInvalidIPDevice(TimeSpan webRequestTimeout)
        {
            var jsonRpcDevice = new JsonRpcDevice(IPAddress.Parse("192.0.2.0"), "1234", webRequestTimeout);
            jsonRpcDevice.RetryCountOnException = 10;
            jsonRpcDevice.WaitBeforeRetryOnException = TimeSpan.FromMilliseconds(100);

            return jsonRpcDevice;
        }

        public JsonRpcDevice CreateValidIPDevice(TimeSpan webRequestTimeout)
        {
            var jsonRpcDevice = new JsonRpcDevice(IPAddress.Parse(_deviceSettings["host"].ToString()),
                                     _deviceSettings["preSharedKey"].ToString(),
                                     webRequestTimeout);

            return jsonRpcDevice;
        }

        [TestMethod]
        public void GivenInvalidIPDevice_WhenPostWithInvlaidURL_ThentExceptionTimingsAreUsed()
        {
            using (var device = CreateInvalidIPDevice(TimeSpan.FromSeconds(0.5)))
            {
                device.RetryCountOnException = 2;
                device.WaitBeforeRetryOnException = TimeSpan.FromSeconds(1);

                device.RetryCountOnHttpRequestException = 0;
                device.WaitBeforeRetryOnHttpRequestException = TimeSpan.Zero;

                var json = new JObject(
                    new JProperty("version", "1.0")
                );

                var sw = new Stopwatch();

                sw.Start();
                var result = device.Post(json, "invalid/url");
                sw.Stop();

                // As the device itself is invalid, the webRequestTimeout will be hit and then there will be retries
                // timeout + (retryCount * (timeout + retryWait))
                Assert.IsTrue(sw.Elapsed > TimeSpan.FromSeconds(3.5)); 
                Assert.IsNull(result);
            }
        }

        [TestMethod]
        public void GivenValidIPDevice_WhenPostWithInvlaidURL_ThenHttpRequestExceptionTimingsAreUsed()
        {
            using (var device = CreateValidIPDevice(_jsonRpcDeviceWebRequestTimeout))
            {
                device.RetryCountOnException = 0;
                device.WaitBeforeRetryOnException = TimeSpan.Zero;

                device.RetryCountOnHttpRequestException = 2;
                device.WaitBeforeRetryOnHttpRequestException = TimeSpan.FromSeconds(1);

                var json = new JObject(
                    new JProperty("version", "1.0")
                );

                var sw = new Stopwatch();

                sw.Start();
                var result = device.Post(json, "invalid/url");
                sw.Stop();

                // As the device itself is valid, the webRequestTimeout will likely not be hit but there there will be retries
                // (retryCount * retryWait)
                Assert.IsTrue(sw.Elapsed > TimeSpan.FromSeconds(2));
                Assert.IsNull(result);
            }
        }
    }
}
