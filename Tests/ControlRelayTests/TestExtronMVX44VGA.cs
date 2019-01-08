using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ControllableDevice;
using System.IO;
using Newtonsoft.Json.Linq;

namespace Tests
{
    [TestClass]
    public class TestExtronMVX44VGA
    {
        private static ExtronMVX44VGA _device = null;
        private readonly string _settingsFile = "settings.json";

        public TestExtronMVX44VGA()
        {
            JObject jsonParsed;
            using (StreamReader r = new StreamReader(_settingsFile))
            {
                string json = r.ReadToEnd();
                jsonParsed = JObject.Parse(json);
            }

            if (_device == null)
            {
                _device = new ExtronMVX44VGA(jsonParsed["ExtronMVX44VGA"]["PortId"].ToString());
            }
        }

        [TestMethod]
        public void GivenDevice_WhenGetFirmware_ThenFirmwareIsNotNull()
        {
            var firmware = _device.GetFirmware();
            Assert.IsNotNull(firmware);
        }

        [TestMethod]
        public void GivenDevice_WhenGetFirmware_ThenFirmwareIsGTE1_4()
        {
            var firmware = _device.GetFirmware();
            Assert.IsTrue(firmware >= new Version(1, 4));
        }

        [TestMethod]
        public void GivenDevice_WhenCallAvailable_ThenDeviceIsAvailable()
        {
            Assert.IsTrue(_device.GetAvailable());
        }
    }
}
