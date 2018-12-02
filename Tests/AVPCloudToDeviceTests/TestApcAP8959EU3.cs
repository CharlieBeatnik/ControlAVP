using AVPCloudToDevice;
using Microsoft.Azure.Devices;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using NUnit.Framework;
using System.Dynamic;
using System.IO;
using System.Linq;

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
    }
}