using Microsoft.VisualStudio.TestTools.UnitTesting;
using ControllableDevice;
using System.Linq;
using Newtonsoft.Json;
using System.IO;
using Newtonsoft.Json.Linq;
using System.Threading;
using System;
using Renci.SshNet.Common;
using System.Net.Sockets;

namespace Tests
{
    [TestClass]
    public class TestExtronIPL250
    {
        private const string _settingsFile = "settings.json";

        private static string _host;
        private static int _port;

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

            _host = jsonParsed["Devices"]["ExtronIPL250"][0]["host"].ToString();
            _port = int.Parse(jsonParsed["Devices"]["ExtronIPL250"][0]["port"].ToString());
        }

        public static ExtronIPL250 CreateDevice()
        {
            return new ExtronIPL250(_host, _port);
        }

        [TestMethod]
        public void GivenDevice_WhenCallAvailable_ThenDeviceIsAvailable()
        {
            using (var device = CreateDevice())
            {
                Assert.IsTrue(device.GetAvailable());
            }
        }

        [TestMethod]
        public void GivenDevice_Test()
        {
            using (var device = CreateDevice())
            {
                device.Test();
            }
        }

    }
}
