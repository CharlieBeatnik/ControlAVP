using Microsoft.VisualStudio.TestTools.UnitTesting;
using PduDevice;
using System.Linq;
using Newtonsoft.Json;
using System.IO;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Converters;
using System.Dynamic;

namespace Tests
{
    [TestClass]
    public class TestApcAP8959EU3
    {
        private dynamic _settings;
        private readonly string _settingsFile = "settings.json";

        public TestApcAP8959EU3()
        {
            using (StreamReader r = new StreamReader(_settingsFile))
            {
                string json = r.ReadToEnd();
                dynamic parsed = JsonConvert.DeserializeObject<ExpandoObject>(json, new ExpandoObjectConverter());
                _settings = parsed.ApcAP8959EU3;
            }
        }

        private ApcAP8959EU3 GetDevice()
        {
            return new ApcAP8959EU3(_settings.Host, int.Parse(_settings.Port), _settings.Username, _settings.Password);
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
            var result = sshClient.Connect("0.0.0.0", int.Parse(_settings.Port), _settings.Username, _settings.Password, ApcAP8959EU3.TerminalPrompt);
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void GivenInvalidPort_WhenSshClientConnects_ResultIsFalse()
        {
            var sshClient = new PduSshClient();
            var result = sshClient.Connect(_settings.Host, 0, _settings.Username, _settings.Password, ApcAP8959EU3.TerminalPrompt);
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void GivenInvalidUsername_WhenSshClientConnects_ResultIsFalse()
        {
            var sshClient = new PduSshClient();
            var result = sshClient.Connect(_settings.Host, int.Parse(_settings.Port), "", _settings.Password, ApcAP8959EU3.TerminalPrompt);
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void GivenInvalidPassword_WhenSshClientConnects_ResultIsFalse()
        {
            var sshClient = new PduSshClient();
            var result = sshClient.Connect(_settings.Host, int.Parse(_settings.Port), _settings.Username, "", ApcAP8959EU3.TerminalPrompt);
            Assert.IsFalse(result);
        }
    }
}
