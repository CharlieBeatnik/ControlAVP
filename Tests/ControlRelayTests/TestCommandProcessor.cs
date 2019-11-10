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

            _deviceSettings = jsonParsed["Devices"]["ApcAP8959EU3"][0];
        }

        public static ApcAP8959EU3 CreateDevice()
        {
            string host = _deviceSettings["host"].ToString();
            int port = int.Parse(_deviceSettings["port"].ToString());
            string username = _deviceSettings["username"].ToString();
            string password = _deviceSettings["password"].ToString();

            return new ApcAP8959EU3(host, port, username, password);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void GivenJsonIsInvalid_WhenRunCommandeProcessor_ThenArgumentExceptionIsThrown()
        {
            using (StreamReader r = new StreamReader(@".\TestAssets\command-processor-should-fail-validation.json"))
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

        //[TestMethod]
        //public void GivenJsonIsInvalid_WhenRunCommandeProcessor_ThenArgumentExceptionIsThrown()
        //{
        //    for (int i = 0; i < 10; ++i)
        //    {
        //        using (StreamReader r = new StreamReader(@".\TestAssets\command-processor-should-fail-validation.json"))
        //        {
        //            string json = r.ReadToEnd();

        //            using (var device = CreateDevice())
        //            {
        //                var devices = new List<object>();
        //                devices.Add(device);

        //                foreach (var result in CommandProcessorUtils.Process(devices, json))
        //                {
        //                    //Assert.IsTrue(result.Item2);
        //                }
        //            }
        //        }
        //    }
        //}
    }

}
