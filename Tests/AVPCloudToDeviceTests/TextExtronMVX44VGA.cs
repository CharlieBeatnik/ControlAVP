﻿using AVPCloudToDevice;
using Microsoft.Azure.Devices;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using NUnit.Framework;
using System;
using System.Dynamic;
using System.IO;
using System.Linq;
using ControllableDeviceTypes.ExtronMVX44VGATypes;

namespace Tests
{
    public class TestExtronMVX44VGA
    {
        private dynamic _settings;
        private const string _settingsFile = "settings.json";

        private ServiceClient _serviceClient;
        private ExtronMVX44VGA _device;

        public TestExtronMVX44VGA()
        {
            using (StreamReader r = new StreamReader(_settingsFile))
            {
                string json = r.ReadToEnd();
                dynamic parsed = JsonConvert.DeserializeObject<ExpandoObject>(json, new ExpandoObjectConverter());
                _settings = parsed.ExtronMVX44VGA;
            }
        }

        [SetUp]
        public void Setup()
        {
            _serviceClient = ServiceClient.CreateFromConnectionString(_settings.ConnectionString);
            _device = new ExtronMVX44VGA(_serviceClient, _settings.DeviceId);
        }

        [Test]
        public void GivenDevice_WhenGetFirmware_ThenFirmwareIsGTE1_4()
        {
            var firmware = _device.GetFirmware();
            if (firmware == null) Assert.Fail();
            Assert.IsTrue(firmware >= new Version(1, 4));
        }

        [Test]
        public void GivenDevice_WhenCallAvailable_ThenDeviceIsAvailable()
        {
            Assert.IsTrue(_device.GetAvailable());
        }

        [Test]
        public void GivenDevice_WhenGetTieState_ThenTieStateIsNotNull()
        {
            Assert.IsNotNull(_device.GetTieState());
        }

        [Test]
        public void GivenDevice_WhenTieInputPortToAllOutputPorts_ThenSuccessIsTrue()
        {
            Assert.IsTrue(_device.TieInputPortToAllOutputPorts(InputPort.Port1, TieType.AudioVideo));
        }

        [Test]
        public void GivenDevice_WhenTieInputPortToOutputPort_ThenSuccessIsTrue()
        {
            Assert.IsTrue(_device.TieInputPortToOutputPort(InputPort.Port1, OutputPort.Port1, TieType.AudioVideo));
        }
    }
}
