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
using System.Diagnostics;

namespace Tests
{
    //TODO - Need to write these tests
    // Duplicate method names

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
        [ExpectedException(typeof(ArgumentNullException))]
        public void GivenJsonIsNull_WhenRunCommandProcessor_ThenArgumentNullExceptionIsThrown()
        {
            using (var device = CreateDevice())
            {
                var devices = new List<object>();
                devices.Add(device);

                foreach (var result in CommandProcessorUtils.Execute(devices, null))
                {
                }
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void GivenDevicesIsNull_WhenRunCommandProcessor_ThenArgumentNullExceptionIsThrown()
        {
            using (StreamReader r = new StreamReader(@".\TestAssets\command-processor-call-1-function.json"))
            {
                string json = r.ReadToEnd();
                foreach (var result in CommandProcessorUtils.Execute(null, json))
                {
                }
            }
        }


        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void GivenJsonIsInvalid_WhenRunCommandProcessor_ThenArgumentExceptionIsThrown()
        {
            using (StreamReader r = new StreamReader(@".\TestAssets\command-processor-fail-validation.json"))
            {
                string json = r.ReadToEnd();
                using (var device = CreateDevice())
                {
                    var devices = new List<object>();
                    devices.Add(device);

                    foreach (var result in CommandProcessorUtils.Execute(devices, json))
                    {
                    }
                }
            }
        }

        [TestMethod]
        public void GivenJsonAndDevice_WhenCallSumValuesAndReturnEquality_ThenCommandResultIsTrue()
        {
            using (StreamReader r = new StreamReader(@".\TestAssets\command-processor-call-sumvaluesandreturnequality.json"))
            {
                string json = r.ReadToEnd();
                using (var device = CreateDevice())
                {
                    var devices = new List<object>();
                    devices.Add(device);

                    foreach (var commandResult in CommandProcessorUtils.Execute(devices, json))
                    {
                        Assert.IsNotNull(commandResult.Result);
                        Assert.IsTrue(commandResult.Result.GetType() == typeof(bool));
                        Assert.IsTrue((bool)commandResult.Result);

                        Assert.IsTrue(commandResult.Success);

                        //Only time we need to check this, no need in any other tests
                        Assert.IsTrue(commandResult.Function == "SumValuesAndReturnEquality");
                        Assert.IsTrue(commandResult.Description == "Example description.");
                    }
                }
            }
        }

        [TestMethod]
        public void GivenJsonAndDevice_WhenCallSetFunctionWithAFieldTypeParameter_ThenCommandResultIsCorrect()
        {
            using (StreamReader r = new StreamReader(@".\TestAssets\command-processor-call-function-with-a-field-type-parameter.json"))
            {
                string json = r.ReadToEnd();
                using (var device = CreateDevice())
                {
                    var devices = new List<object>();
                    devices.Add(device);

                    foreach (var commandResult in CommandProcessorUtils.Execute(devices, json))
                    {
                        Assert.IsTrue(commandResult.Success);
                        Assert.IsNotNull(commandResult.Result);
                        Assert.IsTrue(commandResult.Result.GetType() == typeof(bool));
                        Assert.IsTrue((bool)commandResult.Result);
                    }
                }
            }
        }

        [TestMethod]
        public void GivenJsonAndDevice_WhenCallSetFunctionWithAPropertyTypeParameter_ThenCommandResultIsCorrect()
        {
            using (StreamReader r = new StreamReader(@".\TestAssets\command-processor-call-function-with-a-property-type-parameter.json"))
            {
                string json = r.ReadToEnd();
                using (var device = CreateDevice())
                {
                    var devices = new List<object>();
                    devices.Add(device);

                    foreach (var commandResult in CommandProcessorUtils.Execute(devices, json))
                    {
                        Assert.IsTrue(commandResult.Success);
                        Assert.IsNotNull(commandResult.Result);
                        Assert.IsTrue(commandResult.Result.GetType() == typeof(bool));
                        Assert.IsTrue((bool)commandResult.Result);
                    }
                }
            }
        }

        [TestMethod]
        public void GivenJsonAndDevice_WhenCallGetSumValuesAndReturnAnswer_ThenCommandResultIsCorrect()
        {
            using (StreamReader r = new StreamReader(@".\TestAssets\command-processor-call-sumvaluesandreturnanswer.json"))
            {
                string json = r.ReadToEnd();
                using (var device = CreateDevice())
                {
                    var devices = new List<object>();
                    devices.Add(device);

                    foreach (var commandResult in CommandProcessorUtils.Execute(devices, json))
                    {
                        Assert.IsTrue(commandResult.Success);
                        Assert.IsNotNull(commandResult.Result);
                        Assert.IsTrue((int?)commandResult.Result == 42);
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
                    foreach (var commandResult in CommandProcessorUtils.Execute(devices, json))
                    {
                        functionCallCount++;
                    }

                    Assert.IsTrue(functionCallCount == 2);
                }
            }
        }

        [TestMethod]
        public void GivenJsonAndDevice_WhenCallFunctionWithEnumParameter_ThenCommandResultIsCorrect()
        {
            using (StreamReader r = new StreamReader(@".\TestAssets\command-processor-call-function-with-enum-parameter.json"))
            {
                string json = r.ReadToEnd();
                using (var device = CreateDevice())
                {
                    var devices = new List<object>();
                    devices.Add(device);

                    foreach (var commandResult in CommandProcessorUtils.Execute(devices, json))
                    {
                        Assert.IsTrue(commandResult.Success);
                        Assert.IsTrue((DummyDeviceSetting)commandResult.Result == DummyDeviceSetting.Setting2);
                    }
                }
            }
        }

        [TestMethod]
        public void GivenJsonAndDevice_WhenExecuteCommandWith5SecondsPostWait_ThenEndTimeIs5SecondsGreaterThanOrEqualToExecutionEndTime()
        {
            using (StreamReader r = new StreamReader(@".\TestAssets\command-processor-post-wait.json"))
            {
                string json = r.ReadToEnd();
                using (var device = CreateDevice())
                {
                    var devices = new List<object>();
                    devices.Add(device);

                    foreach (var commandResult in CommandProcessorUtils.Execute(devices, json))
                    {
                        Assert.IsTrue((commandResult.EndTime - commandResult.ExecutionEndTime).TotalSeconds >= 5);
                    }
                }
            }
        }

        [TestMethod]
        public void GivenCommandsWithNoDeviceIndexOrAssembly_WhenExecuteCommands_ThenSuccessIsFalseAndResultIsNullAndDeviceIndexIsNull()
        {
            using (StreamReader r = new StreamReader(@".\TestAssets\command-processor-no-device-index-or-assembly.json"))
            {
                string json = r.ReadToEnd();
                using (var device = CreateDevice())
                {
                    var devices = new List<object>();
                    devices.Add(device);

                    foreach (var commandResult in CommandProcessorUtils.Execute(devices, json))
                    {
                        Assert.IsFalse(commandResult.Success);
                        Assert.IsNull(commandResult.Result);
                        Assert.IsNull(commandResult.DeviceIndex);
                    }
                }
            }
        }

        [TestMethod]
        public void GivenCommandsWithNoDeviceIndexOrAssemblyWithDefaults_WhenExecuteCommands_ThenSuccessIsTrueResultIsNotNullAndDeviceIndexIs0()
        {
            using (StreamReader r = new StreamReader(@".\TestAssets\command-processor-default-device-index-and-assembly.json"))
            {
                string json = r.ReadToEnd();
                using (var device = CreateDevice())
                {
                    var devices = new List<object>();
                    devices.Add(device);

                    foreach (var commandResult in CommandProcessorUtils.Execute(devices, json))
                    {
                        Assert.IsTrue(commandResult.Success);
                        Assert.IsNotNull(commandResult.Result);
                        Assert.AreEqual(0, commandResult.DeviceIndex);
                    }
                }
            }
        }

        [TestMethod]
        public void GivenJsonAndDevice_WhenExecuteCommandWithExecuteAfter5Seconds_ThenExecutionStartTimeIsGreaterThanOrEqualTo5Seconds()
        {
            using (StreamReader r = new StreamReader(@".\TestAssets\command-processor-execute-after.json"))
            {
                string json = r.ReadToEnd();
                using (var device = CreateDevice())
                {
                    var devices = new List<object>();
                    devices.Add(device);

                    foreach (var commandResult in CommandProcessorUtils.Execute(devices, json))
                    {
                        Assert.IsTrue(commandResult.ExecutionStartTime.TotalSeconds > 5);
                    }
                }
            }
        }

        [TestMethod]
        public void GivenJsonAndDevice_WhenCallFunctionThatDoesNotExist_ThenSuccessIsFalesAndResultIsNull()
        {
            using (StreamReader r = new StreamReader(@".\TestAssets\command-processor-call-function-that-does-not-exist.json"))
            {
                string json = r.ReadToEnd();
                using (var device = CreateDevice())
                {
                    var devices = new List<object>();
                    devices.Add(device);

                    foreach (var commandResult in CommandProcessorUtils.Execute(devices, json))
                    {
                        Assert.IsFalse(commandResult.Success);
                        Assert.IsNull(commandResult.Result);
                    }
                }
            }
        }

        [TestMethod]
        public void GivenJsonAndDevice_WhenCallFunctionWithMissingParameters_ThenSuccessIsFalesAndResultIsNull()
        {
            using (StreamReader r = new StreamReader(@".\TestAssets\command-processor-call-function-with-missing-parameters.json"))
            {
                string json = r.ReadToEnd();
                using (var device = CreateDevice())
                {
                    var devices = new List<object>();
                    devices.Add(device);

                    foreach (var commandResult in CommandProcessorUtils.Execute(devices, json))
                    {
                        Assert.IsFalse(commandResult.Success);
                        Assert.IsNull(commandResult.Result);
                    }
                }
            }
        }

        [TestMethod]
        public void GivenJsonAndDevice_WhenCallFunctionWithIncorrectlyNamedParameters_ThenSuccessIsFalesAndResultIsNull()
        {
            using (StreamReader r = new StreamReader(@".\TestAssets\command-processor-call-function-with-incorrectly-named-parameters.json"))
            {
                string json = r.ReadToEnd();
                using (var device = CreateDevice())
                {
                    var devices = new List<object>();
                    devices.Add(device);

                    foreach (var commandResult in CommandProcessorUtils.Execute(devices, json))
                    {
                        Assert.IsFalse(commandResult.Success);
                        Assert.IsNull(commandResult.Result);
                    }
                }
            }
        }
    }
}
