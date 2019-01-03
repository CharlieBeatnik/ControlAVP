using Microsoft.VisualStudio.TestTools.UnitTesting;
using ControllableDevice;
using ControllableDeviceTypes.AtenVS0801HTypes;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Tests
{
    [TestClass]
    public class TestAtenVS0801H
    {
        private static AtenVS0801H _device = null;
        private readonly string _settingsFile = "settings.json";

        public TestAtenVS0801H()
        {
            JObject jsonParsed;
            using (StreamReader r = new StreamReader(_settingsFile))
            {
                string json = r.ReadToEnd();
                jsonParsed = JObject.Parse(json);
            }

            if (_device == null)
            {
                _device = new AtenVS0801H(jsonParsed["AtenVS0801H"][0]["PortId"].ToString());
            }
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
    }
}
