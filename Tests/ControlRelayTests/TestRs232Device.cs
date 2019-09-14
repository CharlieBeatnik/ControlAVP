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

        private Rs232Device CreateDevice(int messageStoreCapacity = 0)
        {
            var device = new Rs232Device(_settings["AtenVS0801H"][0]["PortId"].ToString(), messageStoreCapacity);
            device.BaudRate = 19200;
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
        [ExpectedException(typeof(InvalidOperationException))]
        public void GivenInvalidPartialId_WhenNewDevice_ThenInvalidOperationExceptionThrown()
        {
            var rs232Device = new Rs232Device("invalid");
        }


        [TestMethod]
        public void GivenClass_WhenDisposed_ThenClassCanBeRecreated()
        {
            for(int i = 0; i < 2; ++i)
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
                var result = device.WriteWithResponse("read", @"^F/W: V[0-9]+.[0-9]+.[0-9]+$");
                Assert.AreEqual(result, "F/W: V2.0.197");
            }
        }

        [TestMethod]
        public void GivenDevice_WhenWriteThenRead_ThenReadIsExactlyAsExpected()
        {
            using (var device = CreateDevice())
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
            using (var device = CreateDevice())
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
            using (var device1 = CreateDevice())
            {
                using (var device2 = CreateDevice())
                {
                }
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void GivenDevice_WhenRequestTooManyResponses_ThenArgumentExceptionIsThrown()
        {
            using (var device = CreateDevice())
            {
                var result = device.WriteWithResponses("read", 7);
            }
        }

        [TestMethod]
        public void GivenDevice_WhenWriteCommandAndWaitGreaterThanMessageExpiry_ThenReadIsNull()
        {
            using (var device = CreateDevice())
            {
                device.Write("read");
                Thread.Sleep(device.PostWriteWait);
                var result = device.Read(@"^F/W: V[0-9]+.[0-9]+.[0-9]+$");
                Assert.IsNotNull(result);

                Thread.Sleep(device.MessageLifetime + TimeSpan.FromSeconds(2));

                result = device.Read(@"^F/W: V[0-9]+.[0-9]+.[0-9]+$");
                Assert.IsNull(result);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(OutOfMemoryException))]
        public void GivenDevice_WhenCreateDeviceWithMessageStoreCapacityOfIntMax_ThenOutOfMemoryExceptionIsThrown()
        {
            using (var device = CreateDevice(int.MaxValue))
            {
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void GivenDevice_WhenCreateDeviceWithMessageStoreCapacityOfIntMin_ThenAgumentExceptionIsThrown()
        {
            using (var device = CreateDevice(int.MinValue))
            {
            }
        }
    }
}
