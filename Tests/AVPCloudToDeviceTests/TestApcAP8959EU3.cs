using AVPCloudToDevice;
using Microsoft.Azure.Devices;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using NUnit.Framework;
using System.Dynamic;
using System.IO;
using System.Linq;
using PduDevice.ApcAP8959EU3Types;

namespace Tests
{
    public class TestApcAP8959EU3
    {
        private dynamic _settings;
        private readonly string _settingsFile = "settings.json";

        private ServiceClient _serviceClient;
        private ApcAP8959EU3 _device;

        public TestApcAP8959EU3()
        {
            using (StreamReader r = new StreamReader(_settingsFile))
            {
                string json = r.ReadToEnd();
                dynamic parsed = JsonConvert.DeserializeObject<ExpandoObject>(json, new ExpandoObjectConverter());
                _settings = parsed.ApcAP8959EU3;
            }
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
            Assert.IsTrue(outlets.Count() == 24);
        }

        [Test]
        public void GivenOutlet5IsOff_WhenTurnOutlet5On_ThenOutlet5IsOn()
        {
            int outletId = 5;

            //Given
            Assert.IsTrue(_device.TurnOutletOff(outletId));
            var outlets = _device.GetOutlets();
            Assert.IsTrue(outlets  != null);
            var outlet = outlets.First(o => o.Id == outletId);
            Assert.IsTrue(outlet != null);
            Assert.IsTrue(outlet.State == Outlet.PowerState.Off);

            //When
            Assert.IsTrue(_device.TurnOutletOn(outletId));

            //Then
            outlets = _device.GetOutletsWaitForPending();
            Assert.IsTrue(outlets != null);
            outlet = outlets.First(o => o.Id == outletId);
            Assert.IsTrue(outlet != null);
            Assert.IsTrue(outlet.State == Outlet.PowerState.On);
        }
    }
}