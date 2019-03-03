using AVPCloudToDevice;
using ControllableDeviceTypes.ExtronDSC301HDTypes;
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
    class TestExtronDSC301HD
    {
        private dynamic _settings;
        private readonly string _settingsFile = "settings.json";

        private ServiceClient _serviceClient;
        private ExtronDSC301HD _device;

        private int _invalidScaleType = 999;
        private int _invalidPositionType = 999;

        public TestExtronDSC301HD()
        {
            using (StreamReader r = new StreamReader(_settingsFile))
            {
                string json = r.ReadToEnd();
                dynamic parsed = JsonConvert.DeserializeObject<ExpandoObject>(json, new ExpandoObjectConverter());
                _settings = parsed.ExtronDSC301HD;
            }
        }

        [SetUp]
        public void Setup()
        {
            _serviceClient = ServiceClient.CreateFromConnectionString(_settings.ConnectionString);
            _device = new ExtronDSC301HD(_serviceClient, _settings.DeviceId);
        }

        [Test]
        public void GivenDevice_WhenGetFirmware_ThenFirmwareIsGTE1_25_1()
        {
            var firmware = _device.GetFirmware();
            if (firmware == null) Assert.Fail();
            Assert.IsTrue(firmware >= new Version(1, 25, 1));
        }

        [Test]
        public void GivenDevice_WhenCallAvailable_ThenDeviceIsAvailable()
        {
            Assert.IsTrue(_device.GetAvailable());
        }

        [Test]
        public void GivenDevice_WhenSetOutputRate_ThenResultIsTrue()
        {
            Assert.IsTrue(_device.SetOutputRate(Edid.GetEdid(1280, 720, 50.0f)));
        }

        [Test]
        public void GivenDevice_WhenSetScale_ThenSuccessIsTrue()
        {
            Assert.IsTrue(_device.SetScale(ScaleType.Fit, PositionType.Centre));
        }

        [Test]
        public void GivenDevice_WhenSetScaleWithInvalidPositionType_ThenSuccessIsFalse()
        {
            Assert.IsFalse(_device.SetScale(ScaleType.Fit, (PositionType)_invalidPositionType));
        }

        [Test]
        public void GivenDevice_WhenSetScaleWithInvalidScaleType_ThenSuccessIsFalse()
        {
            Assert.IsFalse(_device.SetScale((ScaleType)_invalidScaleType, PositionType.Centre));
        }
    }
}
