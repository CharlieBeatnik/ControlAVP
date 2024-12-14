using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ControllableDevice;
using System.IO;
using Newtonsoft.Json.Linq;
using ControllableDeviceTypes.RetroTink4KTypes;

namespace Tests
{
    [TestClass]
    public class TestRetroTink4K
    {
        private const string _settingsFile = "settings.json";
        private static JToken _serialBlasterSettings;

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

            _serialBlasterSettings = jsonParsed["Devices"]["SerialBlaster"][0];
        }

        public static SerialBlaster CreateSerialBlaster()
        {
            return new SerialBlaster(_serialBlasterSettings["portId"].ToString());
        }

        public static SerialBlaster CreateInvalidSerialBlaster()
        {
            return new SerialBlaster("invalid");
        }

        public static RetroTink4K CreateDevice(SerialBlaster serialBlaster)
        {
            return new RetroTink4K(serialBlaster);
        }

        [TestMethod]
        public void GivenDevice_WhenSendCommand_ThenResponseIsTrue()
        {
            using (var serialBlaster = CreateSerialBlaster())
            using (var device = CreateDevice(serialBlaster))
            {
                var result = device.SendCommand(CommandName.Back);
                Assert.IsTrue(result);
            }
        }

        [TestMethod]
        public void GivenDeviceAndCommandSentAndDeviceDisposed_WhenCreateDeviceAndSendCommand_ThenResponseIsTrue()
        {
            using (var serialBlaster = CreateSerialBlaster())
            using (var device = CreateDevice(serialBlaster))
            {
                var result = device.SendCommand(CommandName.Back);
                Assert.IsTrue(result);
            }

            using (var serialBlaster = CreateSerialBlaster())
            using (var device = CreateDevice(serialBlaster))
            {
                var result = device.SendCommand(CommandName.Back);
                Assert.IsTrue(result);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void GivenDevice_WhenSendCommandUsingInvalidEnum_ThenExceptionThrown()
        {
            using (var serialBlaster = CreateSerialBlaster())
            using (var device = CreateDevice(serialBlaster))
            {
                var result = device.SendCommand((CommandName)int.MaxValue);
                Assert.IsTrue(result);
            }
        }

        [TestMethod]
        public void GivenDevice_WhenGetAvailable_ThenDeviceIsAvailable()
        {
            using (var serialBlaster = CreateSerialBlaster())
            using (var device = CreateDevice(serialBlaster))
            {
                Assert.IsTrue(device.GetAvailable());
            }
        }

        [TestMethod]
        public void GivenInvalidDevice_WhenGetAvailable_ThenDeviceIsNotAvailable()
        {
            using (var serialBlaster = CreateInvalidSerialBlaster())
            using (var device = CreateDevice(serialBlaster))
            {
                Assert.IsFalse(device.GetAvailable());
            }
        }

        [TestMethod]
        public void GivenInvalidDevice_WhenSendCommandFromEnum_ThenResponseIsFalse()
        {
            using (var serialBlaster = CreateInvalidSerialBlaster())
            using (var device = CreateDevice(serialBlaster))
            {
                var result = device.SendCommand(CommandName.Back);
                Assert.IsFalse(result);
            }
        }
    }
}
