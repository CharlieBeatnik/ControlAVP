using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ControllableDevice;
using System.IO;
using Newtonsoft.Json.Linq;
using ControllableDeviceTypes.ExtronDSC301HDTypes;
using System.Threading;

namespace Tests
{
    [TestClass]
    public class TestExtronDSC301HD
    {
        private static ExtronDSC301HD _device = null;
        private readonly string _settingsFile = "settings.json";

        public TestExtronDSC301HD()
        {
            JObject jsonParsed;
            using (StreamReader r = new StreamReader(_settingsFile))
            {
                string json = r.ReadToEnd();
                jsonParsed = JObject.Parse(json);
            }

            if (_device == null)
            {
                _device = new ExtronDSC301HD(jsonParsed["ExtronDSC301HD"]["PortId"].ToString());
            }
        }

        [TestMethod]
        public void GivenDevice_WhenGetFirmware_ThenFirmwareIsNotNull()
        {
            var firmware = _device.GetFirmware();
            Assert.IsNotNull(firmware);
        }

        [TestMethod]
        public void GivenDevice_WhenGetFirmware_ThenFirmwareIsGTE1_25_1()
        {
            var firmware = _device.GetFirmware();
            Assert.IsTrue(firmware >= new Version(1, 25, 1));
        }

        [TestMethod]
        public void GivenDevice_WhenCallAvailable_ThenDeviceIsAvailable()
        {
            Assert.IsTrue(_device.GetAvailable());
        }
    }
}
