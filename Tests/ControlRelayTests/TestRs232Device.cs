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

        private Rs232Device CreateDevice()
        {
            var device = new Rs232Device(_settings["SerialBlaster"]["PortId"].ToString());
            device.BaudRate = 115200;
            device.PreWrite = (x) =>
            {
                return x + "\r";
            };

            return device;
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void GivenEmptyPartialId_WhenNewDevice_ThenArgumentExceptionThrown()
        {
            var rs232Device = new Rs232Device(string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void GivenNullPartialId_WhenNewDevice_ThenArgumentExceptionThrown()
        {
            var rs232Device = new Rs232Device(null);
        }

        [TestMethod]
        public void GivenDevice_WhenCreateDeviceCorrectPartialId_ThenDeviceIsEnabled()
        {
            using (var device = CreateDevice())
            {
                Assert.IsTrue(device.Enabled);
            }
        }

        [TestMethod]
        public void GivenDevice_WhenCreateDeviceWithUnknownPartialId_ThenDeviceIsNotEnabled()
        {
            using (var device = new Rs232Device("invalid"))
            {
                Assert.IsFalse(device.Enabled);
            }
        }

        [TestMethod]
        public void GivenClass_WhenDisposed_ThenClassCanBeRecreated()
        {
            for (int i = 0; i < 2; ++i)
            {
                using (var device = CreateDevice())
                {
                }
            }
        }

        [TestMethod]
        public void GivenDevice_WhenWrite_ThenNoExceptionThrown()
        {
            using (var device = CreateDevice())
            {
                device.Write("invalid");
            }
        }

        [TestMethod]
        public void GivenDevice_WhenWriteWithResponse_ThenResponseIsNotNullOrEmpty()
        {
            using (var device = CreateDevice())
            {
                var result = device.WriteWithResponse("invalid", ".*");
                Assert.IsFalse(string.IsNullOrEmpty(result));
            }
        }

        [TestMethod]
        public void GivenDevice_WhenWriteWithResponse_ThenResponseIsExactlyAsExpected()
        {
            using (var device = CreateDevice())
            {
                var result = device.WriteWithResponse($"send nec 0x3EC14DB2", "OK");
                Assert.AreEqual(result, "OK");
            }
        }

        [TestMethod]
        public void GivenDevice_WhenWriteThenRead_ThenReadIsExactlyAsExpected()
        {
            using (var device = CreateDevice())
            {
                device.Write("send nec 0x3EC14DB2");
                Thread.Sleep(device.PostWriteWait);
                var result = device.Read("OK");
                Assert.AreEqual(result, "OK");
            }
        }

        [TestMethod]
        public void GivenDevice_WhenWriteWithResponses_ThenResponsesAreAsExpected()
        {
            using (var device = CreateDevice())
            {
                var result = device.WriteWithResponses("send nec 0x3EC14DB2", 1);
                Assert.IsNotNull(result);
                Assert.AreEqual(result.Count, 1);
                Assert.AreEqual(result[0], "OK");
            }
        }

        [TestMethod]
        public void GivenDevice_WhenCreateAnotherDeviceWithSameID_ThenSecondDeviceIsNotEnabled()
        {
            using (var device1 = CreateDevice())
            {
                Assert.IsTrue(device1.Enabled);
                using (var device2 = CreateDevice())
                {
                    Assert.IsTrue(device1.Enabled);
                    Assert.IsFalse(device2.Enabled);
                }
                Assert.IsTrue(device1.Enabled);
            }
        }

        [TestMethod]
        public void GivenDevice_WhenCreateAnotherDeviceWithSameIDAfterDisposingFirstDevice0_ThenSecondDevicIsEnabled()
        {
            using (var device1 = CreateDevice())
            {
                Assert.IsTrue(device1.Enabled);
            }

            using (var device2 = CreateDevice())
            {
                Assert.IsTrue(device2.Enabled);
            }

        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void GivenDevice_WhenRequestTooManyResponses_ThenArgumentExceptionIsThrown()
        {
            using (var device = CreateDevice())
            {
                var result = device.WriteWithResponses("send nec 0x3EC14DB2", 2);
            }
        }

        [TestMethod]
        public void GivenDevice_WhenWriteCommandAndWaitGreaterThanMessageExpiry_ThenReadIsNull()
        {
            using (var device = CreateDevice())
            {
                device.Write("send nec 0x3EC14DB2");
                Thread.Sleep(device.PostWriteWait);
                var result = device.Read("OK");
                Assert.IsNotNull(result);

                Thread.Sleep(device.MessageLifetime + TimeSpan.FromSeconds(2));

                result = device.Read("OK");
                Assert.IsNull(result);
            }
        }

        [TestMethod]
        [Ignore("Requires manual intervention.")]
        public void GivenDevice_WhenPrintMessageAskingToBeDisconnectedAndWaitForReconnect_ThenResponseIsNotNully()
        {
            using (var device = CreateDevice())
            {
                var result = device.WriteWithResponse("message DisconnectNow", "OK");
                Assert.IsNotNull(result);

                Thread.Sleep(TimeSpan.FromSeconds(10));

                result = device.WriteWithResponse("message Reconnected", "OK");
                Assert.IsNotNull(result);
            }
        }
    }
}
