﻿using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ControllableDevice;
using System.IO;
using Newtonsoft.Json.Linq;
using System.Net;
using System.Net.NetworkInformation;
using ControllableDeviceTypes.SonySimpleIPTypes;

namespace Tests
{
    [TestClass]
    public class TestSonySimpleIP
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

            _deviceSettings = jsonParsed["Devices"]["SonySimpleIP"][0];
        }

        [ClassCleanup]
        public static void ClassCleanup()
        {
            using (var device = CreateDevice())
            {
                Assert.IsTrue(device.TurnOff());
            }
        }

        [TestInitialize()]
        public void TestInitialize()
        {
            using (var device = CreateDevice())
            {
                Assert.IsTrue(device.TurnOn());
            }
        }

        [TestCleanup()]
        public void TestCleanup()
        {
        }


        public static SonySimpleIP CreateDevice()
        {
            return new SonySimpleIP(IPAddress.Parse(_deviceSettings["host"].ToString()),
                                     PhysicalAddress.Parse(_deviceSettings["physicalAddress"].ToString()),
                                     _deviceSettings["preSharedKey"].ToString());
        }

        public static SonySimpleIP CreateInvalidIPDevice()
        {
            return new SonySimpleIP(IPAddress.Parse("192.0.2.0"),
                                     PhysicalAddress.Parse(_deviceSettings["physicalAddress"].ToString()),
                                     _deviceSettings["preSharedKey"].ToString());
        }

        [TestMethod]
        public void GivenInvalidIPDevice_WhenGetPowerStatus_ThenPowerStatusIsOff()
        {
            using (var device = CreateInvalidIPDevice())
            {
                var powerStatus = device.GetPowerStatus();
                Assert.IsTrue(powerStatus == PowerStatus.Off);
            }
        }

        [TestMethod]
        public void GivenDevice_WhenVolumeIsSetTo11_ThenVolumeIs11()
        {
            using (var device = CreateDevice())
            {
                bool success = device.SetVolume(11);
                Assert.IsTrue(success);

                int? volume = device.GetVolume();
                Assert.IsNotNull(volume);
                Assert.AreEqual(11, volume);
            }
        }

        [TestMethod]
        public void GivenDeviceVolumeIsUnmuted_WhenVolumeIsMuted_ThenVolumeIsMuted()
        {
            using (var device = CreateDevice())
            {
                bool success = device.SetMute(false);
                Assert.IsTrue(success);

                success = device.SetMute(true);
                Assert.IsTrue(success);

                bool? muted = device.GetIsMuted();
                Assert.IsNotNull(muted);
                Assert.IsTrue((bool)muted);
            }
        }

        [TestMethod]
        public void GivenTVIsOff_WhenTurnOn_ThenTVIsOn()
        {
            using (var device = CreateDevice())
            {
                Assert.IsTrue(device.TurnOff());
                Assert.IsTrue(device.TurnOn());
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
        public void GivenInvalidIPDevice_WhenCallGetFunctions_ThenResultsAreNull()
        {
            using (var device = CreateInvalidIPDevice())
            {
                Assert.IsNull(device.GetIsMuted());
                Assert.IsNull(device.GetMaxVolume());
                Assert.IsNull(device.GetMinVolume());
                Assert.IsNull(device.GetVolume());
                Assert.IsNull(device.GetInputPort());
            }
        }

        [TestMethod]
        public void GivenDevice_WhenGetMinVolume_ThenMinVolumeIs0()
        {
            using (var device = CreateDevice())
            {
                int? minVolume = device.GetMinVolume();
                Assert.IsNotNull(minVolume);
                Assert.IsTrue(minVolume == 0);
            }
        }

        [TestMethod]
        public void GivenDevice_WhenGetMaxVolume_ThenMaxVolumeIs100()
        {
            using (var device = CreateDevice())
            {
                int? maxVolume = device.GetMaxVolume();
                Assert.IsNotNull(maxVolume);
                Assert.IsTrue(maxVolume == 100);
            }
        }

        [TestMethod]
        public void GivenInputIsPortHdmi1_WhenSetInputPortToHdmi2_ThenInputPortIsHdmi2()
        {
            using (var device = CreateDevice())
            {
                //Change to HDMI1
                bool success = device.SetInputPort(InputPort.Hdmi1);
                Assert.IsTrue(success);

                //Confirm HDMI1
                var inputPort = device.GetInputPort();
                Assert.IsNotNull(inputPort);
                Assert.AreEqual(InputPort.Hdmi1, inputPort);

                //Change to HDMI2
                success = device.SetInputPort(InputPort.Hdmi2);
                Assert.IsTrue(success);

                //Confirm HDMI2
                inputPort = device.GetInputPort();
                Assert.IsNotNull(inputPort);
                Assert.AreEqual(InputPort.Hdmi2, inputPort);
            }
        }
    }
}
