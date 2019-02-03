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
        private static ApcAP8959EU3 _device = null;
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

        [ClassCleanup]
        public static void ClassCleanup()
        {
        }

        [TestInitialize]
        public void TestInitialize()
        {
            _device = new ApcAP8959EU3(_host, _port, _username, _password);
        }

        [TestCleanup]
        public void TestCleanup()
        {
            _device.Dispose();
            _device = null;
        }

        [TestMethod]
        public void GivenDevice_WhenGetOutlets_ThenOutletCountIs24()
        {
            var outlets = _device.GetOutlets();
            Assert.IsTrue(outlets.Count() == 24);
        }

        [TestMethod]
        public void GivenInvalidHost_WhenSshClientConnects_ResultIsFalse()
        {
            var sshClient = new SshDevice();
            var result = sshClient.Connect("0.0.0.0", _port, _username, _password, ApcAP8959EU3.TerminalPrompt);
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void GivenInvalidPort_WhenSshClientConnects_ResultIsFalse()
        {
            var sshClient = new SshDevice();
            var result = sshClient.Connect(_host, 0, _username, _password, ApcAP8959EU3.TerminalPrompt);
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void GivenInvalidUsername_WhenSshClientConnects_ResultIsFalse()
        {
            var sshClient = new SshDevice();
            var result = sshClient.Connect(_host, _port, "", _password, ApcAP8959EU3.TerminalPrompt);
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void GivenInvalidPassword_WhenSshClientConnects_ResultIsFalse()
        {
            var sshClient = new SshDevice();
            var result = sshClient.Connect(_host, _port, _username, "", ApcAP8959EU3.TerminalPrompt);
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void GivenDevice_WhenCallAvailable_ThenDeviceIsAvailable()
        {
            Assert.IsTrue(_device.GetAvailable());
        }
    }
}
