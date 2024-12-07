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
using System.Numerics;

namespace Tests
{
    internal sealed class TestExtronDSC301HD
    {
        private readonly dynamic _settings;
        private const string _settingsFile = "settings.json";

        private ServiceClient _serviceClient;
        private ExtronDSC301HD _device;

        private readonly int _invalidScaleType = 999;
        private readonly int _invalidPositionType = 999;

        public TestExtronDSC301HD()
        {
            using StreamReader r = new(_settingsFile);
            string json = r.ReadToEnd();
            dynamic parsed = JsonConvert.DeserializeObject<ExpandoObject>(json, new ExpandoObjectConverter());
            _settings = parsed.ExtronDSC301HD;
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
            Assert.That(firmware >= new Version(1, 25, 1), Is.True);
        }

        [Test]
        public void GivenDevice_WhenCallAvailable_ThenDeviceIsAvailable()
        {
            Assert.That(_device.GetAvailable(), Is.True);
        }

        [Test]
        public void GivenDevice_WhenSetOutputRate_ThenResultIsTrue()
        {
            Assert.That(_device.SetOutputRate(Edid.GetEdid(1280, 720, 50.0f)), Is.True);
        }

        [Test]
        public void GivenDevice_WhenSetOutputRateWithNullEdid_ThenArgumentNullExceptionThrown()
        {
            Assert.Throws<ArgumentNullException>(() => _device.SetOutputRate(null));
        }

        [Test]
        public void GivenDevice_WhenSetScale_ThenSuccessIsTrue()
        {
            Assert.That(_device.SetScale(ScaleType.Fit, PositionType.Centre, AspectRatio.RatioPreserve, new Vector2(42,7)), Is.True);
        }

        [Test]
        public void GivenDevice_WhenSetScaleWithInvalidPositionType_ThenSuccessIsFalse()
        {
            Assert.That(_device.SetScale(ScaleType.Fit, (PositionType)_invalidPositionType, AspectRatio.RatioPreserve, new Vector2(0)), Is.False);
        }

        [Test]
        public void GivenDevice_WhenSetScaleWithInvalidScaleType_ThenSuccessIsFalse()
        {
            Assert.That(_device.SetScale((ScaleType)_invalidScaleType, PositionType.Centre, AspectRatio.RatioPreserve, new Vector2(0)), Is.False);
        }

        [Test]
        public void GivenDevice_WhenSetInputPortToHDMI_ThenInputPortIsHDMI()
        {
            Assert.That(_device.SetInputPort(InputPort.HDMI), Is.True);

            var inputPort = _device.GetInputPort();
            Assert.That(inputPort, Is.Not.Null);
            Assert.That(inputPort, Is.EqualTo(InputPort.HDMI));
        }

        [Test]
        public void GivenDevice_WhenGetTemperature_ThenResultIsNotNull()
        {
            var temperature = _device.GetTemperature();
            Assert.That(temperature, Is.Not.Null);
        }

        [Test]
        public void GivenDevice_WhenSetDetailFilterTo32_ThenDetailFilterIs32()
        {
            bool success = _device.SetDetailFilter(32);
            Assert.That(success, Is.True);

            var value = _device.GetDetailFilter();
            Assert.That(value, Is.EqualTo(32));

            _device.SetDetailFilter(64);
        }

        [Test]
        public void GivenDevice_WhenSetDetailFilterToInvalidValue_ThenResultIsFalse()
        {
            bool success = _device.SetDetailFilter(-1);
            Assert.That(success, Is.False);
        }

        [Test]
        public void GivenDevice_WhenSetBrightnessTo32_ThenBrightnessIs32()
        {
            bool success = _device.SetBrightness(32);
            Assert.That(success, Is.True);

            var value = _device.GetBrightness();
            Assert.That(value, Is.EqualTo(32));

            _device.SetBrightness(64);
        }

        [Test]
        public void GivenDevice_WhenSetBrightnessToInvalidValue_ThenResultIsFalse()
        {
            bool success = _device.SetBrightness(-1);
            Assert.That(success, Is.False);
        }

        [Test]
        public void GivenDevice_WhenSetContrastTo32_ThenContrastIs32()
        {
            bool success = _device.SetContrast(32);
            Assert.That(success, Is.True);

            var value = _device.GetContrast();
            Assert.That(value, Is.EqualTo(32));

            _device.SetContrast(64);
        }

        [Test]
        public void GivenDevice_WhenSetContrastToInvalidValue_ThenResultIsFalse()
        {
            bool success = _device.SetContrast(-1);
            Assert.That(success, Is.False);
        }

        [Test]
        public void GivenDeviceAndSetFreezeFalse_WhenSetFreezeTrue_ThenFreezeIsTrue()
        {
            //Given
            bool success = _device.SetFreeze(false);
            Assert.That(success, Is.True);

            var result = _device.GetFreeze();
            Assert.That((bool)result, Is.False);

            //When
            success = _device.SetFreeze(true);
            Assert.That(success, Is.True);

            //Then
            result = _device.GetFreeze();
            Assert.That((bool)result, Is.True);

            //Default
            success = _device.SetFreeze(false);
            Assert.That(success, Is.True);
        }
    }
}
