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

        public AtenVS0801H CreateDevice()
        {
            return AtenVS0801H.Create(_deviceSettings["PortId"].ToString());
        }

        [TestMethod]
        public void GivenInputPortIsPort1_WhenGoToNextInput_ThenInputPortIsPort2()
        {
            using (var device = CreateDevice())
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
            using (var device = CreateDevice())
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
            using (var device = CreateDevice())
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
            using (var device = CreateDevice())
            {
                Assert.IsTrue(device.GetAvailable());
            }
        }

        [TestMethod]
        public void GivenEmptyPartialId_WhenNewDevice_ThenDeviceIsNull()
        {
            var device = AtenVS0801H.Create(string.Empty);
            Assert.IsNull(device);
        }

        [TestMethod]
        public void GivenNullPartialId_WhenNewDevice_ThenDeviceIsNull()
        {
            var device = AtenVS0801H.Create(null);
            Assert.IsNull(device);
        }

        [TestMethod]
        public void GivenInvalidPartialId_WhenNewDevice_ThenDeviceIsNull()
        {
            var device = AtenVS0801H.Create("invalid");
            Assert.IsNull(device);
        }
    }
}
