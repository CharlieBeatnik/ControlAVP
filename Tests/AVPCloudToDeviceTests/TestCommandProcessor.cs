using AVPCloudToDevice;
using Microsoft.Azure.Devices;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using NUnit.Framework;
using System.Dynamic;
using System.IO;
using ControllableDeviceTypes.AtenVS0801HTypes;

namespace Tests
{
    public class TestCommandProcessor
    {
        private dynamic _settings;
        private const string _settingsFile = "settings.json";

        private ServiceClient _serviceClient;
        private CommandProcessor _cp;
        private AtenVS0801H _hdmiDevice0, _hdmiDevice1;

        public TestCommandProcessor()
        {
            using (StreamReader r = new StreamReader(_settingsFile))
            {
                string json = r.ReadToEnd();
                dynamic parsed = JsonConvert.DeserializeObject<ExpandoObject>(json, new ExpandoObjectConverter());
                _settings = parsed.CommandProcessor;
            }
        }

        [SetUp]
        public void Setup()
        {
            _serviceClient = ServiceClient.CreateFromConnectionString(_settings.ConnectionString);
            _cp = new CommandProcessor(_serviceClient, _settings.DeviceId);
            _hdmiDevice0 = new AtenVS0801H(_serviceClient, _settings.DeviceId, 0);
            _hdmiDevice1 = new AtenVS0801H(_serviceClient, _settings.DeviceId, 1);
        }

        [Test]
        public void GivenJsonIsInvalid_WhenCommandProcessorExecute_ThenResultIsFalse()
        {
            using (StreamReader r = new StreamReader(@".\TestAssets\command-processor-fail-validation.json"))
            {
                string json = r.ReadToEnd();
                bool result = _cp.Execute(json);

                Assert.IsFalse(result);
            }
        }

        [Test]
        public void GivenJsonThatCallsFunction_WhenCommandProcessorExecute_ThenResultIsTrue()
        {
            using (StreamReader r = new StreamReader(@".\TestAssets\command-processor-call-function.json"))
            {
                string json = r.ReadToEnd();
                bool result = _cp.Execute(json);

                Assert.IsTrue(result);
            }
        }

        [Test]
        public void GivenJsonTheChangesHDMIInputPortToPort8_WhenCommandProcessorExecute_ThenResultIsTrueAndInputPortIsPort8()
        {
            using (StreamReader r = new StreamReader(@".\TestAssets\command-processor-change-hdmi-input.json"))
            {
                string json = r.ReadToEnd();

                //Firstly, make sure HDMI ports on both switchers are set to 1
                _hdmiDevice0.SetInputPort(InputPort.Port1);
                _hdmiDevice1.SetInputPort(InputPort.Port1);

                //JSON commands should change both ports to 8
                bool result = _cp.Execute(json);
                Assert.IsTrue(result);

                //Read back and verify
                var hdmiDevice0State = _hdmiDevice0.GetState();
                Assert.IsTrue(hdmiDevice0State.InputPort == InputPort.Port8);

                var hdmiDevice1State = _hdmiDevice1.GetState();
                Assert.IsTrue(hdmiDevice1State.InputPort == InputPort.Port8);
            }
        }
    }
}
