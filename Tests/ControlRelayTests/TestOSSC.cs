using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ControllableDevice;
using System.IO;
using Newtonsoft.Json.Linq;
using ControllableDeviceTypes.OSSCTypes;

namespace Tests
{
    [TestClass]
    public class TestOSSC
    {
        private static readonly string _settingsFile = "settings.json";
        private static JToken _serialBlasterSettings;
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

            _serialBlasterSettings = jsonParsed["SerialBlaster"];
            _deviceSettings = jsonParsed["OSSC"];
        }

        public SerialBlaster CreateSerialBlaster()
        {
            return new SerialBlaster(_serialBlasterSettings["PortId"].ToString());
        }

        public SerialBlaster CreateInvalidSerialBlaster()
        {
            return new SerialBlaster("invalid");
        }

        public OSSC CreateDevice(SerialBlaster serialBlaster)
        {
            return new OSSC(serialBlaster);
        }

        [TestMethod]
        public void GivenDevice_WhenSendCommand_ThenResponseIsTrue()
        {
            using (var serialBlaster = CreateSerialBlaster())
            using (var device = CreateDevice(serialBlaster))
            {
                var result = device.SendCommand(CommandName.Menu);
                Assert.IsTrue(result);
            }
        }

        [TestMethod]
        public void GivenDeviceAndCommandSentAndDeviceDisposed_WhenCreateDeviceAndSendCommand_ThenResponseIsTrue()
        {
            using (var serialBlaster = CreateSerialBlaster())
            using (var device = CreateDevice(serialBlaster))
            {
                var result = device.SendCommand(CommandName.Menu);
                Assert.IsTrue(result);
            }

            using (var serialBlaster = CreateSerialBlaster())
            using (var device = CreateDevice(serialBlaster))
            {
                var result = device.SendCommand(CommandName.Menu);
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
                var result = device.SendCommand(CommandName.Menu);
                Assert.IsFalse(result);
            }
        }
    }
}
