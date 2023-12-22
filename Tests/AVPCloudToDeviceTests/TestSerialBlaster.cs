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

        [Test]
        public void GivenRawHexCode_WhenConvertToNecHexCode_ThenResultIsCorrect()
        {
            string rawHex = "0000 006E 0022 0002 0155 00AB 0015 0015 0015 0015 0015 0015 0015 0015 0015 0015 0015 0015 0015 0015 0015 0015 0015 0040 0015 0040 0015 0040 0015 0040 0015 0040 0015 0040 0015 0040 0015 0040 0015 0040 0015 0015 0015 0015 0015 0015 0015 0015 0015 0015 0015 0015 0015 0040 0015 0015 0015 0040 0015 0040 0015 0040 0015 0040 0015 0040 0015 0040 0015 0015 0015 05EB 0155 0055 0015 0E42";
            uint command = SerialBlaster.ConvertRawHexToNecHex(rawHex);

            Assert.That(command, Is.EqualTo(0x00FF817E));
        }

        [Test]
        public void GivenRawHexCodeIsNullString_WhenConvertToNecHexCode_ThenResultIs0()
        {
            uint command = SerialBlaster.ConvertRawHexToNecHex(null);

            Assert.That(command, Is.EqualTo(0));
        }

        [Test]
        public void GivenRawHexCodeIsGarbage_WhenConvertToNecHexCode_ThenResultIs0()
        {
            string rawHex = "qwEHVb rTK19qW6 uhnxlGj zWFQZ4Fib0Cd8Meb H1qK oIZzwHCl09 Zwp0cM31LFi pdI6ehtlCk ae29ypFAkv3RRndip8g3h 3SvWWcTSOohy JkAAWQ3 LbDUAjehMw 7tE3hu";
            uint command = SerialBlaster.ConvertRawHexToNecHex(rawHex);

            Assert.That(command, Is.EqualTo(0));
        }

        [Test]
        public void GivenRawHexCodeIsEmpty_WhenConvertToNecHexCode_ThenResultIs0()
        {
            uint command = SerialBlaster.ConvertRawHexToNecHex(string.Empty);

            Assert.That(command, Is.EqualTo(0));
        }
    }
}