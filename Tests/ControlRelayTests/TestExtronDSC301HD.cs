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
        private static ExtronDSC301HD _device = null;
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

        [ClassCleanup]
        public static void ClassCleanup()
        {
        }

        [TestInitialize]
        public void TestInitialize()
        {
            _device = new ExtronDSC301HD(_deviceSettings["PortId"].ToString());
        }

        [TestCleanup]
        public void TestCleanup()
        {
            _device.Dispose();
            _device = null;
        }

        [TestMethod]
        public void GivenDevice_WhenGetFirmware_ThenFirmwareIsNotNull()
        {
            var firmware = _device.GetFirmware();
            Assert.IsNotNull(firmware);
        }

        [TestMethod]
        public void GivenDevice_WhenGetFirmware_ThenFirmwareIsGTE1_25_1()
        {
            var firmware = _device.GetFirmware();
            Assert.IsTrue(firmware >= new Version(1, 25, 1));
        }

        [TestMethod]
        public void GivenDevice_WhenCallAvailable_ThenDeviceIsAvailable()
        {
            Assert.IsTrue(_device.GetAvailable());
        }

        //ANDREWDENN_TODO: Disabled these because they can fail depending on if the scaler has a video input
        //[TestMethod]
        //public void GivenDevice_WhenGetActivePixels_ThenResultIsValid()
        //{
        //    Assert.IsTrue(_device.ActivePixels != 0);
        //}

        //[TestMethod]
        //public void GivenDevice_WhenGetActiveLines_ThenResultIsValid()
        //{
        //    Assert.IsTrue(_device.ActiveLines != 0);
        //}

        [TestMethod]
        public void GivenDevice_WhenSetHorizontalPosition_ThenHorizontalPositionIsTheSame()
        {
            _device.HorizontalPosition = 10;
            Assert.IsTrue(_device.HorizontalPosition == 10);

            _device.HorizontalPosition = 0;
            Assert.IsTrue(_device.HorizontalPosition == 0);

            _device.HorizontalPosition = -10;
            Assert.IsTrue(_device.HorizontalPosition == -10);
        }

        [TestMethod]
        public void GivenDevice_WhenSetVertialPosition_ThenVerticalPositionIsTheSame()
        {
            _device.VerticalPosition = 10;
            Assert.IsTrue(_device.VerticalPosition == 10);

            _device.VerticalPosition = 0;
            Assert.IsTrue(_device.VerticalPosition == 0);

            _device.VerticalPosition = -10;
            Assert.IsTrue(_device.VerticalPosition == -10);
        }

        [TestMethod]
        public void GivenDevice_WhenSetHorizontalSize_ThenHorizontalSizeIsTheSame()
        {
            _device.HorizontalSize = 10;
            Assert.IsTrue(_device.HorizontalSize == 10);
        }

        [TestMethod]
        public void GivenDevice_WhenSetVerticalSize_ThenVerticalSizeIsTheSame()
        {
            _device.VerticalSize = 10;
            Assert.IsTrue(_device.VerticalSize == 10);
        }

        [TestMethod]
        public void GivenDevice_WhenSetOutputRate_ThenOutputRateIsTheSame()
        {
            var edid = Edid.GetEdid(1280, 720, 60.0f);
            Assert.IsNotNull(edid);

            _device.OutputRate = edid;

            var outputRate = _device.OutputRate;
            Assert.IsTrue(outputRate == edid);
        }
    }
}
