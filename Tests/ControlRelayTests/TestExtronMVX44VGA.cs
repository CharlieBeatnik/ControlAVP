using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ControllableDevice;
using System.IO;
using Newtonsoft.Json.Linq;
using ControllableDeviceTypes.ExtronMVX44VGATypes;

namespace Tests
{
    [TestClass]
    public class TestExtronMVX44VGA
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

            _deviceSettings = jsonParsed["Devices"]["ExtronMVX44VGA"][0];
        }

        public static ExtronMVX44VGA CreateDevice()
        {
            return new ExtronMVX44VGA(_deviceSettings["portId"].ToString());
        }

        public static ExtronMVX44VGA CreateInvalidDevice()
        {
            return new ExtronMVX44VGA("invalid");
        }

        [TestMethod]
        public void GivenDevice_WhenGetFirmware_ThenFirmwareIsNotNull()
        {
            using (var device = CreateDevice())
            {
                var firmware = device.GetFirmware();
                Assert.IsNotNull(firmware);
            }
        }

        [TestMethod]
        public void GivenDevice_WhenGetFirmware_ThenFirmwareIsGTE1_4()
        {
            using (var device = CreateDevice())
            {
                var firmware = device.GetFirmware();
                Assert.IsTrue(firmware >= new Version(1, 4));
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
        [ExpectedException(typeof(ArgumentException))]
        public void GivenEmptyPartialId_WhenNewDevice_ThenExceptionThrown()
        {
            using (var device = new ExtronMVX44VGA(string.Empty))
            {
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void GivenNullPartialId_WhenNewDevice_ThenExceptionThrown()
        {
            using (var device = new ExtronMVX44VGA(null))
            {
            }
        }

        [TestMethod]
        public void GivenInvalidPartialId_WhenNewDevice_ThenDeviceIsNotAvailable()
        {
            using (var device = new ExtronMVX44VGA("invalid"))
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
        public void GivenDevice_WhenIssueAllResetTypes_ThenAllResetsSuccessful()
        {
            using (var device = CreateDevice())
            {
                foreach (var resetType in Enum.GetValues(typeof(ResetType)))
                {
                    Assert.IsTrue(device.Reset((ResetType)resetType));
                }
            }
        }

        [TestMethod]
        public void GivenDevice_WhenGetInputPort_ThenTieIsNotNull()
        {
            using (var device = CreateDevice())
            {
                InputPort? inputTie = device.GetInputPortForOutputPort(OutputPort.Port1, TieType.Video);
                Assert.IsNotNull(inputTie);

                inputTie = device.GetInputPortForOutputPort(OutputPort.Port1, TieType.Audio);
                Assert.IsNotNull(inputTie);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void GivenDevice_WhenGetInputPortWithTypeAudioVideo_ThenExceptionThrown()
        {
            using (var device = CreateDevice())
            {
                device.GetInputPortForOutputPort(OutputPort.Port1, TieType.AudioVideo);
            }
        }

        [TestMethod]
        public void GivenDevice_WhenGetTieState_ThenStateIsNotNull()
        {
            using (var device = CreateDevice())
            {
                Assert.IsNotNull(device.GetTieState());
            }
        }

        [TestMethod]
        public void GivenDevice_WhenTieInputPortToAllOutputPorts_ThenTieIsSuccessful()
        {
            using (var device = CreateDevice())
            {
                var inputPort = InputPort.Port1;
                Assert.IsTrue(device.TieInputPortToAllOutputPorts(inputPort, TieType.AudioVideo));

                var tieState = device.GetTieState();
                Assert.IsNotNull(tieState);

                Assert.AreEqual(tieState.Video[OutputPort.Port1], inputPort);
                Assert.AreEqual(tieState.Video[OutputPort.Port2], inputPort);
                Assert.AreEqual(tieState.Video[OutputPort.Port3], inputPort);
                Assert.AreEqual(tieState.Video[OutputPort.Port4], inputPort);

                Assert.AreEqual(tieState.Audio[OutputPort.Port1], inputPort);
                Assert.AreEqual(tieState.Audio[OutputPort.Port2], inputPort);
                Assert.AreEqual(tieState.Audio[OutputPort.Port3], inputPort);
                Assert.AreEqual(tieState.Audio[OutputPort.Port4], inputPort);
            }
        }

        [TestMethod]
        public void GivenDevice_WhenTieAudioAndVideoToDifferentInputPorts_ThenTieIsSuccessful()
        {
            using (var device = CreateDevice())
            {
                var inputPortVideo = InputPort.Port1;
                var inputPortAudio = InputPort.Port2;

                Assert.IsTrue(device.TieInputPortToAllOutputPorts(inputPortVideo, TieType.Video));
                Assert.IsTrue(device.TieInputPortToAllOutputPorts(inputPortAudio, TieType.Audio));

                var tieState = device.GetTieState();
                Assert.IsNotNull(tieState);

                Assert.AreEqual(tieState.Video[OutputPort.Port1], inputPortVideo);
                Assert.AreEqual(tieState.Video[OutputPort.Port2], inputPortVideo);
                Assert.AreEqual(tieState.Video[OutputPort.Port3], inputPortVideo);
                Assert.AreEqual(tieState.Video[OutputPort.Port4], inputPortVideo);

                Assert.AreEqual(tieState.Audio[OutputPort.Port1], inputPortAudio);
                Assert.AreEqual(tieState.Audio[OutputPort.Port2], inputPortAudio);
                Assert.AreEqual(tieState.Audio[OutputPort.Port3], inputPortAudio);
                Assert.AreEqual(tieState.Audio[OutputPort.Port4], inputPortAudio);
            }
        }

        [TestMethod]
        public void GivenDevice_WhenTieInputPortToOutputPort_ThenTieIsSuccessful()
        {
            using (var device = CreateDevice())
            {
                var result = device.TieInputPortToOutputPort(InputPort.Port1, OutputPort.Port1, TieType.Audio);
                Assert.IsNotNull(result);
                Assert.AreEqual(device.GetInputPortForOutputPort(OutputPort.Port1, TieType.Audio), InputPort.Port1);

                result = device.TieInputPortToOutputPort(InputPort.Port2, OutputPort.Port2, TieType.Video);
                Assert.IsNotNull(result);
                Assert.AreEqual(device.GetInputPortForOutputPort(OutputPort.Port2, TieType.Video), InputPort.Port2);

                result = device.TieInputPortToOutputPort(InputPort.Port3, OutputPort.Port3, TieType.AudioVideo);
                Assert.IsNotNull(result);
                Assert.AreEqual(device.GetInputPortForOutputPort(OutputPort.Port3, TieType.Video), InputPort.Port3);
                Assert.AreEqual(device.GetInputPortForOutputPort(OutputPort.Port3, TieType.Audio), InputPort.Port3);
            }
        }

        public static void GivenInvalidDevice_WhenGetAvailable_ThenDeviceIsNotAvailable()
        {
            using (var device = CreateInvalidDevice())
            {
                Assert.IsFalse(device.GetAvailable());
            }
        }

        [TestMethod]
        public void GivenInvalidDevice_WhenGetFirmware_ThenResultsIsNull()
        {
            using (var device = CreateInvalidDevice())
            {
                Assert.IsNull(device.GetFirmware());
            }
        }

        [TestMethod]
        public void GivenInvalidDevice_WheReset_ThenResultsIsFalse()
        {
            using (var device = CreateInvalidDevice())
            {
                Assert.IsFalse(device.Reset(ResetType.Full));
            }
        }

        [TestMethod]
        public void GivenInvalidDevice_WhenGetInputPortForOutputPort_ThenResultsIsNull()
        {
            using (var device = CreateInvalidDevice())
            {
                Assert.IsNull(device.GetInputPortForOutputPort(OutputPort.Port1, TieType.Audio));
            }
        }

        [TestMethod]
        public void GivenInvalidDevice_WhenGetTieState_ThenResultsIsNull()
        {
            using (var device = CreateInvalidDevice())
            {
                Assert.IsNull(device.GetTieState());
            }
        }

        [TestMethod]
        public void GivenInvalidDevice_WhenTieInputPortToAllOutputPorts_ThenResultsIsFalse()
        {
            using (var device = CreateInvalidDevice())
            {
                Assert.IsFalse(device.TieInputPortToAllOutputPorts(InputPort.Port1, TieType.Audio));
            }
        }

        [TestMethod]
        public void GivenInvalidDevice_WhenTieInputPortToOutputPort_ThenResultsIsFalse()
        {
            using (var device = CreateInvalidDevice())
            {
                Assert.IsFalse(device.TieInputPortToOutputPort(InputPort.Port1, OutputPort.Port1, TieType.Audio));
            }
        }
        
    }
}
