using Microsoft.VisualStudio.TestTools.UnitTesting;
using ControllableDevice;
using ControllableDeviceTypes.AtenVS0801HTypes;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace Tests
{
    [TestClass]
    public class TestAtenVS0801H
    {
        private static AtenVS0801H _device;
        private static readonly string _settingsFile = "settings.json";
        private static JToken _deviceSettings;

        [ClassInitialize]
        public static void ClassInitialize(TestContext tc)
        {
            JObject jsonParsed;
            using (StreamReader r = new StreamReader(_settingsFile))
            {
                string json = r.ReadToEnd();
                jsonParsed = JObject.Parse(json);
            }

            _deviceSettings = jsonParsed["AtenVS0801H"][0];
        }

        [ClassCleanup]
        public static void ClassCleanup()
        {
        }

        [TestInitialize]
        public void TestInitialize()
        {
            _device = new AtenVS0801H(_deviceSettings["PortId"].ToString());
        }

        [TestCleanup]
        public void TestCleanup()
        {
            _device.Dispose();
            _device = null;
        }

        [TestMethod]
        public void GivenInputPortIsPort1_WhenGoToNextInput_ThenInputPortIsPort2()
        {
            Assert.IsTrue(_device.SetInput(InputPort.Port1));
            Assert.IsTrue(_device.GoToNextInput());

            var state = _device.GetState();
            Assert.IsTrue(state != null);
            Assert.IsTrue(state.InputPort == InputPort.Port2);
        }

        [TestMethod]
        public void GivenInputPortIsPort2_WhenGoToPreviousInput_ThenInputPortIsPort1()
        {
            Assert.IsTrue(_device.SetInput(InputPort.Port2));
            Assert.IsTrue(_device.GoToPreviousInput());

            var state = _device.GetState();
            Assert.IsTrue(state != null);
            Assert.IsTrue(state.InputPort == InputPort.Port1);
        }

        [TestMethod]
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

        [TestMethod]
        public void GivenDevice_WhenCallAvailable_ThenDeviceIsAvailable()
        {
            Assert.IsTrue(_device.GetAvailable());
        }
    }
}
