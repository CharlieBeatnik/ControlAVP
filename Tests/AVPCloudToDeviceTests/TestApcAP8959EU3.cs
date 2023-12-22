using AVPCloudToDevice;
using Microsoft.Azure.Devices;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using NUnit.Framework;
using System.Dynamic;
using System.IO;
using System.Linq;
using ControllableDeviceTypes.ApcAP8959EU3Types;
using System.Collections.Generic;

namespace Tests
{
    public class TestApcAP8959EU3
    {
        private readonly dynamic _settings;
        private const string _settingsFile = "settings.json";

        private ServiceClient _serviceClient;
        private ApcAP8959EU3 _device;

        private readonly int _invalidOutletId = 999;

        public TestApcAP8959EU3()
        {
            using StreamReader r = new(_settingsFile);
            string json = r.ReadToEnd();
            dynamic parsed = JsonConvert.DeserializeObject<ExpandoObject>(json, new ExpandoObjectConverter());
            _settings = parsed.ApcAP8959EU3;
        }

        [SetUp]
        public void Setup()
        {
            _serviceClient = ServiceClient.CreateFromConnectionString(_settings.ConnectionString);
            _device = new ApcAP8959EU3(_serviceClient, _settings.DeviceId);
        }

        [Test]
        public void GivenDevice_WhenGetOutlets_ThenOutletCountIs24()
        {
            var outlets = _device.GetOutlets();
            if (outlets == null) Assert.Fail();
            Assert.That(outlets.Count(), Is.EqualTo(24));
        }

        [Test]
        [Ignore("Risk of physical component wear, ignore by default.")]
        public void GivenOutlet5IsOff_WhenTurnOutlet5On_ThenOutlet5IsOn()
        {
            int outletId = 5;

            //Given
            Assert.That(_device.TurnOutletOff(outletId), Is.True);
            var outlets = _device.GetOutletsWaitForPending();
            if (outlets == null) Assert.Fail();

            var outlet = outlets.First(o => o.Id == outletId);
            if (outlet == null) Assert.Fail();
            Assert.That(outlet.State, Is.EqualTo(Outlet.PowerState.Off));

            //When
            Assert.That(_device.TurnOutletOn(outletId), Is.True);

            //Then
            outlets = _device.GetOutletsWaitForPending();
            if (outlets == null) Assert.Fail();
            outlet = outlets.First(o => o.Id == outletId);
            if (outlet == null) Assert.Fail();
            Assert.That(outlet.State, Is.EqualTo(Outlet.PowerState.On));
        }

        [Test]
        public void GivenDevice_WhenCallAvailable_ThenDeviceIsAvailable()
        {
            Assert.That(_device.GetAvailable(), Is.True);
        }

        [Test]
        public void GivenDevice_WhenTurnOutletOnWithInvalidId_ThenSuccessIsFalse()
        {
            Assert.That(_device.TurnOutletOn(_invalidOutletId), Is.False);
        }

        [Test]
        public void GivenDevice_WhenTurnOutletOffWithInvalidId_ThenSuccessIsFalse()
        {
            Assert.That(_device.TurnOutletOff(_invalidOutletId), Is.False);
        }

        [Test]
        public void GivenDevice_WhenGetPhases_ThenFirstPhaseVoltageIsGreaterThan220()
        {
            var phase = (List<Phase>)_device.GetPhases();

            Assert.That(phase, Is.Not.Null);
            Assert.That(phase, Is.Not.Empty);

            Assert.That(phase.First().Voltage, Is.GreaterThan(220));
        }
    }
}
