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

            _deviceSettings = jsonParsed["SonyKDL60W855"];
        }

        public SonyKDL60W855 CreateDevice()
        {
            return new SonyKDL60W855(IPAddress.Parse(_deviceSettings["Host"].ToString()),
                                     PhysicalAddress.Parse(_deviceSettings["PhysicalAddress"].ToString()),
                                     _deviceSettings["PreSharedKey"].ToString());
        }

        [TestMethod]
        public void GivenDevice_WhenVolumeIsSetTo11_VolumeIs11()
        {
            using (var device = CreateDevice())
            {
                bool success = device.SetVolume(11);
                Assert.IsTrue(success);

                int volume = device.GetVolume();
                Assert.AreEqual(11, volume);
            }
        }

        public void GivenTVIsOff_WhenTurnOn_ThenTVIsOn()
        {
            //ANDREWDENN_TODO: Need to make sure TV is off first

            var result = _device.TurnOn();
            Assert.IsTrue(result);
        }

        public void GivenDevice_WhenCallAvailable_ThenDeviceIsAvailable()
        {
            Assert.IsTrue(_device.GetAvailable());
        }
    }
}
