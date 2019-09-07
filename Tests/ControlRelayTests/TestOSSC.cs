using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ControllableDevice;
using System.IO;
using Newtonsoft.Json.Linq;

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
        public void GivenDevice_WhenSendCommand_ThenResponseIsOK()
        {
            using (var device = CreateDevice())
            {
                var result = device.SendCommand(0x3EC14DB2);
                Assert.IsTrue(result);
            }
        }
    }
}
