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
    internal sealed class TestSerialBlaster
    {
        private readonly dynamic _settings;
        private const string _settingsFile = "settings.json";

        private ServiceClient _serviceClient;
        private readonly List<SerialBlaster> _devices = [];

        private readonly uint _invalidDeviceIndex = 999;

        public TestSerialBlaster()
        {
            using StreamReader r = new(_settingsFile);
            string json = r.ReadToEnd();
            dynamic parsed = JsonConvert.DeserializeObject<ExpandoObject>(json, new ExpandoObjectConverter());
            _settings = parsed.SerialBlaster;
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
                Assert.That(device.GetAvailable(), Is.True);
            }
        }

        [Test]
        public void GivenDeviceWithInvalidDeviceIndex_WhenGetAvailable_ThenAvailableIsFalse()
        {
            var device = new SerialBlaster(_serviceClient, _settings.DeviceId, _invalidDeviceIndex);
            Assert.That(device.GetAvailable(), Is.False);
        }

        [Test]
        public void GivenDevice_WhenSendMessage_ThenResultIsTrue()
        {
            foreach (var device in _devices)
            {
                Assert.That(device.SendMessage("Test"), Is.True);
            }
        }

        [Test]
        public void GivenDevice_WhenSendNullMessage_ThenResultIsFalse()
        {
            foreach (var device in _devices)
            {
                Assert.That(device.SendMessage(null), Is.False);
            }
        }

        [Test]
        public void GivenDevice_WhenSendEmptyMessage_ThenResultIsFalse()
        {
            foreach (var device in _devices)
            {
                Assert.That(device.SendMessage(string.Empty), Is.False);
            }
        }

        [Test]
        public void GivenDeviceWithInvalidDeviceIndex_WhenSendMessage_ThenResultIsFalse()
        {
            var device = new SerialBlaster(_serviceClient, _settings.DeviceId, _invalidDeviceIndex);
            Assert.That(device.SendMessage("Test"), Is.False);
        }

        [Test]
        public void GivenDevice_WhenSendValidCommand_ThenResultIsTrue()
        {
            foreach (var device in _devices)
            {
                Assert.That(device.SendCommand(Protocol.Nec, 0x01FE817E, 0), Is.True);
            }
        }

        [Test]
        public void GivenInvalidDevice_WhenSendValidCommand_ThenResultIsTrue()
        {
            var device = new SerialBlaster(_serviceClient, _settings.DeviceId, _invalidDeviceIndex);
            Assert.That(device.SendCommand(Protocol.Nec, 0x01FE817E, 0), Is.False);
        }
    }
}