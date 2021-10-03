using AVPCloudToDevice;
using Microsoft.Azure.Devices;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using NUnit.Framework;
using System.Dynamic;
using System.IO;
using ControllableDeviceTypes.AtenVS0801HBTypes;
using System.Collections.Generic;

namespace Tests
{
    public class TestAtenVS0801HB
    {
        private dynamic _settings;
        private const string _settingsFile = "settings.json";

        private ServiceClient _serviceClient;
        private List<AtenVS0801HB> _devices = new List<AtenVS0801HB>();

        private uint _invalidDeviceIndex = 999;
        private int _invalidInputPort = 999;

        public TestAtenVS0801HB()
        {
            using (StreamReader r = new StreamReader(_settingsFile))
            {
                string json = r.ReadToEnd();
                dynamic parsed = JsonConvert.DeserializeObject<ExpandoObject>(json, new ExpandoObjectConverter());
                _settings = parsed.AtenVS0801HB;
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
                var device = new AtenVS0801HB(_serviceClient, _settings.DeviceId, i);
                _devices.Add(device);
            }
        }

        [Test]
        public void GivenInputPortIsPort1_WhenSetInputPort2_ThenInputPortIsPort2()
        {
            foreach (var device in _devices)
            {
                Assert.IsTrue(device.SetInputPort(InputPort.Port1));
                var state = device.GetState();
                Assert.IsTrue(state != null);
                Assert.IsTrue(state.InputPort == InputPort.Port1);

                Assert.IsTrue(device.SetInputPort(InputPort.Port2));
                state = device.GetState();
                Assert.IsTrue(state != null);
                Assert.IsTrue(state.InputPort == InputPort.Port2);
            }
        }

        [Test]
        public void GivenDevice_WhenCallAvailable_ThenDeviceIsAvailable()
        {
            foreach (var device in _devices)
            {
                Assert.IsTrue(device.GetAvailable());
            }
        }

        [Test]
        public void GivenDeviceWithInvalidDeviceIndex_WhenGetState_ThenStateIsNull()
        {
            var device = new AtenVS0801HB(_serviceClient, _settings.DeviceId, _invalidDeviceIndex);
            var state = device.GetState();
            Assert.IsNull(state);
        }

        [Test]
        public void GivenDeviceWithInvalidDeviceIndex_WhenGetAvailable_ThenAvailableIsFalse()
        {
            var device = new AtenVS0801HB(_serviceClient, _settings.DeviceId, _invalidDeviceIndex);
            var available = device.GetAvailable();
            Assert.IsFalse(available);
        }

        [Test]
        public void GivenDeviceWithInvalidDeviceIndex_WhenSetInput_ThenSuccessIsFalse()
        {
            var device = new AtenVS0801HB(_serviceClient, _settings.DeviceId, _invalidDeviceIndex);
            var success = device.SetInputPort(InputPort.Port1);
            Assert.IsFalse(success);
        }

        [Test]
        public void GivenDeviceWithInvalidDeviceIndex_WhenGoToNextInput_ThenSuccessIsFalse()
        {
            var device = new AtenVS0801HB(_serviceClient, _settings.DeviceId, _invalidDeviceIndex);
            var success = device.GoToNextInput();
            Assert.IsFalse(success);
        }

        [Test]
        public void GivenDeviceWithInvalidDeviceIndex_WhenGoToPreviousInput_ThenSuccessIsFalse()
        {
            var device = new AtenVS0801HB(_serviceClient, _settings.DeviceId, _invalidDeviceIndex);
            var success = device.GoToPreviousInput();
            Assert.IsFalse(success);
        }

        [Test]
        public void GivenDevice_WhenSetInvalidInput_ThenSuccessIsFalse()
        {
            foreach (var device in _devices)
            {
                var success = device.SetInputPort((InputPort)_invalidInputPort);
                Assert.IsFalse(success);
            }
        }

        [Test]
        public void GivenInputPortIsPort1_WhenGoToNextInput_ThenInputPortIsPort2()
        {
            foreach (var device in _devices)
            {
                Assert.IsTrue(device.SetInputPort(InputPort.Port1));
                Assert.IsTrue(device.GoToNextInput());

                var state = device.GetState();
                Assert.IsTrue(state != null);
                Assert.IsTrue(state.InputPort == InputPort.Port2);
            }
        }

        [Test]
        public void GivenInputPortIsPort2_WhenGoToPreviousInput_ThenInputPortIsPort1()
        {
            foreach (var device in _devices)
            {
                Assert.IsTrue(device.SetInputPort(InputPort.Port2));
                Assert.IsTrue(device.GoToPreviousInput());

                var state = device.GetState();
                Assert.IsTrue(state != null);
                Assert.IsTrue(state.InputPort == InputPort.Port1);
            }
        }
    }
}