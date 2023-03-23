using Microsoft.VisualStudio.TestTools.UnitTesting;
using ControllableDevice;
using System.Net;

namespace Tests
{
    [TestClass]
    public class TestPing
    {
        [TestMethod]
        public void GivenUnavailableIP_WhenSendPing_ThenResponseIsFalse()
        {
            var result = Ping.Send(IPAddress.Parse("192.0.2.0"));
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void GivenAvailableIP_WhenSendPing_ThenResponseIsTrue()
        {
            var result = Ping.Send(IPAddress.Parse("192.168.0.1"));
            Assert.IsTrue(result);
        }

    }
}
