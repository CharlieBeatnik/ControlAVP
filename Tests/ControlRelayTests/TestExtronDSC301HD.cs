using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ControllableDevice;
using System.IO;
using Newtonsoft.Json.Linq;
using ControllableDeviceTypes.ExtronDSC301HDTypes;
using System.Threading;

namespace Tests
{
    [TestClass]
    public class TestExtronDSC301HD
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

            _deviceSettings = jsonParsed["ExtronDSC301HD"];
        }

        public ExtronDSC301HD CreateDevice()
        {
            return new ExtronDSC301HD(_deviceSettings["PortId"].ToString());
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
        public void GivenDevice_WhenGetFirmware_ThenFirmwareIsGTE1_25_1()
        {
            using (var device = CreateDevice())
            {
                var firmware = device.GetFirmware();
                Assert.IsTrue(firmware >= new Version(1, 25, 1));
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

        //ANDREWDENN_TODO: Disabled these because they can fail depending on if the scaler has a video input
        //[TestMethod]
        //public void GivenDevice_WhenGetActivePixels_ThenResultIsValid()
        //{
        //    using (var device = CreateDevice())
        //    {
        //        Assert.IsTrue(device.ActivePixels != 0);
        //    }
        //}

        //[TestMethod]
        //public void GivenDevice_WhenGetActiveLines_ThenResultIsValid()
        //{
        //    using (var device = CreateDevice())
        //    {
        //        Assert.IsTrue(device.ActiveLines != 0);
        //    }
        //}

        [TestMethod]
        public void GivenDevice_WhenSetHorizontalPosition_ThenHorizontalPositionIsTheSame()
        {
            using (var device = CreateDevice())
            {
                var result = device.SetHorizontalPosition(10);
                Assert.IsTrue(result);
                Assert.IsTrue(device.GetHorizontalPosition() == 10);

                result = device.SetHorizontalPosition(0);
                Assert.IsTrue(result);
                Assert.IsTrue(device.GetHorizontalPosition() == 0);

                result = device.SetHorizontalPosition(-10);
                Assert.IsTrue(result);
                Assert.IsTrue(device.GetHorizontalPosition() == -10);
            }
        }

        [TestMethod]
        public void GivenDevice_WhenSetVertialPosition_ThenVerticalPositionIsTheSame()
        {
            using (var device = CreateDevice())
            {
                var result = device.SetVerticalPosition(10);
                Assert.IsTrue(result);
                Assert.IsTrue(device.GetVerticalPosition() == 10);

                result = device.SetVerticalPosition(0);
                Assert.IsTrue(result);
                Assert.IsTrue(device.GetVerticalPosition() == 0);

                result = device.SetVerticalPosition(-10);
                Assert.IsTrue(result);
                Assert.IsTrue(device.GetVerticalPosition() == -10);
            }
        }

        [TestMethod]
        public void GivenDevice_WhenSetHorizontalSize_ThenHorizontalSizeIsTheSame()
        {
            using (var device = CreateDevice())
            {
                var result = device.SetHorizontalSize(10);
                Assert.IsTrue(result);
                Assert.IsTrue(device.GetHorizontalSize() == 10);
            }
        }

        [TestMethod]
        public void GivenDevice_WhenSetVerticalSize_ThenVerticalSizeIsTheSame()
        {
            using (var device = CreateDevice())
            {
                var result = device.SetVerticalSize(10);
                Assert.IsTrue(result);
                Assert.IsTrue(device.GetVerticalSize() == 10);
            }
        }

        [TestMethod]
        public void GivenDevice_WhenSetOutputRate_ThenOutputRateIsTheSame()
        {
            using (var device = CreateDevice())
            {
                var edid = Edid.GetEdid(1280, 720, 60.0f);
                Assert.IsNotNull(edid);

                var result = device.SetOutputRate(edid);
                Assert.IsTrue(result);

                var outputRate = device.GetOutputRate();
                Assert.IsTrue(outputRate == edid);
            }
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
        [ExpectedException(typeof(ArgumentException))]
        public void GivenEmptyPartialId_WhenNewDevice_ThenExceptionThrown()
        {
            var device = new ExtronDSC301HD(string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void GivenNullPartialId_WhenNewDevice_ThenExceptionThrown()
        {
            var device = new ExtronDSC301HD(null);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void GivenInvalidPartialId_WhenNewDevice_ThenExceptionThrown()
        {
            var device = new ExtronDSC301HD("invalid");
        }

        [TestMethod]
        public void GivenDevice_WhenSetPositionAndSize_ThenGetPositionAndSizeMatch()
        {
            using (var device = CreateDevice())
            {
                var posAndSizeBefore = new PositionAndSize(32, 64, 640, 480);
                var result = device.SetImagePositionAndSize(posAndSizeBefore);
                Assert.IsTrue(result);

                var posAndSizeAfter = device.GetImagePositionAndSize();
                Assert.AreEqual(posAndSizeBefore, posAndSizeAfter);
            }
        }

        [TestMethod]
        public void GivenDevice_WhenSetInputPortToHDMI_ThenInputPortIsHDMI()
        {
            using (var device = CreateDevice())
            {
                var result = device.SetInputPort(InputPort.HDMI);
                Assert.IsTrue(result);

                var inputPort = device.GetInputPort();
                Assert.IsNotNull(inputPort);
                Assert.AreEqual(InputPort.HDMI, inputPort);
            }
        }

        [TestMethod]
        public void GivenDevice_WhenGetTemperature_ThenTemperatureIsNotNull()
        {
            using (var device = CreateDevice())
            {
                var result = device.GetTemperature();
                Assert.IsNotNull(result);
            }
        }
    }
}
