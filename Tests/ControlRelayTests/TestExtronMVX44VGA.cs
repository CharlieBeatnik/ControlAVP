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

            _deviceSettings = jsonParsed["ExtronMVX44VGA"];
        }

        public ExtronMVX44VGA CreateDevice()
        {
            return new ExtronMVX44VGA(_deviceSettings["PortId"].ToString());
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
            var device = new ExtronMVX44VGA(string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void GivenNullPartialId_WhenNewDevice_ThenExceptionThrown()
        {
            var device = new ExtronMVX44VGA(null);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void GivenInvalidPartialId_WhenNewDevice_ThenExceptionThrown()
        {
            var device = new ExtronMVX44VGA("invalid");
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public void GivenDevice_WhenCreateAnotherDeviceWithSameID_ThenExceptionThrown()
        {
            using (var device1 = CreateDevice())
            {
                using (var device2 = CreateDevice())
                {
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
    }
}
