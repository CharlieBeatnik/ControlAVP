using AVPCloudToDevice;
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
    class TestSonyKDL60W855
    {
        private dynamic _settings;
        private readonly string _settingsFile = "settings.json";

        private ServiceClient _serviceClient;
        private SonyKDL60W855 _device;

        public TestSonyKDL60W855()
        {
            using (StreamReader r = new StreamReader(_settingsFile))
            {
                string json = r.ReadToEnd();
                dynamic parsed = JsonConvert.DeserializeObject<ExpandoObject>(json, new ExpandoObjectConverter());
                _settings = parsed.SonyKDL60W855;
            }
        }

        [OneTimeSetUp]
        public void OneTimeSetup()
        {
        }

        [SetUp]
        public void Setup()
        {
            _serviceClient = ServiceClient.CreateFromConnectionString(_settings.ConnectionString);
            _device = new SonyKDL60W855(_serviceClient, _settings.DeviceId);
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
            Assert.IsTrue(_device.TurnOff());
            Assert.IsTrue(_device.TurnOn());
        }
    }
}
