﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
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
        private const string _settingsFile = "settings.json";

        private static string _host;
        private static int _port;
        private static string _username;
        private static string _password;

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

            _host = jsonParsed["Devices"]["ApcAP8959EU3"][0]["host"].ToString();
            _port = int.Parse(jsonParsed["Devices"]["ApcAP8959EU3"][0]["port"].ToString());
            _username = jsonParsed["Devices"]["ApcAP8959EU3"][0]["username"].ToString();
            _password = jsonParsed["Devices"]["ApcAP8959EU3"][0]["password"].ToString();
        }


        public static SshDevice CreateDevice()
        {
            return new SshDevice(_host, _port, _username, _password, ApcAP8959EU3.TerminalPrompt);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void GivenZeroHostAddress_WhenNewDevice_ThenExceptionThrown()
        {
            //Returns ArgumentException if IP address is 0
            //IPv4 address 0.0.0.0 and IPv6 address ::0 are unspecified addresses that cannot be used as a target address.
            using (var device = new SshDevice("0.0.0.0", _port, _username, _password, ApcAP8959EU3.TerminalPrompt))
            {
            }
        }

        [TestMethod]
        [ExpectedException(typeof(SocketException))]
        public void GivenInvalidHost_WhenNewDevice_ThenExceptionThrown()
        {
            using (var device = new SshDevice("203.0.113.0", _port, _username, _password, ApcAP8959EU3.TerminalPrompt))
            {
            }
        }

        [TestMethod]
        [ExpectedException(typeof(SocketException))]
        public void GivenInvalidPort_WhenNewDevice_ThenExceptionThrown()
        {
            using (var device = new SshDevice(_host, 0, _username, _password, ApcAP8959EU3.TerminalPrompt))
            {
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void GivenInvalidUsername_WhenNewDevice_ThenExceptionThrown()
        {
            using (var device = new SshDevice(_host, _port, "", _password, ApcAP8959EU3.TerminalPrompt))
            {
            }
        }

        [TestMethod]
        [ExpectedException(typeof(SshConnectionException))]
        public void GivenInvalidPassword_WhenNewDevice_ThenExceptionThrown()
        {
            using (var device = new SshDevice(_host, _port, _username, "", ApcAP8959EU3.TerminalPrompt))
            {
            }
        }
    }
}
