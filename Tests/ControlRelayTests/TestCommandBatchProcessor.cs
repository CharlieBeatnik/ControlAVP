using ControllableDevice;
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
    public class TestCommandBatchProcessor
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

            _deviceSettings = jsonParsed["Devices"]["AtenVS0801H"][0];
        }

        public static AtenVS0801H CreateDevice()
        {
            return new AtenVS0801H(_deviceSettings["portId"].ToString());
        }


        [TestMethod]
        public void SimpleCommandBatchProcessorTest()
        {
            using (StreamReader r = new StreamReader(@".\TestAssets\commandbatch1.json"))
            {
                string json = r.ReadToEnd();

                using(var device = CreateDevice())
                {
                    var devices = new List<object>();
                    devices.Add(device);
                    ControlRelay.CommandBatchProcessor.ProcessBatch(devices, json);
                }
            }
        }
    }

}
