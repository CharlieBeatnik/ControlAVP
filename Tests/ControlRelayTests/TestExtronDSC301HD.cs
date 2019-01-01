using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;
using AudioVideoDevice;
using System.IO;
using System.Dynamic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;

namespace Tests
{
    [TestClass]
    public class TestExtronDSC301HD
    {
        private static ExtronDSC301HD _device = null;
        private readonly string _settingsFile = "settings.json";

        public TestExtronDSC301HD()
        {
            dynamic settings;
            using (StreamReader r = new StreamReader(_settingsFile))
            {
                string json = r.ReadToEnd();
                settings = JObject.Parse(json);
            }

            if (_device == null)
            {
                _device = new ExtronDSC301HD((string)settings.SelectToken("ExtronDSC301HD.SerialID"));
            }
        }

        [TestMethod]
        public void GivenDevice_WhenGetFirmware_ThenFirmwareIsNotNull()
        {
            var firmware = _device.GetFirmware();
            Assert.IsNotNull(firmware);
        }

        [TestMethod]
        public void GivenDevice_WhenGetFirmware_ThenFirmwareIs1_25_1()
        {
            var firmware = _device.GetFirmware();
            Assert.AreEqual(new Version(1,25,1,0), firmware);
        }
    }
}
