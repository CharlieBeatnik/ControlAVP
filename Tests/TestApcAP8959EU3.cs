using Microsoft.VisualStudio.TestTools.UnitTesting;
using PduDevice;
using System.Linq;
using Newtonsoft.Json;
using System.IO;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace Tests
{
    [TestClass]
    public class TestApcAP8959EU3
    {
        private class Settings
        {
            public string Host { get; set; }
            public int Port { get; set; }
            public string Username { get; set; }
            public string Password { get; set; }
        }

        private Settings _settings;
        private string _settingsFile = "settings.json";
        private string _settingsSectionName = "ApcAP8959EU3";

        public TestApcAP8959EU3()
        {
            using (StreamReader r = new StreamReader(_settingsFile))
            {
                string json = r.ReadToEnd();
                JObject parsed = JObject.Parse(json);
                _settings = JsonConvert.DeserializeObject<Settings>(parsed[_settingsSectionName].ToString());
            }
        }

        private ApcAP8959EU3 GetDevice()
        {
            var sshClient = new PduSshClient();
            sshClient.Connect(_settings.Host, _settings.Port, _settings.Username, _settings.Password, ApcAP8959EU3.TerminalPrompt);
            return new ApcAP8959EU3(sshClient);
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
            var result = sshClient.Connect("0.0.0.0", _settings.Port, _settings.Username, _settings.Password, ApcAP8959EU3.TerminalPrompt);
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
            var result = sshClient.Connect(_settings.Host, _settings.Port, "", _settings.Password, ApcAP8959EU3.TerminalPrompt);
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void GivenInvalidPassword_WhenSshClientConnects_ResultIsFalse()
        {
            var sshClient = new PduSshClient();
            var result = sshClient.Connect(_settings.Host, _settings.Port, _settings.Username, "", ApcAP8959EU3.TerminalPrompt);
            Assert.IsFalse(result);
        }
    }
}
