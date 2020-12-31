using AVPCloudToDevice;
using Microsoft.Azure.Devices;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using NUnit.Framework;
using System.Dynamic;
using System.IO;
using ControllableDeviceTypes.AtenVS0801HTypes;
using EventHub;
using System.Threading;
using System;
using System.Threading.Tasks;

namespace Tests
{
    public class TestCommandProcessor : System.IDisposable
    {
        private dynamic _commandProcessorSettings;
        private dynamic _eventHubSettings;
        private const string _settingsFile = "settings.json";

        private ServiceClient _serviceClient;
        private CommandProcessor _cp;
        private AtenVS0801H _hdmiDevice0, _hdmiDevice1;
        private SmartEventHubConsumer _smartEventHubConsumer;
        private CancellationTokenSource _cts;

        public TestCommandProcessor()
        {
            using (StreamReader r = new StreamReader(_settingsFile))
            {
                string json = r.ReadToEnd();
                dynamic parsed = JsonConvert.DeserializeObject<ExpandoObject>(json, new ExpandoObjectConverter());
                _commandProcessorSettings = parsed.CommandProcessor;
                _eventHubSettings = parsed.EventHub;
            }
        }

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            _smartEventHubConsumer = new SmartEventHubConsumer(_eventHubSettings.ConnectionString, _eventHubSettings.Name);

            _cts = new CancellationTokenSource();
            _ = _smartEventHubConsumer.ReceiveMessagesFromDeviceAsync(_cts.Token);
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            _cts.Cancel();
            _cts.Token.WaitHandle.WaitOne();
        }

        [SetUp]
        public void Setup()
        {
            _serviceClient = ServiceClient.CreateFromConnectionString(_commandProcessorSettings.ConnectionString);
            _cp = new CommandProcessor(_serviceClient, _commandProcessorSettings.DeviceId);
            _hdmiDevice0 = new AtenVS0801H(_serviceClient, _commandProcessorSettings.DeviceId, 0);
            _hdmiDevice1 = new AtenVS0801H(_serviceClient, _commandProcessorSettings.DeviceId, 1);
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
                _cp.Execute(json);

                //Read back and verify
                var hdmiDevice0State = _hdmiDevice0.GetState();
                Assert.IsTrue(hdmiDevice0State.InputPort == InputPort.Port8);

                var hdmiDevice1State = _hdmiDevice1.GetState();
                Assert.IsTrue(hdmiDevice1State.InputPort == InputPort.Port8);
            }
        }

        [Test]
        public void GivenJsonThatCalls2FunctionsWithPostWait_WhenGetMessages_ThenMessageCountIs2()
        {           
            using (StreamReader r = new StreamReader(@".\TestAssets\command-processor-call-2-functions-with-post-wait.json"))
            {
                Guid id = Guid.NewGuid();
                bool result = _smartEventHubConsumer.RegisterEventQueue(id);
                Assert.IsTrue(result);

                string json = r.ReadToEnd();

                _cp.Execute(json, id);

                int messageCount = 0;
                foreach(var message in _smartEventHubConsumer.GetMessages(id))
                {
                    messageCount++;
                }
                Assert.AreEqual(2, messageCount);

                _smartEventHubConsumer.DeregisterEventQueue(id);
            }
        }

        [Test]
        public void GivenJsonThatCalls2Functions_WhenDeregisterEventQueueDuringGetMessages_GetMessagesCompletesOnNextEnumeration()
        {
            using (StreamReader r = new StreamReader(@".\TestAssets\command-processor-call-2-functions-with-post-wait.json"))
            {
                Guid id = Guid.NewGuid();
                _smartEventHubConsumer.RegisterEventQueue(id);
                string json = r.ReadToEnd();

               _cp.Execute(json, id);

                int messageCount = 0;
                foreach (var message in _smartEventHubConsumer.GetMessages(id))
                {
                    messageCount++;
                    bool result = _smartEventHubConsumer.DeregisterEventQueue(id);
                    Assert.IsTrue(result);
                }

                Assert.AreEqual(1, messageCount);
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_cts != null)
                {
                    _cts.Dispose();
                    _cts = null;
                }
            }
        }
    }
}
