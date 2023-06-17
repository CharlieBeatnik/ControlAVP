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
        [ClassInitialize]
        public static void ClassInitialize(TestContext tc)
        {
            if (tc == null)
            {
                throw new ArgumentNullException(nameof(tc));
            }
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

        public static JsonRpcDevice CreateInvalidIPDevice(TimeSpan webRequestTimeout, int retryCount, TimeSpan waitBeforeRetry)
        {
            return new JsonRpcDevice(IPAddress.Parse("192.0.2.0"), "1234", webRequestTimeout, retryCount, waitBeforeRetry);
        }

        [TestMethod]
        public void GivenInvalidIPDeviceWith500msTimeout10Retries100msWaitBeforeRetry_WhenPost_ThenResultIsNullAndDurationIsLessThan7000ms()
        {
            using (var device = CreateInvalidIPDevice(TimeSpan.FromMilliseconds(500), 10, TimeSpan.FromMilliseconds(100)))
            {
                var sw = new Stopwatch();
                var json = new JObject(
                    new JProperty("version", "1.0")
                );

                sw.Start();
                var result = device.Post(json, "invalid/url");
                sw.Stop();

                Assert.IsNull(result);
                Assert.IsTrue(sw.ElapsedMilliseconds < 7000);
            }
        }
    }
}
