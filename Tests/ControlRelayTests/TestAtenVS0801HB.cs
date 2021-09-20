using Microsoft.VisualStudio.TestTools.UnitTesting;
using ControllableDevice;
using ControllableDeviceTypes.AtenVS0801HBTypes;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System;
using System.Threading;

namespace Tests
{
    [TestClass]
    public class TestAtenVS0801HB
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

            _deviceSettings = jsonParsed["Devices"]["AtenVS0801HB"][0];
        }

        public static AtenVS0801HB CreateDevice()
        {
            return new AtenVS0801HB(_deviceSettings["portId"].ToString());
        }

        public static AtenVS0801HB CreateInvalidDevice()
        {
            return new AtenVS0801HB("invalid");
        }

        [TestMethod]
        public void GivenInputPortIsPort1_WhenSetInputPort2_ThenInputPortIsPort2()
        {
            using (var device = CreateDevice())
            {
                Assert.IsTrue(device.SetInputPort(InputPort.Port1));
                var state = device.GetState();
                Assert.IsNotNull(state);
                Assert.IsTrue(state.InputPort == InputPort.Port1);

                Assert.IsTrue(device.SetInputPort(InputPort.Port2));
                state = device.GetState();
                Assert.IsNotNull(state);
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
            using (var device = new AtenVS0801HB(string.Empty))
            {
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void GivenNullPartialId_WhenNewDevice_ThenExceptionThrown()
        {
            using (var device = new AtenVS0801HB(null))
            {
            }
        }

        [TestMethod]
        public void GivenInvalidPartialId_WhenNewDevice_ThenDeviceIsNotAvailable()
        {
            using (var device = new AtenVS0801HB("invalid"))
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
        public void GivenDevice_WhenGetState_ThenFirmwareIsGTE1_1_105()
        {
            using (var device = CreateDevice())
            {
                var state = device.GetState();
                Assert.IsNotNull(state);

                Assert.IsTrue(state.Firmware >= new Version(1, 1, 105));
            }
        }

        [TestMethod]
        public void GivenInputPortIsPort8_WhenGoToNextInput_ThenInputPortIsPort1()
        {
            using (var device = CreateDevice())
            {
                Assert.IsTrue(device.SetInputPort(InputPort.Port8));
                Assert.IsTrue(device.GoToNextInput());

                var state = device.GetState();
                Assert.IsNotNull(state);
                Assert.IsTrue(state.InputPort == InputPort.Port1);
            }
        }

        [TestMethod]
        public void GivenInputPortIsPort1_WhenGoToPreviousInput_ThenInputPortIsPort8()
        {
            using (var device = CreateDevice())
            {
                Assert.IsTrue(device.SetInputPort(InputPort.Port1));
                Assert.IsTrue(device.GoToPreviousInput());

                var state = device.GetState();
                Assert.IsNotNull(state);
                Assert.IsTrue(state.InputPort == InputPort.Port8);
            }
        }

        [TestMethod]
        public void GivenOutputIsTrue_WhenSetOutputFalse_ThenOutputIsFalse()
        {
            using (var device = CreateDevice())
            {
                //Set initial output state to true
                Assert.IsTrue(device.SetOutput(true));
                
                //Validate
                var state = device.GetState();
                Assert.IsNotNull(state);
                Assert.IsTrue(state.Output);

                //Set output state to false
                Assert.IsTrue(device.SetOutput(false));
                
                //Validate
                state = device.GetState();
                Assert.IsNotNull(state);
                Assert.IsFalse(state.Output);

                //Leave output state as true
                Assert.IsTrue(device.SetOutput(true));
            }
        }

        [TestMethod]
        public void GivenPowerOnDetectionIsTrue_WhenSetPowerOnDetectionFalse_ThenPowerOnDetectionIsFalse()
        {
            using (var device = CreateDevice())
            {
                //Set POD to true
                Assert.IsTrue(device.SetPowerOnDetection(true));

                //Validate
                var state = device.GetState();
                Assert.IsNotNull(state);
                Assert.IsTrue(state.PowerOnDetection);

                //Set POD to false
                Assert.IsTrue(device.SetPowerOnDetection(false));

                //Validate
                state = device.GetState();
                Assert.IsNotNull(state);
                Assert.IsFalse(state.PowerOnDetection);

                //Leave POD as true
                Assert.IsTrue(device.SetPowerOnDetection(true));
            }
        }

        [TestMethod]
        public void GivenSwitchModeIsOff_WhenSetSwitchModeToNext_ThenSwitchModeIsNext()
        {
            using (var device = CreateDevice())
            {
                //Set SwitchMode to Off
                Assert.IsTrue(device.SetMode(SwitchMode.Off, null));

                //Validate
                var state = device.GetState();
                Assert.IsNotNull(state);
                Assert.AreEqual(SwitchMode.Off, state.Mode);

                //Set SwitchMode to Next
                Assert.IsTrue(device.SetMode(SwitchMode.Next, null));

                //Validate
                state = device.GetState();
                Assert.IsNotNull(state);
                Assert.AreEqual(SwitchMode.Next, state.Mode);

                //Lead SwitchMode as Off
                Assert.IsTrue(device.SetMode(SwitchMode.Off, null));
            }
        }

        [TestMethod]
        public void GivenSwitchModeIsOff_WhenSetSwitchModeToPriorityInputPort8_ThenSwitchModeIsPriority()
        {
            using (var device = CreateDevice())
            {
                //Set SwitchMode to Off
                Assert.IsTrue(device.SetMode(SwitchMode.Off, null));

                //Validate
                var state = device.GetState();
                Assert.IsNotNull(state);
                Assert.AreEqual(SwitchMode.Off, state.Mode);

                //Set SwitchMode to Priority InputPort.Port8 
                Assert.IsTrue(device.SetMode(SwitchMode.Priority, InputPort.Port8));

                //Validate
                state = device.GetState();
                Assert.IsNotNull(state);
                Assert.AreEqual(SwitchMode.Priority, state.Mode);

                //Lead SwitchMode as Off
                Assert.IsTrue(device.SetMode(SwitchMode.Off, null));
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
        public void GivenInvalidDevice_WhenSetPowerOnDetection_ThenResultsIsFalse()
        {
            using (var device = CreateInvalidDevice())
            {
                Assert.IsFalse(device.SetPowerOnDetection(true));
            }
        }

        [TestMethod]
        public void GivenInvalidDevice_WhenSetMode_ThenResultsIsFalse()
        {
            using (var device = CreateInvalidDevice())
            {
                Assert.IsFalse(device.SetMode(SwitchMode.Priority, InputPort.Port1));
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
    }
}
