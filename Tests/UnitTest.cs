
using System;
using NUnit;
using AudioVideo;
using NUnit.Framework;

namespace Tests
{
    [TestFixture]
    public class TestAtenVS0801H
    {
        private AtenVS0801H _device;

        public TestAtenVS0801H()
        {
            _device = new AtenVS0801H("AK05UVF8A");
        }

        [Test]
        public void GivenInputPortIsPort1_WhenGoToNextInput_InputPortIsPort2()
        {
            Assert.IsTrue(_device.SetInput(AtenVS0801H.InputPort.Port1));
            Assert.IsTrue(_device.GoToNextInput());

            var state = _device.GetState();
            Assert.IsTrue(state != null);
            Assert.IsTrue(state.InputPort == AtenVS0801H.InputPort.Port2);
        }

        [Test]
        public void GivenInputPortIsPort2_WhenGoToPreviousInput_InputPortIsPort1()
        {
            Assert.IsTrue(_device.SetInput(AtenVS0801H.InputPort.Port2));
            Assert.IsTrue(_device.GoToPreviousInput());

            var state = _device.GetState();
            Assert.IsTrue(state != null);
            Assert.IsTrue(state.InputPort == AtenVS0801H.InputPort.Port1);
        }

        [Test]
        public void GivenInputPortIsPort1_WhenSetInputPort2_InputPortIsPort2()
        {
            Assert.IsTrue(_device.SetInput(AtenVS0801H.InputPort.Port1));
            var state = _device.GetState();
            Assert.IsTrue(state != null);
            Assert.IsTrue(state.InputPort == AtenVS0801H.InputPort.Port1);

            Assert.IsTrue(_device.SetInput(AtenVS0801H.InputPort.Port2));
            state = _device.GetState();
            Assert.IsTrue(state != null);
            Assert.IsTrue(state.InputPort == AtenVS0801H.InputPort.Port2);
        }
    }
}
