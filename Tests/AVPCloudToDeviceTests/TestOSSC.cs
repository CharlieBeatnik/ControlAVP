using AVPCloudToDevice;
using Microsoft.Azure.Devices;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using NUnit.Framework;
using System.Dynamic;
using System.IO;
using ControllableDeviceTypes.OSSCTypes;

namespace Tests
{
    internal sealed class TestOSSC
    {
        private readonly dynamic _settings;
        private const string _settingsFile = "settings.json";

        private ServiceClient _serviceClient;
        private OSSC _device;

        public TestOSSC()
        {
            using StreamReader r = new(_settingsFile);
            string json = r.ReadToEnd();
            dynamic parsed = JsonConvert.DeserializeObject<ExpandoObject>(json, new ExpandoObjectConverter());
            _settings = parsed.OSSC;
        }

        [SetUp]
        public void Setup()
        {
            _serviceClient = ServiceClient.CreateFromConnectionString(_settings.ConnectionString);
            _device = new OSSC(_serviceClient, _settings.DeviceId);
        }

        [Test]
        public void GivenDevice_WhenSendCommand_ThenSuccessIsTrue()
        {
            Assert.That(_device.SendCommand(CommandName.Menu), Is.True);
        }

        [Test]
        public void GivenDevice_WhenLoadProfile_ThenSuccessIsTrue()
        {
            Assert.That(_device.LoadProfile(ProfileName.Profile0), Is.True);
            
        }

    }
}
