﻿using AVPCloudToDevice;
using Microsoft.Azure.Devices;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using NUnit.Framework;
using System;
using System.Dynamic;
using System.IO;
using System.Linq;

namespace Tests
{
    internal sealed class TestSonySimpleIP
    {
        private readonly dynamic _settings;
        private const string _settingsFile = "settings.json";

        private ServiceClient _serviceClient;
        private SonySimpleIP _device;

        public TestSonySimpleIP()
        {
            using StreamReader r = new(_settingsFile);
            string json = r.ReadToEnd();
            dynamic parsed = JsonConvert.DeserializeObject<ExpandoObject>(json, new ExpandoObjectConverter());
            _settings = parsed.SonySimpleIP;
        }

        [OneTimeSetUp]
        public void OneTimeSetup()
        {
        }

        [SetUp]
        public void Setup()
        {
            _serviceClient = ServiceClient.CreateFromConnectionString(_settings.ConnectionString);
            _device = new SonySimpleIP(_serviceClient, _settings.DeviceId);
            _device.TurnOn();
        }

        [TearDown]
        public static void TearDown()
        {
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            _device.TurnOff();
        }

        [Test]
        public void GivenTVIsOff_WhenTurnOn_ThenTVIsOn()
        {
            Assert.That(_device.TurnOff(), Is.True);
            Assert.That(_device.TurnOn(), Is.True);
        }
    }
}
