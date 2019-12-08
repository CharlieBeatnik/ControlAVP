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

            _deviceSettings = jsonParsed["Devices"]["ExtronDSC301HD"][0];
        }

        public static ExtronDSC301HD CreateDevice()
        {
            return new ExtronDSC301HD(_deviceSettings["portId"].ToString());
        }

        public static ExtronDSC301HD CreateInvalidDevice()
        {
            return new ExtronDSC301HD("invalid");
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
        [ExpectedException(typeof(ArgumentNullException))]
        public void GivenDevice_WhenSetOutputRateWithNullEdid_ThenArgumentNullExceptionThrown()
        {
            using (var device = CreateDevice())
            {
                var result = device.SetOutputRate(null);
            }
        }

        [TestMethod]
        public void GivenDevice_WhenSetOutputRateWithValues_ThenResultsIsTrue()
        {
            using (var device = CreateDevice())
            {
                Assert.IsTrue(device.SetOutputRateWithoutEdid(1280, 720, 60.0f));
            }
        }

        [TestMethod]
        public void GivenDevice_WhenSetOutputRateWithInvalidValues_ThenResultsIsFalse()
        {
            using (var device = CreateInvalidDevice())
            {
                Assert.IsFalse(device.SetOutputRateWithoutEdid(0, 0, 0.0f));
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
            using (var device = new ExtronDSC301HD(string.Empty))
            {
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void GivenNullPartialId_WhenNewDevice_ThenExceptionThrown()
        {
            using (var device = new ExtronDSC301HD(null))
            {
            }
        }

        [TestMethod]
        public void GivenInvalidPartialId_WhenNewDevice_ThenDeviceIsNotAvailable()
        {
            using (var device = new ExtronDSC301HD("invalid"))
            {
                Assert.IsFalse(device.GetAvailable());
            }
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
        [ExpectedException(typeof(ArgumentNullException))]
        public void GivenDevice_WhenSetPositionAndSizeWithNull_ThenArgumentNullExceptionThrown()
        {
            using (var device = CreateDevice())
            {
                var result = device.SetImagePositionAndSize(null);
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

        [TestMethod]
        public void GivenInvalidDevice_WhenGetAvailable_ThenDeviceIsNotAvailable()
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
        public void GivenInvalidDevice_WhenScale_ThenResultsIsFalse()
        {
            using (var device = CreateInvalidDevice())
            {
                Assert.IsFalse(device.Scale(ScaleType.Fit, PositionType.Centre, AspectRatio.RatioPreserve));
            }
        }

        [TestMethod]
        public void GivenInvalidDevice_WhenGetActivePixels_ThenResultsIsNull()
        {
            using (var device = CreateInvalidDevice())
            {
                Assert.IsNull(device.GetActivePixels());
            }
        }

        [TestMethod]
        public void GivenInvalidDevice_WhenGetActiveLines_ThenResultsIsNull()
        {
            using (var device = CreateInvalidDevice())
            {
                Assert.IsNull(device.GetActiveLines());
            }
        }

        [TestMethod]
        public void GivenInvalidDevice_WhenGetHorizontalPosition_ThenResultsIsNull()
        {
            using (var device = CreateInvalidDevice())
            {
                Assert.IsNull(device.GetHorizontalPosition());
            }
        }

        [TestMethod]
        public void GivenInvalidDevice_WhenSetHorizontalPosition_ThenResultsIsFalse()
        {
            using (var device = CreateInvalidDevice())
            {
                Assert.IsFalse(device.SetHorizontalPosition(PositionAndSize.HPosMin));
            }
        }

        [TestMethod]
        public void GivenInvalidDevice_WhenGetVerticalPosition_ThenResultsIsNull()
        {
            using (var device = CreateInvalidDevice())
            {
                Assert.IsNull(device.GetVerticalPosition());
            }
        }

        [TestMethod]
        public void GivenInvalidDevice_WhenSetVerticalPosition_ThenResultsIsFalse()
        {
            using (var device = CreateInvalidDevice())
            {
                Assert.IsFalse(device.SetVerticalPosition(PositionAndSize.VPosMin));
            }
        }

        [TestMethod]
        public void GivenInvalidDevice_WhenGetHorizontalSize_ThenResultsIsNull()
        {
            using (var device = CreateInvalidDevice())
            {
                Assert.IsNull(device.GetHorizontalSize());
            }
        }

        [TestMethod]
        public void GivenInvalidDevice_WhenSetHorizontalSize_ThenResultsIsFalse()
        {
            using (var device = CreateInvalidDevice())
            {
                Assert.IsFalse(device.SetHorizontalSize(PositionAndSize.HSizeMin));
            }
        }

        [TestMethod]
        public void GivenInvalidDevice_WhenGetVerticalSize_ThenResultsIsNull()
        {
            using (var device = CreateInvalidDevice())
            {
                Assert.IsNull(device.GetVerticalSize());
            }
        }

        [TestMethod]
        public void GivenInvalidDevice_WhenSetVerticalSize_ThenResultsIsFalse()
        {
            using (var device = CreateInvalidDevice())
            {
                Assert.IsFalse(device.SetVerticalSize(PositionAndSize.VSizeMin));
            }
        }

        [TestMethod]
        public void GivenInvalidDevice_WhenGetOutputRate_ThenResultsIsNull()
        {
            using (var device = CreateInvalidDevice())
            {
                Assert.IsNull(device.GetOutputRate());
            }
        }

        [TestMethod]
        public void GivenInvalidDevice_WhenSetOutputRate_ThenResultsIsFalse()
        {
            using (var device = CreateInvalidDevice())
            {
                var edid = Edid.GetEdid(1280, 720, 60.0f);
                Assert.IsNotNull(edid);
                Assert.IsFalse(device.SetOutputRate(edid));
            }
        }

        [TestMethod]
        public void GivenInvalidDevice_WhenSetOutputRateWithValues_ThenResultsIsFalse()
        {
            using (var device = CreateInvalidDevice())
            {
                Assert.IsFalse(device.SetOutputRateWithoutEdid(1280, 720, 60.0f));
            }
        }

        [TestMethod]
        public void GivenInvalidDevice_WhenGetImagePositionAndSize_ThenResultsIsNull()
        {
            using (var device = CreateInvalidDevice())
            {
                Assert.IsNull(device.GetImagePositionAndSize());
            }
        }

        [TestMethod]
        public void GivenInvalidDevice_WhenSetImagePositionAndSize_ThenResultsIsFalse()
        {
            using (var device = CreateInvalidDevice())
            {
                var posAndSize = new PositionAndSize(32, 64, 640, 480);
                Assert.IsFalse(device.SetImagePositionAndSize(posAndSize));
            }
        }

        [TestMethod]
        public void GivenInvalidDevice_WhenSetInputPort_ThenResultsIsFalse()
        {
            using (var device = CreateInvalidDevice())
            {
                Assert.IsFalse(device.SetInputPort(InputPort.Composite));
            }
        }

        [TestMethod]
        public void GivenInvalidDevice_WhenGetInputPort_ThenResultsIsNull()
        {
            using (var device = CreateInvalidDevice())
            {
                Assert.IsNull(device.GetInputPort());
            }
        }

        [TestMethod]
        public void GivenInvalidDevice_WhenGetTemperature_ThenResultsIsNull()
        {
            using (var device = CreateInvalidDevice())
            {
                Assert.IsNull(device.GetTemperature());
            }
        }

        [TestMethod]
        public void GivenDevice_WhenSetDetailFilterTo32_ThenResultIs32()
        {
            using(var device = CreateDevice())
            {
                bool success = device.SetDetailFilter(32);
                Assert.IsTrue(success);
                var result = device.GetDetailFilter();
                Assert.IsTrue(result == 32);
                device.SetDetailFilterDefault();
            }
        }

        [TestMethod]
        public void GivenDevice_WhenGetDetailFilter_ThenResultIsNull()
        {
            using (var device = CreateInvalidDevice())
            {
                var result = device.GetDetailFilter();
                Assert.IsNull(result);
            }
        }
    }
}
