using Microsoft.VisualStudio.TestTools.UnitTesting;
using PduDevice;
using System.Linq;

namespace Tests
{
    [TestClass]
    public class TestApcAP8959EU3
    {
        private string _host = "192.168.0.75";
        private int _port = 5222;
        private string _username = "relayapp";
        private string _password = "6RRkfax1Fnsv[caIypPEId";
        private string _terminalPrompt = "apc>";


        public TestApcAP8959EU3()
        {

        }

        private ApcAP8959EU3 GetDevice()
        {
            var sshClient = new PduSshClient();
            sshClient.Connect(_host, _port, _username, _password, _terminalPrompt);
            return new ApcAP8959EU3(sshClient);
        }

        [TestMethod]
        public void GivenDevice_WhenGetOutlets_ThenOutletCountIs24()
        {
            var device = GetDevice();
            var outlets = device.GetOutlets();
            Assert.IsTrue(outlets.Count() == 24);
        }

        [TestMethod]
        public void GivenInvalidHost_WhenSshClientConnects_ResultIsFalse()
        {
            var sshClient = new PduSshClient();
            var result = sshClient.Connect("0.0.0.0", _port, _username, _password, _terminalPrompt);
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void GivenInvalidPort_WhenSshClientConnects_ResultIsFalse()
        {
            var sshClient = new PduSshClient();
            var result = sshClient.Connect(_host, 0, _username, _password, _terminalPrompt);
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void GivenInvalidUsername_WhenSshClientConnects_ResultIsFalse()
        {
            var sshClient = new PduSshClient();
            var result = sshClient.Connect(_host, _port, "", _password, _terminalPrompt);
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void GivenInvalidPassword_WhenSshClientConnects_ResultIsFalse()
        {
            var sshClient = new PduSshClient();
            var result = sshClient.Connect(_host, _port, _username, "", _terminalPrompt);
            Assert.IsFalse(result);
        }
    }
}
