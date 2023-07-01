using AVPCloudToDevice;
using Microsoft.Azure.Devices;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using NUnit.Framework;
using System.Dynamic;
using System.IO;
using ControllableDeviceTypes.SerialBlasterTypes;
using System.Collections.Generic;
using NUnit.Framework.Internal;

namespace Tests
{
    public class TestSerialBlaster
    {
        private dynamic _settings;
        private const string _settingsFile = "settings.json";

        private ServiceClient _serviceClient;
        private List<SerialBlaster> _devices = new List<SerialBlaster>();

        private uint _invalidDeviceIndex = 999;

        public TestSerialBlaster()
        {
            using (StreamReader r = new StreamReader(_settingsFile))
            {
                string json = r.ReadToEnd();
                dynamic parsed = JsonConvert.DeserializeObject<ExpandoObject>(json, new ExpandoObjectConverter());
                _settings = parsed.SerialBlaster;
            }
        }

        [SetUp]
        public void Setup()
        {
            _devices.Clear();

            _serviceClient = ServiceClient.CreateFromConnectionString(_settings.ConnectionString);

            uint deviceCount = (uint)_settings.DeviceCount;
            for (uint i = 0; i < deviceCount; i++)
            {
                var device = new SerialBlaster(_serviceClient, _settings.DeviceId, i);
                _devices.Add(device);
            }
        }

        [Test]
        public void GivenDevice_WhenGetAvailable_ThenDeviceIsAvailable()
        {
            foreach (var device in _devices)
            {
                Assert.IsTrue(device.GetAvailable());
            }
        }

        [Test]
        public void GivenDeviceWithInvalidDeviceIndex_WhenGetAvailable_ThenAvailableIsFalse()
        {
            var device = new SerialBlaster(_serviceClient, _settings.DeviceId, _invalidDeviceIndex);
            Assert.IsFalse(device.GetAvailable());
        }

        [Test]
        public void GivenDevice_WhenSendMessage_ThenResultIsTrue()
        {
            foreach (var device in _devices)
            {
                Assert.IsTrue(device.SendMessage("Test"));
            }
        }

        [Test]
        public void GivenDevice_WhenSendNullMessage_ThenResultIsFalse()
        {
            foreach (var device in _devices)
            {
                Assert.IsFalse(device.SendMessage(null));
            }
        }

        [Test]
        public void GivenDevice_WhenSendEmptyMessage_ThenResultIsFalse()
        {
            foreach (var device in _devices)
            {
                Assert.IsFalse(device.SendMessage(string.Empty));
            }
        }

        [Test]
        public void GivenDeviceWithInvalidDeviceIndex_WhenSendMessage_ThenResultIsFalse()
        {
            var device = new SerialBlaster(_serviceClient, _settings.DeviceId, _invalidDeviceIndex);
            Assert.IsFalse(device.SendMessage("Test"));
        }

        [Test]
        public void GivenDevice_WhenSendValidCommand_ThenResultIsTrue()
        {
            foreach (var device in _devices)
            {
                Assert.IsTrue(device.SendCommand(Protocol.Nec, 0x01FE817E, 0));
            }
        }

        [Test]
        public void GivenInvalidDevice_WhenSendValidCommand_ThenResultIsTrue()
        {
            var device = new SerialBlaster(_serviceClient, _settings.DeviceId, _invalidDeviceIndex);
            Assert.IsFalse(device.SendCommand(Protocol.Nec, 0x01FE817E, 0));
        }
    }
}