using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ControllableDevice;
using System.IO;
using Newtonsoft.Json.Linq;
using ControllableDeviceTypes.SerialBlasterTypes;

namespace Tests
{
    [TestClass]
    public class TestSerialBlaster
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

            _deviceSettings = jsonParsed["Devices"]["SerialBlaster"][0];
        }

        public static SerialBlaster CreateDevice()
        {
            return new SerialBlaster(_deviceSettings["deviceIndex"].ToObject<uint>());
        }

        public static SerialBlaster CreateInvalidDevice()
        {
            return new SerialBlaster("invalid");
        }

        [TestMethod]
        public void GivenDevice_WhenSendCommand_ThenResponseIsTrue()
        {
            using (var device = CreateDevice())
            {
                var result = device.SendCommand(Protocol.Nec, 0x3EC14DB2);
                Assert.IsTrue(result);
            }
        }

        [TestMethod]
        public void GivenInvalidDevice_WhenSendCommand_ThenResponseIsFalse()
        {
            using (var device = CreateInvalidDevice())
            {
                var result = device.SendCommand(Protocol.Nec, 0x3EC14DB2);
                Assert.IsFalse(result);
            }
        }

        [TestMethod]
        public void GivenDevice_WhenGetEnabled_ThenDeviceIsEnabled()
        {
            using (var device = CreateDevice())
            {
                Assert.IsTrue(device.Enabled);
            }
        }

        [TestMethod]
        public void GivenInvalidDevice_WhenGetEnabled_ThenDeviceIsNotEnabled()
        {
            using (var device = CreateInvalidDevice())
            {
                Assert.IsFalse(device.Enabled);
            }
        }

        [TestMethod]
        public void GiveDevice_WhenSendMessageThenResponseIsTrue()
        {
            using (var device = CreateDevice())
            {
                var result = device.SendMessage("TestMessage");
                Assert.IsTrue(result);
            }
        }

        [TestMethod]
        public void GiveInvalidDevice_WhenSendMessageThenResponseIsFalse()
        {
            using (var device = CreateInvalidDevice())
            {
                var result = device.SendMessage("TestMessage");
                Assert.IsFalse(result);
            }
        }
    }
}
