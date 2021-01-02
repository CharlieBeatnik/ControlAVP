using ControllableDeviceTypes.SonyKDL60W855Types;
using Newtonsoft.Json.Linq;
using System;
using System.Diagnostics;
using System.Net;
using System.Net.NetworkInformation;
using System.Text.RegularExpressions;
using System.Threading;

namespace ControllableDevice
{
    public class SonyKDL60W855 : IControllableDevice
    {
        private bool _disposed;
        private JsonRpcDevice _jsonRpcDevice;

        private IPAddress _host;
        private  PhysicalAddress _physicalAddress;
        private string _preSharedKey;

        private readonly TimeSpan _fromColdBootToOnTimeout = TimeSpan.FromSeconds(30);
        private readonly TimeSpan _fromStandbyToOnWait = TimeSpan.FromSeconds(1);
        private readonly TimeSpan _fromOnToStandbyWait = TimeSpan.FromSeconds(1);

        public SonyKDL60W855(IPAddress host, PhysicalAddress physicalAddress, string preSharedKey)
        {
            _host = host;
            _physicalAddress = physicalAddress;
            _preSharedKey = preSharedKey;

            _jsonRpcDevice = new JsonRpcDevice(host, preSharedKey, TimeSpan.MaxValue);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                _jsonRpcDevice.Dispose();
            }

            _disposed = true;
        }

        private static JObject GetDefaultJsonPayload()
        {
            return new JObject(
                      new JProperty("id", 20),
                      new JProperty("version", "1.0")
                      );
        }

        private JObject CallMethod(string methodName, string url, JArray parameters = null)
        {
            if (parameters == null)
            {
                parameters = new JArray(string.Empty);
            }
            JObject jsonIn = GetDefaultJsonPayload();
            jsonIn.Add(new JProperty("method", methodName));
            jsonIn.Add(new JProperty("params", parameters));

            return _jsonRpcDevice.Post(jsonIn, url);
        }

        private static bool ResultIsSuccessful(JObject result)
        {
            if (result == null) return false;
            if (result["error"] != null) return false;

            return true;
        }

        private JObject GetVolumeInformation()
        {
            return CallMethod("getVolumeInformation", "sony/audio");
        }

        public PowerStatus? GetPowerStatus()
        {
            PowerStatus powerStatus = PowerStatus.Off;
            var result = CallMethod("getPowerStatus", "sony/system");

            if(ResultIsSuccessful(result))
            {
                string status = result["result"][0]["status"].ToString();
                switch (status)
                {
                    case "active":  powerStatus = PowerStatus.On;  break;
                    case "standby": powerStatus = PowerStatus.Standby; break;
                }
            }

            return powerStatus;
        }

        public bool GetAvailable()
        {
            return true;
        }

        public bool TurnOn(bool wait = true)
        {
            //Regardless of state, issue the WakeOnLan as cold boots take the most time
            WakeOnLan.WakeUp(_physicalAddress.ToString());

            var powerStatus = GetPowerStatus();

            switch(powerStatus)
            {
                case PowerStatus.Off:
                    {
                        if (wait)
                        {
                            //Loop until the TV has booted from cold or we hit a timeout
                            var sw = new Stopwatch();
                            sw.Start();
                            while (sw.ElapsedMilliseconds < _fromColdBootToOnTimeout.TotalMilliseconds)
                            {
                                powerStatus = GetPowerStatus();
                                if (powerStatus == PowerStatus.On)
                                {
                                    return true;
                                }
                            }
                            sw.Stop();

                            return false;
                        }
                        else return true;
                    }
                case PowerStatus.Standby:
                    {
                        var parameters = new JArray(
                            new JObject(
                                new JProperty("status", true)
                            )
                        );

                        var result = CallMethod("setPowerStatus", "sony/system", parameters);

                        if(wait)
                        {
                            Thread.Sleep(_fromStandbyToOnWait);
                        }
                        return ResultIsSuccessful(result);
                    }
            }

            return true;
        }

        public bool TurnOff()
        {
            var powerStatus = GetPowerStatus();

            switch (powerStatus)
            {
                case PowerStatus.On:
                    {
                        var parameters = new JArray(
                            new JObject(
                                new JProperty("status", false)
                            )
                        );

                        var result = CallMethod("setPowerStatus", "sony/system", parameters);
                        Thread.Sleep(_fromOnToStandbyWait);
                        return ResultIsSuccessful(result);
                    }
            }

            return true;
        }

        public int? GetVolume()
        {
            var volumeInfo = GetVolumeInformation();
            if (ResultIsSuccessful(volumeInfo))
            {
                return (int)volumeInfo["result"][0][0]["volume"];
            }
            else return null;
        }

        public int? GetMaxVolume()
        {
            var volumeInfo = GetVolumeInformation();
            if(ResultIsSuccessful(volumeInfo))
            {
                return (int)volumeInfo["result"][0][0]["maxVolume"];
            }
            else return null;
        }

        public int? GetMinVolume()
        {
            var volumeInfo = GetVolumeInformation();
            if (ResultIsSuccessful(volumeInfo))
            {
                return (int)volumeInfo["result"][0][0]["minVolume"];
            }
            else return null;
        }

        public bool? GetIsMuted()
        {
            var volumeInfo = GetVolumeInformation();
            if (ResultIsSuccessful(volumeInfo))
            {
                return (bool)volumeInfo["result"][0][0]["mute"];
            }
            else return null;
        }

        public bool SetVolume(int volume)
        {
            var parameters = new JArray(
                new JObject(
                    new JProperty("target", "speaker"),
                    new JProperty("volume", volume.ToString())
                )
            );

            var result = CallMethod("setAudioVolume", "sony/audio", parameters);
            return ResultIsSuccessful(result);
        }

        public bool SetMute(bool enable)
        {
            var parameters = new JArray(
                new JObject(
                    new JProperty("status", enable)
                )
            );

            var result = CallMethod("setAudioMute", "sony/audio", parameters);
            return ResultIsSuccessful(result);
        }

        public InputPort? GetInputPort()
        {
            var result = CallMethod("getPlayingContentInfo", "sony/avContent");

            if (ResultIsSuccessful(result))
            {
                //Example of playing content uri
                //extInput:hdmi?port=1
                string uri = result["result"][0]["uri"].ToString();

                Match match = Regex.Match(uri, @"^.+:(.+)\?port=([0-9]+)$");
                if (match.Success)
                {
                    string portType = match.Groups[1].Value;
                    string portId = match.Groups[2].Value;

                    InputPort inputPort;
                    if (Enum.TryParse($"{portType}{portId}", true, out inputPort))
                    {
                        return inputPort;
                    }
                }
            }

            return null;
        }

        public bool SetInputPort(InputPort inputPort)
        {
            //Build playing content uri
            //Example of playing content uri
            //extInput:hdmi?port=1
            Match match = Regex.Match(inputPort.ToString(), @"^(.+)([0-9]+)$");
            if (match.Success)
            {
                string portType = match.Groups[1].Value.ToLower();
                string portId = match.Groups[2].Value;

                string uri = $@"extInput:{portType}?port={portId}";

                var parameters = new JArray(
                new JObject(
                        new JProperty("uri", uri)
                    )
                );

                var result = CallMethod("setPlayContent", "sony/avContent", parameters);
                return ResultIsSuccessful(result);
            }

            return false;
        }
    }
}