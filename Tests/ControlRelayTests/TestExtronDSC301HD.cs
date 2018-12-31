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

namespace Tests
{
    [TestClass]
    public class TestExtronDSC301HD
    {
        private static ExtronDSC301HD _device = null;
        private dynamic _settings;
        private readonly string _settingsFile = "settings.json";

        public TestExtronDSC301HD()
        {
            using (StreamReader r = new StreamReader(_settingsFile))
            {
                string json = r.ReadToEnd();
                _settings = JsonConvert.DeserializeObject<ExpandoObject>(json, new ExpandoObjectConverter());
            }

            if (_device == null)
            {
                _device = new ExtronDSC301HD(_settings.ExtronDSC301HD[0].SerialID);
            }
        }

        [TestMethod]
        public void GivenDevice_WhenGetFirmware_ThenFirmwareIsNotNull()
        {
            var firmware = _device.GetFirmware();
            Assert.IsNotNull(firmware);
        }
    }
}
