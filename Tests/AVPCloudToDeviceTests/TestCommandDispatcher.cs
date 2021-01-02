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
using Newtonsoft.Json.Linq;
using System.Linq;
using CommandProcessor;
using ControllableDeviceTypes.SonyKDL60W855Types;

namespace Tests
{
    public class TestCommandDispatcher : System.IDisposable
    {
        private dynamic _commandExecuterSettings;
        private dynamic _eventHubSettings;
        private const string _settingsFile = "settings.json";

        private ServiceClient _serviceClient;
        private CommandDispatcher _cd;
        private AtenVS0801H _hdmiDevice0, _hdmiDevice1;
        private SonyKDL60W855 _tvDevice;
        private SmartEventHubConsumer _smartEventHubConsumer;
        private CancellationTokenSource _cts;

        public TestCommandDispatcher()
        {
            using (StreamReader r = new StreamReader(_settingsFile))
            {
                string json = r.ReadToEnd();
                dynamic parsed = JsonConvert.DeserializeObject<ExpandoObject>(json, new ExpandoObjectConverter());
                _commandExecuterSettings = parsed.CommandDispatcher;
                _eventHubSettings = parsed.EventHub;
            }
        }

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            _smartEventHubConsumer = new SmartEventHubConsumer(_eventHubSettings.ConnectionString, _eventHubSettings.Name);

            _cts = new CancellationTokenSource();
            _ = _smartEventHubConsumer.ReceiveEventsFromDeviceAsync(_cts.Token);
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
            _serviceClient = ServiceClient.CreateFromConnectionString(_commandExecuterSettings.ConnectionString);
            _cd = new CommandDispatcher(_serviceClient, _commandExecuterSettings.DeviceId);
            _hdmiDevice0 = new AtenVS0801H(_serviceClient, _commandExecuterSettings.DeviceId, 0);
            _hdmiDevice1 = new AtenVS0801H(_serviceClient, _commandExecuterSettings.DeviceId, 1);
            _tvDevice = new SonyKDL60W855(_serviceClient, _commandExecuterSettings.DeviceId);
        }

        [Test]
        public void GivenJsonIsInvalid_WhenCommandDispatcherDispatch_ThenResultIsFalse()
        {
            using (StreamReader r = new StreamReader(@".\TestAssets\command-processor-fail-validation.json"))
            {
                string json = r.ReadToEnd();
                bool result = _cd.Dispatch(json);

                Assert.IsFalse(result);
            }
        }

        [Test]
        public void GivenJsonThatCallsFunction_WhenCommandDispatcherDispatch_ThenResultIsTrue()
        {
            using (StreamReader r = new StreamReader(@".\TestAssets\command-processor-call-function.json"))
            {
                string json = r.ReadToEnd();
                bool result = _cd.Dispatch(json);

                Assert.IsTrue(result);
            }
        }

        [Test]
        [Ignore("Requries the rack to be on, so ignoring during development.")]
        public void GivenJsonThetChangesHDMIInputPortToPort8_WhenCommandDispatcherDispatch_ThenResultIsTrueAndInputPortIsPort8()
        {
            using (StreamReader r = new StreamReader(@".\TestAssets\command-processor-change-hdmi-input.json"))
            {
                string json = r.ReadToEnd();

                //Firstly, make sure HDMI ports on both switchers are set to 1
                _hdmiDevice0.SetInputPort(ControllableDeviceTypes.AtenVS0801HTypes.InputPort.Port1);
                _hdmiDevice1.SetInputPort(ControllableDeviceTypes.AtenVS0801HTypes.InputPort.Port1);

                //JSON commands should change both ports to 8
                _cd.Dispatch(json);

                //Read back and verify
                var hdmiDevice0State = _hdmiDevice0.GetState();
                Assert.IsTrue(hdmiDevice0State.InputPort == ControllableDeviceTypes.AtenVS0801HTypes.InputPort.Port8);

                var hdmiDevice1State = _hdmiDevice1.GetState();
                Assert.IsTrue(hdmiDevice1State.InputPort == ControllableDeviceTypes.AtenVS0801HTypes.InputPort.Port8);
            }
        }

        [Test]
        public void GivenJsonThetGetsTVPowerState_WhenCommandDispatcherDispatch_ThenPowerStateMatches()
        {
            using (StreamReader r = new StreamReader(@".\TestAssets\command-processor-get-tv-power-state.json"))
            {
                string json = r.ReadToEnd();

                //Firstly, get the current TV power status directly from the TV device
                PowerStatus? powerStatusDirect = _tvDevice.GetPowerStatus();

                //Secondly, get the current TV power via a command processor json
                Guid id = Guid.NewGuid();
                bool result = _smartEventHubConsumer.RegisterEventQueue(id);

                _cd.Dispatch(json, id);
                var commandResult = _smartEventHubConsumer.GetEvents<CommandResult>(id).First();
                PowerStatus? powerStatusCommandDispatcher = commandResult.Result == null ? (PowerStatus?)null : (PowerStatus)(long)commandResult.Result;

                Assert.NotNull(powerStatusDirect);
                Assert.NotNull(powerStatusCommandDispatcher);
                Assert.AreEqual(powerStatusDirect, powerStatusCommandDispatcher);

                _smartEventHubConsumer.DeregisterEventQueue(id);
            }
        }

        [Test]
        public void GivenJsonThatCalls2FunctionsWith2SecondPostWaitAfterFirst_WhenGetEvents_ThenEventCountIs2()
        {           
            using (StreamReader r = new StreamReader(@".\TestAssets\command-processor-call-2-functions-with-2-second-post-wait.json"))
            {
                Guid id = Guid.NewGuid();
                bool result = _smartEventHubConsumer.RegisterEventQueue(id);
                Assert.IsTrue(result);

                string json = r.ReadToEnd();

                _cd.Dispatch(json, id);

                int eventCount = 0;
                foreach(var eventJson in _smartEventHubConsumer.GetEvents(id))
                {
                    eventCount++;
                }
                Assert.AreEqual(2, eventCount);

                _smartEventHubConsumer.DeregisterEventQueue(id);
            }
        }

        [Test]
        public void GivenJsonThatCalls2Functions_WhenDeregisterEventQueueDuringGetEvents_GetEventsCompletesOnNextEnumeration()
        {
            using (StreamReader r = new StreamReader(@".\TestAssets\command-processor-call-2-functions.json"))
            {
                Guid id = Guid.NewGuid();
                _smartEventHubConsumer.RegisterEventQueue(id);
                string json = r.ReadToEnd();

               _cd.Dispatch(json, id);

                int eventCount = 0;
                foreach (var eventJson in _smartEventHubConsumer.GetEvents(id))
                {
                    eventCount++;
                    bool result = _smartEventHubConsumer.DeregisterEventQueue(id);
                    Assert.IsTrue(result);
                }
                Assert.AreEqual(1, eventCount);

                _smartEventHubConsumer.DeregisterEventQueue(id);
            }
        }

        [Test]
        public void GivenJsonThatCalls10Functions_WhenGetEvents_ThenEventsAreReturnedInTheExpectedOrder()
        {
            using (StreamReader r = new StreamReader(@".\TestAssets\command-processor-call-10-functions.json"))
            {
                Guid id = Guid.NewGuid();
                _smartEventHubConsumer.RegisterEventQueue(id);
                string json = r.ReadToEnd();

                _cd.Dispatch(json, id);

                int i = 0;
                foreach (var eventJson in _smartEventHubConsumer.GetEvents(id))
                {
                    var parsedEventJson = JObject.Parse(eventJson);
                    var commandResult = parsedEventJson.ToObject<CommandProcessor.CommandResult>();

                    Assert.AreEqual(i, commandResult.Index);

                    i++;
                }

                Assert.AreEqual(10, i);

                _smartEventHubConsumer.DeregisterEventQueue(id);
            }
        }

        [Test]
        public void GivenJsonThatCallsFunctionThatWillFail_WhenGetEvents_ThenEventIsUnccessfulAndHasErrorMessage()
        {
            using (StreamReader r = new StreamReader(@".\TestAssets\command-processor-call-function-that-will-fail.json"))
            {
                Guid id = Guid.NewGuid();
                _smartEventHubConsumer.RegisterEventQueue(id);
                string json = r.ReadToEnd();

                _cd.Dispatch(json, id);

                int i = 0;
                foreach (var eventJson in _smartEventHubConsumer.GetEvents(id))
                {
                    var parsedEventJson = JObject.Parse(eventJson);
                    var commandResult = parsedEventJson.ToObject<CommandProcessor.CommandResult>();

                    Assert.AreEqual(false, commandResult.Success);
                    Assert.AreNotEqual(null, commandResult.ErrorMessage);
                    Assert.AreNotEqual(string.Empty, commandResult.ErrorMessage);

                    i++;
                }

                Assert.AreEqual(1, i);

                _smartEventHubConsumer.DeregisterEventQueue(id);
            }
        }

        [Test]
        public void GivenJsonThatCalls2FunctionsWith2SecondPostWaitAfterFirst_WhenGetEventsWithMaxEventWaitTime1Second_ThenEventCountIs0()
        {
            using (StreamReader r = new StreamReader(@".\TestAssets\command-processor-call-2-functions-with-2-second-post-wait.json"))
            {
                Guid id = Guid.NewGuid();
                bool result = _smartEventHubConsumer.RegisterEventQueue(id);
                Assert.IsTrue(result);

                string json = r.ReadToEnd();

                _cd.Dispatch(json, id);

                int eventCount = 0;
                foreach (var eventJson in _smartEventHubConsumer.GetEvents(id, TimeSpan.FromSeconds(1)))
                {
                    eventCount++;
                }
                Assert.AreEqual(0, eventCount);

                _smartEventHubConsumer.DeregisterEventQueue(id);
            }
        }

        [Test]
        public void GivenJsonThatCallsFunction_WhenGetEvents_ThenCommandResultCountAndIdAreCorrect()
        {
            using (StreamReader r = new StreamReader(@".\TestAssets\command-processor-call-function.json"))
            {
                Guid id = Guid.NewGuid();
                _smartEventHubConsumer.RegisterEventQueue(id);
                string json = r.ReadToEnd();

                _cd.Dispatch(json, id);

                foreach (var eventJson in _smartEventHubConsumer.GetEvents(id))
                {
                    var parsedEventJson = JObject.Parse(eventJson);
                    var commandResult = parsedEventJson.ToObject<CommandProcessor.CommandResult>();

                    Assert.AreEqual(1, commandResult.Count);
                    Assert.AreEqual(id, commandResult.Id);
                }

                _smartEventHubConsumer.DeregisterEventQueue(id);
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
