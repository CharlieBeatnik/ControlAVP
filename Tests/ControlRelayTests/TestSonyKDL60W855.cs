using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ControllableDevice;
using System.IO;
using Newtonsoft.Json.Linq;
using System.Net;
using System.Net.NetworkInformation;
using ControllableDeviceTypes.SonyKDL60W855Types;

namespace Tests
{
    [TestClass]
    public class TestSonyKDL60W855
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

            _deviceSettings = jsonParsed["SonyKDL60W855"];
        }

        [ClassCleanup]
        public static void ClassCleanup()
        {
            using (var device = CreateDevice())
            {
                Assert.IsTrue(device.TurnOff());
            }
        }

        [TestInitialize()]
        public void TestInitialize()
        {
            using (var device = CreateDevice())
            {
                Assert.IsTrue(device.TurnOn());
            }
        }

        [TestCleanup()]
        public void TestCleanup()
        {
        }


        public static SonyKDL60W855 CreateDevice()
        {
            return new SonyKDL60W855(IPAddress.Parse(_deviceSettings["Host"].ToString()),
                                     PhysicalAddress.Parse(_deviceSettings["PhysicalAddress"].ToString()),
                                     _deviceSettings["PreSharedKey"].ToString());
        }

        public static SonyKDL60W855 CreateInvalidIPDevice()
        {
            return new SonyKDL60W855(IPAddress.Parse("192.0.2.0"),
                                     PhysicalAddress.Parse(_deviceSettings["PhysicalAddress"].ToString()),
                                     _deviceSettings["PreSharedKey"].ToString());
        }

        [TestMethod]
        public void GivenInvalidIPDevice_WhenGetPowerStatus_ThenPowerStatusIsOff()
        {
            using (var device = CreateInvalidIPDevice())
            {
                var powerStatus = device.GetPowerStatus();
                Assert.IsTrue(powerStatus == PowerStatus.Off);
            }
        }

        [TestMethod]
        public void GivenDevice_WhenVolumeIsSetTo11_ThenVolumeIs11()
        {
            using (var device = CreateDevice())
            {
                bool success = device.SetVolume(11);
                Assert.IsTrue(success);

                int volume = device.GetVolume();
                Assert.AreEqual(11, volume);
            }
        }

        [TestMethod]
        public void GivenTVIsOff_WhenTurnOn_ThenTVIsOn()
        {
            using (var device = CreateDevice())
            {
                Assert.IsTrue(device.TurnOff());
                Assert.IsTrue(device.TurnOn());
            }
        }

        public void GivenDevice_WhenCallAvailable_ThenDeviceIsAvailable()
        {
            using (var device = CreateDevice())
            {
                Assert.IsTrue(device.GetAvailable());
            }
        }
    }
}
