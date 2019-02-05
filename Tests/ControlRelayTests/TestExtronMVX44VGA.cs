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

        public ExtronMVX44VGA CreateDevice()
        {
            return ExtronMVX44VGA.Create(_deviceSettings["PortId"].ToString());
        }

        [TestMethod]
        public void GivenDevice_WhenGetFirmware_ThenFirmwareIsNotNull()
        {
            using (var device = CreateDevice())
            {
                var firmware = device.GetFirmware();
                Assert.IsNotNull(firmware);
            }
        }

        [TestMethod]
        public void GivenDevice_WhenGetFirmware_ThenFirmwareIsGTE1_4()
        {
            using (var device = CreateDevice())
            {
                var firmware = device.GetFirmware();
                Assert.IsTrue(firmware >= new Version(1, 4));
            }
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
        public void GivenEmptyPartialId_WhenNewDevice_ThenDeviceIsNull()
        {
            var device = ExtronMVX44VGA.Create(string.Empty);
            Assert.IsNull(device);
        }

        [TestMethod]
        public void GivenNullPartialId_WhenNewDevice_ThenDeviceIsNull()
        {
            var device = ExtronMVX44VGA.Create(null);
            Assert.IsNull(device);
        }

        [TestMethod]
        public void GivenInvalidPartialId_WhenNewDevice_ThenDeviceIsNull()
        {
            var device = ExtronMVX44VGA.Create("invalid");
            Assert.IsNull(device);
        }
    }
}
