﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using Renci.SshNet.Security.Cryptography.Ciphers.Modes;
using Renci.SshNet.Tests.Common;

namespace Renci.SshNet.Tests.Classes.Security.Cryptography.Ciphers.Modes
{
    /// <summary>
    ///This is a test class for CtrCipherModeTest and is intended
    ///to contain all CtrCipherModeTest Unit Tests
    ///</summary>
    [TestClass()]
    public class CtrCipherModeTest : TestBase
    {
        /// <summary>
        ///A test for EncryptBlock
        ///</summary>
        [TestMethod]
        [Ignore] // placeholder for actual test
        public void EncryptBlockTest()
        {
            byte[] iv = null; // TODO: Initialize to an appropriate value
            CtrCipherMode target = new CtrCipherMode(iv); // TODO: Initialize to an appropriate value
            byte[] inputBuffer = null; // TODO: Initialize to an appropriate value
            int inputOffset = 0; // TODO: Initialize to an appropriate value
            int inputCount = 0; // TODO: Initialize to an appropriate value
            byte[] outputBuffer = null; // TODO: Initialize to an appropriate value
            int outputOffset = 0; // TODO: Initialize to an appropriate value
            int expected = 0; // TODO: Initialize to an appropriate value
            int actual;
            actual = target.EncryptBlock(inputBuffer, inputOffset, inputCount, outputBuffer, outputOffset);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for DecryptBlock
        ///</summary>
        [TestMethod]
        [Ignore] // placeholder for actual test
        public void DecryptBlockTest()
        {
            byte[] iv = null; // TODO: Initialize to an appropriate value
            CtrCipherMode target = new CtrCipherMode(iv); // TODO: Initialize to an appropriate value
            byte[] inputBuffer = null; // TODO: Initialize to an appropriate value
            int inputOffset = 0; // TODO: Initialize to an appropriate value
            int inputCount = 0; // TODO: Initialize to an appropriate value
            byte[] outputBuffer = null; // TODO: Initialize to an appropriate value
            int outputOffset = 0; // TODO: Initialize to an appropriate value
            int expected = 0; // TODO: Initialize to an appropriate value
            int actual;
            actual = target.DecryptBlock(inputBuffer, inputOffset, inputCount, outputBuffer, outputOffset);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for CtrCipherMode Constructor
        ///</summary>
        [TestMethod]
        [Ignore] // placeholder for actual test
        public void CtrCipherModeConstructorTest()
        {
            byte[] iv = null; // TODO: Initialize to an appropriate value
            CtrCipherMode target = new CtrCipherMode(iv);
            Assert.Inconclusive("TODO: Implement code to verify target");
        }
    }
}
