using Microsoft.VisualStudio.TestTools.UnitTesting;
using AudioVideoDevice;

namespace Tests
{
    [TestClass]
    public class TestAtenVS0801H
    {
        private static AtenVS0801H _device = null;

        public TestAtenVS0801H()
        {
            if (_device == null)
            {
                _device = new AtenVS0801H("AK05UVF8A");
            }
        }

        [TestMethod]
        public void GivenInputPortIsPort1_WhenGoToNextInput_ThenInputPortIsPort2()
        {
            Assert.IsTrue(_device.SetInput(AtenVS0801H.InputPort.Port1));
            Assert.IsTrue(_device.GoToNextInput());

            var state = _device.GetState();
            Assert.IsTrue(state != null);
            Assert.IsTrue(state.InputPort == AtenVS0801H.InputPort.Port2);
        }

        [TestMethod]
        public void GivenInputPortIsPort2_WhenGoToPreviousInput_ThenInputPortIsPort1()
        {
            Assert.IsTrue(_device.SetInput(AtenVS0801H.InputPort.Port2));
            Assert.IsTrue(_device.GoToPreviousInput());

            var state = _device.GetState();
            Assert.IsTrue(state != null);
            Assert.IsTrue(state.InputPort == AtenVS0801H.InputPort.Port1);
        }

        [TestMethod]
        public void GivenInputPortIsPort1_WhenSetInputPort2_ThenInputPortIsPort2()
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
