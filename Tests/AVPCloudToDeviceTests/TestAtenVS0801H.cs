using AVPCloudToDevice;
using Microsoft.Azure.Devices;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using NUnit.Framework;
using System.Dynamic;
using System.IO;
using ControllableDeviceTypes.AtenVS0801HTypes;
using System.Collections.Generic;

namespace Tests
{
    internal sealed class TestAtenVS0801H
    {
        private readonly dynamic _settings;
        private const string _settingsFile = "settings.json";

        private ServiceClient _serviceClient;
        private readonly List<AtenVS0801H> _devices = [];

        private readonly uint _invalidDeviceIndex = 999;
        private readonly int _invalidInputPort = 999;

        public TestAtenVS0801H()
        {
            using StreamReader r = new(_settingsFile);
            string json = r.ReadToEnd();
            dynamic parsed = JsonConvert.DeserializeObject<ExpandoObject>(json, new ExpandoObjectConverter());
            _settings = parsed.AtenVS0801H;
        }

        [SetUp]
        public void Setup()
        {
            _devices.Clear();

            _serviceClient = ServiceClient.CreateFromConnectionString(_settings.ConnectionString);

            uint deviceCount = (uint)_settings.DeviceCount;
            for (uint i = 0; i < deviceCount; i++)
            {
                var device = new AtenVS0801H(_serviceClient, _settings.DeviceId, i);
                _devices.Add(device);
            }
        }

        [Test]
        public void GivenInputPortIsPort1_WhenGoToNextInput_ThenInputPortIsPort2()
        {
            foreach (var device in _devices)
            {
                Assert.That(device.SetInputPort(InputPort.Port1), Is.True);
                Assert.That(device.GoToNextInput(), Is.True);

                var state = device.GetState();
                Assert.That(state, Is.Not.EqualTo(null));
                Assert.That(state.InputPort, Is.EqualTo(InputPort.Port2));
            }
        }

        [Test]
        public void GivenInputPortIsPort2_WhenGoToPreviousInput_ThenInputPortIsPort1()
        {
            foreach (var device in _devices)
            {
                Assert.That(device.SetInputPort(InputPort.Port2), Is.True);
                Assert.That(device.GoToPreviousInput(), Is.True);

                var state = device.GetState();
                Assert.That(state, Is.Not.EqualTo(null));
                Assert.That(state.InputPort, Is.EqualTo(InputPort.Port1));
            }
        }

        [Test]
        public void GivenInputPortIsPort1_WhenSetInputPort2_ThenInputPortIsPort2()
        {
            foreach (var device in _devices)
            {
                Assert.That(device.SetInputPort(InputPort.Port1), Is.True);
                var state = device.GetState();
                Assert.That(state, Is.Not.EqualTo(null));
                Assert.That(state.InputPort, Is.EqualTo(InputPort.Port1));

                Assert.That(device.SetInputPort(InputPort.Port2), Is.True);
                state = device.GetState();
                Assert.That(state, Is.Not.EqualTo(null));
                Assert.That(state.InputPort, Is.EqualTo(InputPort.Port2));
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
        public void GivenDeviceWithInvalidDeviceIndex_WhenGetState_ThenStateIsNull()
        {
            var device = new AtenVS0801H(_serviceClient, _settings.DeviceId, _invalidDeviceIndex);
            var state = device.GetState();
            Assert.That(state, Is.Null);
        }

        [Test]
        public void GivenDeviceWithInvalidDeviceIndex_WhenGetAvailable_ThenAvailableIsFalse()
        {
            var device = new AtenVS0801H(_serviceClient, _settings.DeviceId, _invalidDeviceIndex);
            var available = device.GetAvailable();
            Assert.That(available, Is.False);
        }

        [Test]
        public void GivenDeviceWithInvalidDeviceIndex_WhenGoToNextInput_ThenSuccessIsFalse()
        {
            var device = new AtenVS0801H(_serviceClient, _settings.DeviceId, _invalidDeviceIndex);
            var success = device.GoToNextInput();
            Assert.That(success, Is.False);
        }

        [Test]
        public void GivenDeviceWithInvalidDeviceIndex_WhenGoToPreviousInput_ThenSuccessIsFalse()
        {
            var device = new AtenVS0801H(_serviceClient, _settings.DeviceId, _invalidDeviceIndex);
            var success = device.GoToPreviousInput();
            Assert.That(success, Is.False);
        }

        [Test]
        public void GivenDeviceWithInvalidDeviceIndex_WhenSetInput_ThenSuccessIsFalse()
        {
            var device = new AtenVS0801H(_serviceClient, _settings.DeviceId, _invalidDeviceIndex);
            var success = device.SetInputPort(InputPort.Port1);
            Assert.That(success, Is.False);
        }

        [Test]
        public void GivenDevice_WhenSetInvalidInput_ThenSuccessIsFalse()
        {
            foreach (var device in _devices)
            {
                var success = device.SetInputPort((InputPort)_invalidInputPort);
                Assert.That(success, Is.False);
            }
        }
   }
}