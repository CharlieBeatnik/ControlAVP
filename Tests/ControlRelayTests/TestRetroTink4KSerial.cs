using Microsoft.VisualStudio.TestTools.UnitTesting;
using ControllableDevice;
using ControllableDeviceTypes.RetroTink4KTypes;
using System.IO;
using Newtonsoft.Json.Linq;
using System;

namespace Tests
{
    [TestClass]
    public class TestRetroTink4KSerial
    {
        private const string _settingsFile = "settings.json";
        private static JToken _deviceSettings;

        [ClassInitialize]
        public static void ClassInitialize(TestContext tc)
        {
            if (tc == null)
            {
                throw new ArgumentNullException(nameof(tc));
            }

            JObject jsonParsed;
            using (StreamReader r = new StreamReader(_settingsFile))
            {
                string json = r.ReadToEnd();
                jsonParsed = JObject.Parse(json);
            }

            _deviceSettings = jsonParsed["Devices"]["RetroTink4KSerial"][0];
        }

        public static RetroTink4KSerial CreateDevice()
        {
            return new RetroTink4KSerial(_deviceSettings["portId"].ToString());
        }

        public static RetroTink4KSerial CreateInvalidDevice()
        {
            return new RetroTink4KSerial("invalid");
        }

        [TestMethod]
        public void GivenDevice_WhenGetAvailable_ThenDeviceIsAvailable()
        {
            using (var device = CreateDevice())
            {
                Assert.IsTrue(device.GetAvailable());
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void GivenEmptyPartialId_WhenNewDevice_ThenExceptionThrown()
        {
            using (var device = new AtenVS0801H(string.Empty))
            {
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void GivenNullPartialId_WhenNewDevice_ThenExceptionThrown()
        {
            using (var device = new AtenVS0801H(null))
            {
            }
        }

        [TestMethod]
        public void GivenInvalidPartialId_WhenNewDevice_ThenDeviceIsNotAvailable()
        {
            using (var device = new AtenVS0801H("invalid"))
            {
                Assert.IsFalse(device.GetAvailable());
            }
        }

        [TestMethod]
        public void GivenDevice_WhenCreateAnotherDeviceWithSameID_ThenSecondDeviceIsNotAvailable()
        {
            using (var device1 = CreateDevice())
            {
                Assert.IsTrue(device1.GetAvailable());
                using (var device2 = CreateDevice())
                {
                    Assert.IsFalse(device2.GetAvailable());
                }
            }
        }

        [TestMethod]
        public void GivenInvalidDevice_WhenGetAvailable_ThenDeviceIsNotAvailable()
        {
            using (var device = CreateInvalidDevice())
            {
                Assert.IsFalse(device.GetAvailable());
            }
        }

        [TestMethod]
        public void GivenInvalidDevice_WhenSendCommand_ThenResultIsFalse()
        {
            using (var device = CreateInvalidDevice())
            {
                Assert.IsFalse(device.SendCommand(CommandName.Aux8));
            }
        }

        [TestMethod]
        public void GivenInvalidDevice_WhenTurnOn_ThenResultIsFalse()
        {
            using (var device = CreateInvalidDevice())
            {
                Assert.IsFalse(device.TurnOn());
            }
        }

        [TestMethod]
        public void GivenInvalidDevice_WhenTurnOff_ThenResultIsFalse()
        {
            using (var device = CreateInvalidDevice())
            {
                Assert.IsFalse(device.TurnOff());
            }
        }

        [TestMethod]
        public void GivenInvalidDevice_WhenLoadProfile_ThenResultIsFalse()
        {
            using (var device = CreateInvalidDevice())
            {
                Assert.IsFalse(device.LoadProfile(0));
            }
        }
    }
}
