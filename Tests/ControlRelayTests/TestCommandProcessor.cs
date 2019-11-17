using ControllableDevice;
using CommandProcessor;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tests
{
    [TestClass]
    public class TestCommandProcessor
    {
        [ClassInitialize]
        public static void ClassInitialize(TestContext tc)
        {
            if (tc == null)
            {
                throw new ArgumentNullException(nameof(tc));
            }
        }

        public static DummyDevice CreateDevice()
        {
            return new DummyDevice();
        }

        public static DummyDevice CreateInvalidDevice()
        {
            return new DummyDevice(true);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void GivenJsonIsInvalid_WhenRunCommandeProcessor_ThenArgumentExceptionIsThrown()
        {
            using (StreamReader r = new StreamReader(@".\TestAssets\command-processor-fail-validation.json"))
            {
                string json = r.ReadToEnd();
                using (var device = CreateDevice())
                {
                    var devices = new List<object>();
                    devices.Add(device);

                    foreach (var result in CommandProcessorUtils.Process(devices, json))
                    {
                    }
                }
            }
        }

        [TestMethod]
        public void GivenJsonAndDevice_WhenCallSetFunction_ThenCommandResultIsCorrect()
        {
            using (StreamReader r = new StreamReader(@".\TestAssets\command-processor-call-set-function.json"))
            {
                string json = r.ReadToEnd();
                using (var device = CreateDevice())
                {
                    var devices = new List<object>();
                    devices.Add(device);

                    foreach (var commandResult in CommandProcessorUtils.Process(devices, json))
                    {
                        Assert.IsTrue(commandResult.Success);
                        Assert.IsNotNull(commandResult.Result);
                        Assert.IsTrue(commandResult.FunctionName == "SetSomething");
                    }
                }
            }
        }

        [TestMethod]
        public void GivenJsonAndDevice_WhenCallGetFunction_ThenCommandResultIsCorrect()
        {
            using (StreamReader r = new StreamReader(@".\TestAssets\command-processor-call-get-function.json"))
            {
                string json = r.ReadToEnd();
                using (var device = CreateDevice())
                {
                    var devices = new List<object>();
                    devices.Add(device);

                    foreach (var commandResult in CommandProcessorUtils.Process(devices, json))
                    {
                        Assert.IsTrue(commandResult.Success);
                        Assert.IsNotNull(commandResult.Result);
                        Assert.IsTrue((int?)commandResult.Result == 0);
                        Assert.IsTrue(commandResult.FunctionName == "GetSomething");
                    }
                }
            }
        }

        [TestMethod]
        public void GivenJsonAndDevice_WhenCall2Functions_ThenFunctionCallCountIs2()
        {
            using (StreamReader r = new StreamReader(@".\TestAssets\command-processor-call-2-functions.json"))
            {
                string json = r.ReadToEnd();
                using (var device = CreateDevice())
                {
                    var devices = new List<object>();
                    devices.Add(device);

                    int functionCallCount = 0;
                    foreach (var commandResult in CommandProcessorUtils.Process(devices, json))
                    {
                        functionCallCount++;
                        Assert.IsTrue(commandResult.Success);
                        Assert.IsNotNull(commandResult.Result);
                        Assert.IsTrue((int?)commandResult.Result == 0);
                        Assert.IsTrue(commandResult.FunctionName != null);
                    }

                    Assert.IsTrue(functionCallCount == 2);
                }
            }
        }
    }
}
