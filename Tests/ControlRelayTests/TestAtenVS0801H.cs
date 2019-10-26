using Microsoft.VisualStudio.TestTools.UnitTesting;
using ControllableDevice;
using ControllableDeviceTypes.AtenVS0801HTypes;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System;

namespace Tests
{
    [TestClass]
    public class TestAtenVS0801H
    {
        private const string _settingsFile = "settings.json";
        private static JToken _deviceSettings;

        [ClassInitialize]
        public static void ClassInitialize(TestContext tc)
        {
            if (tc == null)
            {
                throw new ArgumentNullException(nameof(tc));
            }

            JObject jsonParsed;
            using (StreamReader r = new StreamReader(_settingsFile))
            {
                string json = r.ReadToEnd();
                jsonParsed = JObject.Parse(json);
            }

            _deviceSettings = jsonParsed["Devices"]["AtenVS0801H"][0];
        }

        public static AtenVS0801H CreateDevice()
        {
            return new AtenVS0801H(_deviceSettings["portId"].ToString());
        }

        public static AtenVS0801H CreateInvalidDevice()
        {
            return new AtenVS0801H("invalid");
        }

        [TestMethod]
        public void GivenInputPortIsPort1_WhenGoToNextInput_ThenInputPortIsPort2()
        {
            using (var device = CreateDevice())
            {
                Assert.IsTrue(device.SetInputPort(InputPort.Port1));
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
                Assert.IsTrue(device.SetInputPort(InputPort.Port2));
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
                Assert.IsTrue(device.SetInputPort(InputPort.Port1));
                var state = device.GetState();
                Assert.IsTrue(state != null);
                Assert.IsTrue(state.InputPort == InputPort.Port1);

                Assert.IsTrue(device.SetInputPort(InputPort.Port2));
                state = device.GetState();
                Assert.IsTrue(state != null);
                Assert.IsTrue(state.InputPort == InputPort.Port2);
            }
        }

        [TestMethod]
        public void GivenDevice_WhenGetAvailable_ThenDeviceIsAvailable()
        {
            using (var device = CreateDevice())
            {
                Assert.IsTrue(device.GetAvailable());
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void GivenEmptyPartialId_WhenNewDevice_ThenExceptionThrown()
        {
            using (var device = new AtenVS0801H(string.Empty))
            {
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void GivenNullPartialId_WhenNewDevice_ThenExceptionThrown()
        {
            using (var device = new AtenVS0801H(null))
            {
            }
        }

        [TestMethod]
        public void GivenInvalidPartialId_WhenNewDevice_ThenDeviceIsNotAvailable()
        {
            using (var device = new AtenVS0801H("invalid"))
            {
                Assert.IsFalse(device.GetAvailable());
            }
        }

        [TestMethod]
        public void GivenDevice_WhenCreateAnotherDeviceWithSameID_ThenSecondDeviceIsNotAvailable()
        {
            using (var device1 = CreateDevice())
            {
                Assert.IsTrue(device1.GetAvailable());
                using (var device2 = CreateDevice())
                {
                    Assert.IsFalse(device2.GetAvailable());
                }
            }
        }

        [TestMethod]
        public void GivenDevice_WhenGetState_ThenFirmwareIsGTE2_0_197()
        {
            using (var device = CreateDevice())
            {
                var state = device.GetState();
                Assert.IsNotNull(state);

                Assert.IsTrue(state.Firmware >= new Version(2, 0, 197));
            }
        }

        [TestMethod]
        public void GivenInvalidDevice_WhenGetAvailable_ThenDeviceIsNotAvailable()
        {
            using (var device = CreateInvalidDevice())
            {
                Assert.IsFalse(device.GetAvailable());
            }
        }

        [TestMethod]
        public void GivenInvalidDevice_WhenGetState_ThenResultsIsNull()
        {
            using (var device = CreateInvalidDevice())
            {
                Assert.IsNull(device.GetState());
            }
        }

        [TestMethod]
        public void GivenInvalidDevice_WhenSetGoTo_ThenResultsIsFalse()
        {
            using (var device = CreateInvalidDevice())
            {
                Assert.IsFalse(device.SetGoTo(true));
            }
        }

        [TestMethod]
        public void GivenInvalidDevice_WhenSetMode_ThenResultsIsFalse()
        {
            using (var device = CreateInvalidDevice())
            {
                Assert.IsFalse(device.SetMode(SwitchMode.Auto, InputPort.Port1));
            }
        }

        [TestMethod]
        public void GivenInvalidDevice_WhenSetOutput_ThenResultsIsFalse()
        {
            using (var device = CreateInvalidDevice())
            {
                Assert.IsFalse(device.SetOutput(true));
            }
        }

        [TestMethod]
        public void GivenInvalidDevice_WhenSetInput_ThenResultsIsFalse()
        {
            using (var device = CreateInvalidDevice())
            {
                Assert.IsFalse(device.SetInputPort(InputPort.Port1));
            }
        }

        [TestMethod]
        public void GivenInvalidDevice_WhenGoToPreviousInput_ThenResultsIsFalse()
        {
            using (var device = CreateInvalidDevice())
            {
                Assert.IsFalse(device.GoToPreviousInput());
            }
        }

        [TestMethod]
        public void GivenInvalidDevice_WhenGoToNextInput_ThenResultsIsFalse()
        {
            using (var device = CreateInvalidDevice())
            {
                Assert.IsFalse(device.GoToNextInput());
            }
        }
    }
}
