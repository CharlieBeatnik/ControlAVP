using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ControllableDevice;
using System.IO;
using Newtonsoft.Json.Linq;
using System.Threading;

namespace Tests
{
    [TestClass]
    public class TestRs232Device
    {
        private readonly string _settingsFile = "settings.json";
        private JObject _settings;

        public TestRs232Device()
        {
            using (StreamReader r = new StreamReader(_settingsFile))
            {
                string json = r.ReadToEnd();
                _settings = JObject.Parse(json);
            }
        }

        private Rs232Device GetDevice()
        {
            var device = new Rs232Device(_settings["AtenVS0801H"][0]["PortId"].ToString());
            device.BaudRate = 19200;
            device.PreWrite = (x) =>
            {
                return x + "\r";
            };

            return device;
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void GivenEmptyPartialId_WhenNewRs232Device_ThenExceptionThrown()
        {
            var rs232Device = new Rs232Device(string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void GivenNullPartialId_WhenNewRs232Device_ThenExceptionThrown()
        {
            var rs232Device = new Rs232Device(null);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void GivenInvalidPartialId_WhenNewRs232Device_ThenExceptionThrown()
        {
            var rs232Device = new Rs232Device("invalid");
        }


        [TestMethod]
        public void GivenClass_WhenDisposed_ThenClassCanBeRecreated()
        {
            for(int i = 0; i < 2; ++i)
            {
                using (var device = GetDevice())
                {
                }
            }
        }

        [TestMethod]
        public void GivenDevice_WhenWrite_ThenNoExceptionThrown()
        {
            using (var device = GetDevice())
            {
                device.Write("invalid");
            }
        }

        [TestMethod]
        public void GivenDevice_WhenWriteWithResponse_ThenResponseIsNotNullOrEmpty()
        {
            using (var device = GetDevice())
            {
                var result = device.WriteWithResponse("invalid", ".*");
                Assert.IsFalse(string.IsNullOrEmpty(result));
            }
        }

        [TestMethod]
        public void GivenDevice_WhenWriteWithResponse_ThenResponseIsExactlyAsExpected()
        {
            using (var device = GetDevice())
            {
                var result = device.WriteWithResponse("read", @"^F/W: V[0-9]+.[0-9]+.[0-9]+$");
                Assert.AreEqual(result, "F/W: V2.0.197");
            }
        }

        [TestMethod]
        public void GivenDevice_WhenWriteThenRead_ThenReadIsExactlyAsExpected()
        {
            using (var device = GetDevice())
            {
                device.Write("read");
                Thread.Sleep(device.PostWriteWait);
                var result = device.Read(@"^F/W: V[0-9]+.[0-9]+.[0-9]+$");
                Assert.AreEqual(result, "F/W: V2.0.197");
            }
        }

        [TestMethod]
        public void GivenDevice_WhenWriteWithResponses_ThenResponsesAreAsExpected()
        {
            using (var device = GetDevice())
            {
                var result = device.WriteWithResponses("read", 6);
                Assert.AreEqual(result.Count, 6);
                Assert.AreEqual(result[5], "F/W: V2.0.197");
            }
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public void GivenDevice_WhenCreateAnotherDeviceWithSameID_ThenExceptionThrown()
        {
            using (var device1 = GetDevice())
            {
                using (var device2 = GetDevice())
                {
                }
            }
        }
    }
}
