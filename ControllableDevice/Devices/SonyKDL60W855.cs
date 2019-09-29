using ControllableDeviceTypes.SonyKDL60W855Types;
using Newtonsoft.Json.Linq;
using System;
using System.Net;
using System.Net.NetworkInformation;
using System.Threading;

namespace ControllableDevice
{
    public class SonyKDL60W855 : IControllableDevice
    {
        private bool _disposed = false;
        private JsonRpcDevice _jsonRpcDevice;

        private IPAddress _host;
        private  PhysicalAddress _physicalAddress;
        private string _preSharedKey;

        private readonly TimeSpan _fromColdBootToOnWait = TimeSpan.FromSeconds(15);
        private readonly TimeSpan _fromStandbyToOnWait = TimeSpan.FromSeconds(1);
        private readonly TimeSpan _fromOnToStandbyWait = TimeSpan.FromSeconds(1);

        public SonyKDL60W855(IPAddress host, PhysicalAddress physicalAddress, string preSharedKey)
        {
            _host = host;
            _physicalAddress = physicalAddress;
            _preSharedKey = preSharedKey;

            _jsonRpcDevice = new JsonRpcDevice(host, preSharedKey);
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

        private JObject GetDefaultJsonPayload()
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

        private JObject GetVolumeInformation()
        {
            return CallMethod("getVolumeInformation", "sony/audio");
        }

        public PowerStatus GetPowerStatus()
        {
            PowerStatus powerStatus = PowerStatus.Off;
            var result = CallMethod("getPowerStatus", "sony/system");

            if(result != null)
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
            //ANDREWDENN_TODO
            return false;
        }

        public bool TurnOn()
        {
            var powerStatus = GetPowerStatus();

            switch(powerStatus)
            {
                case PowerStatus.Off:
                    {
                        WakeOnLan.WakeUp(_physicalAddress.ToString());

                        //TV takes a long time to boot from cold, so wait for it to become responsive
                        Thread.Sleep(_fromColdBootToOnWait);
                        break;
                    }
                case PowerStatus.Standby:
                    {
                        var parameters = new JArray(
                            new JObject(
                                new JProperty("status", true)
                            )
                        );

                        CallMethod("setPowerStatus", "sony/system", parameters);
                        Thread.Sleep(_fromStandbyToOnWait);
                        break;
                    }
            }

            powerStatus = GetPowerStatus();
            return (powerStatus == PowerStatus.On);
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

                        CallMethod("setPowerStatus", "sony/system", parameters);
                        Thread.Sleep(_fromOnToStandbyWait);
                        break;
                    }
            }

            powerStatus = GetPowerStatus();
            return (powerStatus == PowerStatus.Off) || (powerStatus == PowerStatus.Standby);
        }

        public int GetVolume()
        {
            var volumeInfo = GetVolumeInformation();
            return (int)volumeInfo["result"][0][0]["volume"];
        }

        public int GetMaxVolume()
        {
            var volumeInfo = GetVolumeInformation();
            return (int)volumeInfo["result"][0][0]["maxVolume"];
        }

        public int GetMinVolume()
        {
            var volumeInfo = GetVolumeInformation();
            return (int)volumeInfo["result"][0][0]["minVolume"];
        }

        public bool GetIsMuted()
        {
            var volumeInfo = GetVolumeInformation();
            return (bool)volumeInfo["result"][0][0]["mute"];
        }

        public bool SetVolume(int volume)
        {
            var parameters = new JArray(
                new JObject(
                    new JProperty("target", "speaker"),
                    new JProperty("volume", volume.ToString())
                )
            );

            CallMethod("setAudioVolume", "sony/audio", parameters);
            return GetVolume() == volume;
        }

    }
}