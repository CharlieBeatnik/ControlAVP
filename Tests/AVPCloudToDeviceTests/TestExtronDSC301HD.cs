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
    public class TestExtronDSC301HD
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
        public void GivenDevice_WhenSetOutputRateWithNullEdid_ThenArgumentNullExceptionThrown()
        {
            Assert.Throws<ArgumentNullException>(() => _device.SetOutputRate(null));
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

        [Test]
        public void GivenDevice_WhenSetInputPortToHDMI_ThenInputPortIsHDMI()
        {
            Assert.IsTrue(_device.SetInputPort(InputPort.HDMI));

            var inputPort = _device.GetInputPort();
            Assert.IsNotNull(inputPort);
            Assert.AreEqual(InputPort.HDMI, inputPort);
        }

        [Test]
        public void GivenDevice_WhenGetTemperature_ThenResultIsNotNull()
        {
            var temperature = _device.GetTemperature();
            Assert.IsNotNull(temperature);
        }

        [Test]
        public void GivenDevice_WhenSetDetailFilterTo32_ThenDetailFilterIs32()
        {
            bool success = _device.SetDetailFilter(32);
            Assert.IsTrue(success);

            var value = _device.GetDetailFilter();
            Assert.IsTrue(value == 32);

            _device.SetDetailFilter(64);
        }

        [Test]
        public void GivenDevice_WhenSetDetailFilterToInvalidValue_ThenResultIsFalse()
        {
            bool success = _device.SetDetailFilter(-1);
            Assert.IsFalse(success);
        }

        [Test]
        public void GivenDevice_WhenSetBrightnessTo32_ThenBrightnessIs32()
        {
            bool success = _device.SetBrightness(32);
            Assert.IsTrue(success);

            var value = _device.GetBrightness();
            Assert.IsTrue(value == 32);

            _device.SetBrightness(64);
        }

        [Test]
        public void GivenDevice_WhenSetBrightnessToInvalidValue_ThenResultIsFalse()
        {
            bool success = _device.SetBrightness(-1);
            Assert.IsFalse(success);
        }

        [Test]
        public void GivenDevice_WhenSetContrastTo32_ThenContrastIs32()
        {
            bool success = _device.SetContrast(32);
            Assert.IsTrue(success);

            var value = _device.GetContrast();
            Assert.IsTrue(value == 32);

            _device.SetContrast(64);
        }

        [Test]
        public void GivenDevice_WhenSetContrastToInvalidValue_ThenResultIsFalse()
        {
            bool success = _device.SetContrast(-1);
            Assert.IsFalse(success);
        }
    }
}
