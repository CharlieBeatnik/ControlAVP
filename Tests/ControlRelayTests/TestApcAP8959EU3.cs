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
using ControllableDeviceTypes.ApcAP8959EU3Types;

namespace Tests
{
    [TestClass]
    public class TestApcAP8959EU3
    {
        private const string _settingsFile = "settings.json";

        private static string _host;
        private static int _port;
        private static string _username;
        private static string _password;

        [ClassInitialize]
        public static void ClassInitialize(TestContext tc)
        {
            if(tc == null)
            {
                throw new ArgumentNullException(nameof(tc));
            }

            JObject jsonParsed;

            using (StreamReader r = new StreamReader(_settingsFile))
            {
                string json = r.ReadToEnd();
                jsonParsed = JObject.Parse(json);
            }

            _host = jsonParsed["Devices"]["ApcAP8959EU3"][0]["host"].ToString();
            _port = int.Parse(jsonParsed["Devices"]["ApcAP8959EU3"][0]["port"].ToString());
            _username = jsonParsed["Devices"]["ApcAP8959EU3"][0]["username"].ToString();
            _password = jsonParsed["Devices"]["ApcAP8959EU3"][0]["password"].ToString();
        }

        public static ApcAP8959EU3 CreateDevice()
        {
            return new ApcAP8959EU3(_host, _port, _username, _password);
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
        public void GivenDevice_WhenGetOutlets_ThenOutletCountIs24()
        {
            using (var device = CreateDevice())
            {
                var outlets = device.GetOutlets();
                Assert.IsNotNull(outlets);
                Assert.IsTrue(outlets.Count() == 24);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void GivenInvalidHost_WhenNewDevice_ThenExceptionThrown()
        {
            using (var device = new ApcAP8959EU3("0.0.0.0", _port, _username, _password))
            {
            }
        }

        [TestMethod]
        [ExpectedException(typeof(SocketException))]
        public void GivenInvalidPort_WhenNewDevice_ThenExceptionThrown()
        {
            using (var device = new ApcAP8959EU3(_host, 0, _username, _password))
            {
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void GivenInvalidUsername_WhenNewDevice_ThenExceptionThrown()
        {
            using (var device = new ApcAP8959EU3(_host, _port, "", _password))
            {
            }
        }

        [TestMethod]
        [ExpectedException(typeof(SshConnectionException))]
        public void GivenInvalidPassword_WhenNewDevice_ThenExceptionThrown()
        {
            using (var device = new ApcAP8959EU3(_host, _port, _username, ""))
            {
            }
        }

        [TestMethod]
        [Ignore("Risk of physical component wear, ignore by default.")]
        public void GivenDevice_WhenTurnOutletOn_ThenOutletIsOn()
        {
            string outletNamePartial = "Outlet";

            using (var device = CreateDevice())
            {
                bool success;
                var outlets = device.GetOutlets();
                if (outlets == null) Assert.Fail();

                var outlet = outlets.FirstOrDefault(o => o.Name.Contains(outletNamePartial, StringComparison.CurrentCulture));
                if (outlet == null) Assert.Fail();

                //Ensure outlet starts in the Off state
                if (outlet.State == Outlet.PowerState.On)
                {
                    success = device.TurnOutletOff(outlet.Id);
                    Assert.IsTrue(success);

                    outlets = device.GetOutletsWaitForPending();
                    if (outlets == null) Assert.Fail();

                    outlet = outlets.FirstOrDefault(o => o.Name.Contains(outletNamePartial, StringComparison.CurrentCulture));
                    if (outlet == null) Assert.Fail();
                    Assert.IsTrue(outlet.State == Outlet.PowerState.Off);
                }

                //Turn outlet on
                success = device.TurnOutletOn(outlet.Id);
                Assert.IsTrue(success);

                outlets = device.GetOutletsWaitForPending();
                if (outlets == null) Assert.Fail();

                //Check outlet is now on
                outlet = outlets.FirstOrDefault(o => o.Name.Contains(outletNamePartial, StringComparison.CurrentCulture));
                if (outlet == null) Assert.Fail();
                Assert.IsTrue(outlet.State == Outlet.PowerState.On);
            }
        }
    }
}
