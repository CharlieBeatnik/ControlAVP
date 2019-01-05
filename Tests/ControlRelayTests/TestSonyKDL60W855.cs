using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ControllableDevice;
using System.IO;
using Newtonsoft.Json.Linq;
using System.Net;
using System.Net.NetworkInformation;

namespace Tests
{
    [TestClass]
    public class TestSonyKDL60W855
    {
        private static SonyKDL60W855 _device = null;
        private readonly string _settingsFile = "settings.json";

        private IPAddress _host;
        private PhysicalAddress _physicalAddress;

        public TestSonyKDL60W855()
        {
            JObject jsonParsed;
            using (StreamReader r = new StreamReader(_settingsFile))
            {
                string json = r.ReadToEnd();
                jsonParsed = JObject.Parse(json);
            }

            _host = IPAddress.Parse(jsonParsed["SonyKDL60W855"]["Host"].ToString());
            _physicalAddress = PhysicalAddress.Parse(jsonParsed["SonyKDL60W855"]["PhysicalAddress"].ToString());

            if (_device == null)
            {
                _device = new SonyKDL60W855(_host, _physicalAddress);
            }
        }

        [TestMethod]
        [Ignore("Can't turn TV off yet, so disabling for now.")]
        public void GivenTVIsOff_WhenTurnOn_ThenTVIsOn()
        {
            //ANDREWDENN_TODO: Need to make sure TV is off first

            var result = _device.TurnOn();
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void GivenDevice_WhenCallAvailable_ThenDeviceIsAvailable()
        {
            Assert.IsTrue(_device.Available);
        }
    }
}
