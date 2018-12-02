using AVPCloudToDevice;
using Microsoft.Azure.Devices;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using NUnit.Framework;
using System.Dynamic;
using System.IO;
using System.Linq;
using AudioVideoDevice.AtenVS0801HTypes;

namespace Tests
{
    public class TestAtenVS0801H
    {
        private dynamic _settings;
        private readonly string _settingsFile = "settings.json";

        private ServiceClient _serviceClient;
        private AtenVS0801H _device;

        public TestAtenVS0801H()
        {
            using (StreamReader r = new StreamReader(_settingsFile))
            {
                string json = r.ReadToEnd();
                dynamic parsed = JsonConvert.DeserializeObject<ExpandoObject>(json, new ExpandoObjectConverter());
                _settings = parsed.AtenVS0801H;
            }
        }

        [SetUp]
        public void Setup()
        {
            _serviceClient = ServiceClient.CreateFromConnectionString(_settings.ConnectionString);
            _device = new AtenVS0801H(_serviceClient, _settings.DeviceId, (int)_settings.HdmiSwitchId);
        }

        [Test]
        public void GivenInputPortIsPort1_WhenGoToNextInput_ThenInputPortIsPort2()
        {
            Assert.IsTrue(_device.SetInput(InputPort.Port1));
            Assert.IsTrue(_device.GoToNextInput());

            var state = _device.GetState();
            Assert.IsTrue(state != null);
            Assert.IsTrue(state.InputPort == InputPort.Port2);
        }

        [Test]
        public void GivenInputPortIsPort2_WhenGoToPreviousInput_ThenInputPortIsPort1()
        {
            Assert.IsTrue(_device.SetInput(InputPort.Port2));
            Assert.IsTrue(_device.GoToPreviousInput());

            var state = _device.GetState();
            Assert.IsTrue(state != null);
            Assert.IsTrue(state.InputPort == InputPort.Port1);
        }

        [Test]
        public void GivenInputPortIsPort1_WhenSetInputPort2_ThenInputPortIsPort2()
        {
            Assert.IsTrue(_device.SetInput(InputPort.Port1));
            var state = _device.GetState();
            Assert.IsTrue(state != null);
            Assert.IsTrue(state.InputPort == InputPort.Port1);

            Assert.IsTrue(_device.SetInput(InputPort.Port2));
            state = _device.GetState();
            Assert.IsTrue(state != null);
            Assert.IsTrue(state.InputPort == InputPort.Port2);
        }
    }
}