using Microsoft.VisualStudio.TestTools.UnitTesting;
using ControllableDevice;
using System.Linq;
using Newtonsoft.Json;
using System.IO;
using Newtonsoft.Json.Linq;
using System.Threading;

namespace Tests
{
    [TestClass]
    public class TestApcAP8959EU3
    {
        private static readonly string _settingsFile = "settings.json";

        private static string _host;
        private static int _port;
        private static string _username;
        private static string _password;

        [ClassInitialize]
        public static void ClassInitialize(TestContext tc)
        {
            JObject jsonParsed;

            using (StreamReader r = new StreamReader(_settingsFile))
            {
                string json = r.ReadToEnd();
                jsonParsed = JObject.Parse(json);
            }

            _host = jsonParsed["ApcAP8959EU3"]["Host"].ToString();
            _port = int.Parse(jsonParsed["ApcAP8959EU3"]["Port"].ToString());
            _username = jsonParsed["ApcAP8959EU3"]["Username"].ToString();
            _password = jsonParsed["ApcAP8959EU3"]["Password"].ToString();
        }

        public ApcAP8959EU3 CreateDevice()
        {
            return ApcAP8959EU3.Create(_host, _port, _username, _password);
        }

        [TestMethod]
        public void GivenDevice_WhenCallAvailable_ThenDeviceIsAvailable()
        {
            using (var device = CreateDevice())
            {
                Assert.IsTrue(device.GetAvailable());
            }
        }

        [TestMethod]
        public void GivenDevice_WhenGetOutlets_ThenOutletCountIs24()
        {
            using (var device = CreateDevice())
            {
                var outlets = device.GetOutlets();
                Assert.IsTrue(outlets.Count() == 24);
            }
        }
    }
}
