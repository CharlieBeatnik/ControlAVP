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

            _deviceSettings = jsonParsed["OSSC"];
        }
        
        public OSSC CreateDevice()
        {
            return new OSSC(_deviceSettings["PortId"].ToString());
        }

        [TestMethod]
        public void GivenDevice_WhenSendCommandFromUint_ThenResponseIsOK()
        {
            using (var device = CreateDevice())
            {
                var result = device.SendCommand(0x3EC14DB2);
                Assert.IsTrue(result);
            }
        }

        [TestMethod]
        public void GivenDevice_WhenSendCommandFromEnum_ThenResponseIsOK()
        {
            using (var device = CreateDevice())
            {
                var result = device.SendCommand(CommandName.Menu);
                Assert.IsTrue(result);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void GivenDevice_WhenSendCommandUsingInvalidEnum_ThenExceptionThrown()
        {
            using (var device = CreateDevice())
            {
                var result = device.SendCommand((CommandName)int.MaxValue);
                Assert.IsTrue(result);
            }
        }
    }
}
