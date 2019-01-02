using Microsoft.VisualStudio.TestTools.UnitTesting;
using ControllableDevice;
using System.Linq;
using Newtonsoft.Json;
using System.IO;
using Newtonsoft.Json.Linq;

namespace Tests
{
    [TestClass]
    public class TestApcAP8959EU3
    {
        private readonly string _settingsFile = "settings.json";

        private string _host;
        private int _port;
        private string _username;
        private string _password;

        public TestApcAP8959EU3()
        {
            dynamic settings;

            using (StreamReader r = new StreamReader(_settingsFile))
            {
                string json = r.ReadToEnd();
                settings = JObject.Parse(json);
            }

            _host = (string)settings.SelectToken("ApcAP8959EU3.Host");
            _port = int.Parse((string)settings.SelectToken("ApcAP8959EU3.Port"));
            _username = (string)settings.SelectToken("ApcAP8959EU3.Username");
            _password = (string)settings.SelectToken("ApcAP8959EU3.Password");
        }

        private ApcAP8959EU3 GetDevice()
        {
            return new ApcAP8959EU3(_host, _port, _username, _password);
        }

        [TestMethod]
        public void GivenDevice_WhenGetOutlets_ThenOutletCountIs24()
        {
            var device = GetDevice();
            var outlets = device.GetOutlets();
            Assert.IsTrue(outlets.Count() == 24);
        }

        [TestMethod]
        public void GivenInvalidHost_WhenSshClientConnects_ResultIsFalse()
        {
            var sshClient = new PduSshClient();
            var result = sshClient.Connect("0.0.0.0", _port, _username, _password, ApcAP8959EU3.TerminalPrompt);
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void GivenInvalidPort_WhenSshClientConnects_ResultIsFalse()
        {
            var sshClient = new PduSshClient();
            var result = sshClient.Connect(_host, 0, _username, _password, ApcAP8959EU3.TerminalPrompt);
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void GivenInvalidUsername_WhenSshClientConnects_ResultIsFalse()
        {
            var sshClient = new PduSshClient();
            var result = sshClient.Connect(_host, _port, "", _password, ApcAP8959EU3.TerminalPrompt);
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void GivenInvalidPassword_WhenSshClientConnects_ResultIsFalse()
        {
            var sshClient = new PduSshClient();
            var result = sshClient.Connect(_host, _port, _username, "", ApcAP8959EU3.TerminalPrompt);
            Assert.IsFalse(result);
        }
    }
}
