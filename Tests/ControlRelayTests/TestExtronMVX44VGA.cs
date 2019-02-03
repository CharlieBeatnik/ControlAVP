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
        private static readonly string _settingsFile = "settings.json";
        private static JToken _deviceSettings;

        [ClassInitialize]
        public static void ClassInitialize(TestContext tc)
        {
            JObject jsonParsed;
            using (StreamReader r = new StreamReader(_settingsFile))
            {
                string json = r.ReadToEnd();
                jsonParsed = JObject.Parse(json);
            }

            _deviceSettings = jsonParsed["ExtronMVX44VGA"];
        }

        [ClassCleanup]
        public static void ClassCleanup()
        {
        }

        [TestInitialize]
        public void TestInitialize()
        {
            _device = new ExtronMVX44VGA(_deviceSettings["PortId"].ToString());
        }

        [TestCleanup]
        public void TestCleanup()
        {
            _device.Dispose();
            _device = null;
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
