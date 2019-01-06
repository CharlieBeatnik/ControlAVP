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
        private static List<AtenVS0801H> _devices = new List<AtenVS0801H>();
        private readonly string _settingsFile = "settings.json";

        public TestAtenVS0801H()
        {
            JObject jsonParsed;
            using (StreamReader r = new StreamReader(_settingsFile))
            {
                string json = r.ReadToEnd();
                jsonParsed = JObject.Parse(json);
            }

            if (_devices.Count == 0)
            {
                foreach (var deviceSettings in jsonParsed["AtenVS0801H"])
                {
                    var device = new AtenVS0801H(deviceSettings["PortId"].ToString());
                    _devices.Add(device);
                }
            }
        }

        [TestMethod]
        public void GivenInputPortIsPort1_WhenGoToNextInput_ThenInputPortIsPort2()
        {
            foreach (var device in _devices)
            {
                Assert.IsTrue(device.SetInput(InputPort.Port1));
                Assert.IsTrue(device.GoToNextInput());

                var state = device.GetState();
                Assert.IsTrue(state != null);
                Assert.IsTrue(state.InputPort == InputPort.Port2);
            }
        }

        [TestMethod]
        public void GivenInputPortIsPort2_WhenGoToPreviousInput_ThenInputPortIsPort1()
        {
            foreach (var device in _devices)
            {
                Assert.IsTrue(device.SetInput(InputPort.Port2));
                Assert.IsTrue(device.GoToPreviousInput());

                var state = device.GetState();
                Assert.IsTrue(state != null);
                Assert.IsTrue(state.InputPort == InputPort.Port1);
            }
        }

        [TestMethod]
        public void GivenInputPortIsPort1_WhenSetInputPort2_ThenInputPortIsPort2()
        {
            foreach (var device in _devices)
            {
                Assert.IsTrue(device.SetInput(InputPort.Port1));
                var state = device.GetState();
                Assert.IsTrue(state != null);
                Assert.IsTrue(state.InputPort == InputPort.Port1);

                Assert.IsTrue(device.SetInput(InputPort.Port2));
                state = device.GetState();
                Assert.IsTrue(state != null);
                Assert.IsTrue(state.InputPort == InputPort.Port2);
            }
        }

        [TestMethod]
        public void GivenDevice_WhenCallAvailable_ThenDeviceIsAvailable()
        {
            foreach (var device in _devices)
            {
                Assert.IsTrue(device.GetAvailable());
            }
        }
    }
}
