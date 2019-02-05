using Microsoft.VisualStudio.TestTools.UnitTesting;
using ControllableDevice;
using System.Linq;
using Newtonsoft.Json;
using System.IO;
using Newtonsoft.Json.Linq;
using System.Threading;
using System;
using Renci.SshNet.Common;
using System.Net.Sockets;

namespace Tests
{
    [TestClass]
    public class TestSshDevice
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


        public SshDevice CreateDevice()
        {
            return new SshDevice(_host, _port, _username, _password, ApcAP8959EU3.TerminalPrompt);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void GivenInvalidHost_WhenNewDevice_ThenExceptionThrown()
        {
            var device = new SshDevice("0.0.0.0", _port, _username, _password, ApcAP8959EU3.TerminalPrompt);
        }

        [TestMethod]
        [ExpectedException(typeof(SocketException))]
        public void GivenInvalidPort_WhenNewDevice_ThenExceptionThrown()
        {
            var device = new SshDevice(_host, 0, _username, _password, ApcAP8959EU3.TerminalPrompt);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void GivenInvalidUsername_WhenNewDevice_ThenExceptionThrown()
        {
            var device = new SshDevice(_host, _port, "", _password, ApcAP8959EU3.TerminalPrompt);
        }

        [TestMethod]
        [ExpectedException(typeof(SshConnectionException))]
        public void GivenInvalidPassword_WhenNewDevice_ThenExceptionThrown()
        {
            var device = new SshDevice(_host, _port, _username, "", ApcAP8959EU3.TerminalPrompt);
        }
    }
}
