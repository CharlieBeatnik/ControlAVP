using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ControllableDevice;
using System.IO;
using Newtonsoft.Json.Linq;
using System.Threading;

namespace Tests
{
    [TestClass]
    public class TestManagedRs232Device
    {
        private readonly string _settingsFile = "settings.json";
        private JObject _settings;

        public TestManagedRs232Device()
        {
            using (StreamReader r = new StreamReader(_settingsFile))
            {
                string json = r.ReadToEnd();
                _settings = JObject.Parse(json);
            }
        }

        private ManagedRs232Device CreateDevice()
        {
            var device = new ManagedRs232Device(_settings["OSSC"]["PortId"].ToString());
            return device;
        }

        [TestMethod]
        public void GivenDevice_WhenCreateDeviceCorrectPartialId_ThenDeviceIsEnabled()
        {
            using (var device = CreateDevice())
            {
                Assert.IsTrue(device.Enabled);
            }
        }

        [TestMethod]
        public void GivenDevice_WhenCreateDeviceWithUnknownPartialId_ThenDeviceIsNotEnabled()
        {
            using (var device = new ManagedRs232Device("invalid"))
            {
                Assert.IsFalse(device.Enabled);
            }
        }
    }
}
