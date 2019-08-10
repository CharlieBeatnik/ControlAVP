using Newtonsoft.Json.Linq;
using System;
using System.Net;
using System.Net.NetworkInformation;

namespace ControllableDevice
{
    public class SonyKDL60W855 : IControllableDevice
    {
        private bool _disposed = false;
        private JsonRpcDevice _jsonRpcDevice;

        private IPAddress _host;
        private  PhysicalAddress _physicalAddress;
        private string _preSharedKey;

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

        private JObject GetVolumeInformation()
        {
            JObject jsonIn = GetDefaultJsonPayload();
            jsonIn.Add(new JProperty("method", "getVolumeInformation"));
            jsonIn.Add(new JProperty("params", new JArray(string.Empty)));

            return _jsonRpcDevice.Post(jsonIn, "sony/audio");
        }

        public bool GetAvailable()
        {
            //ANDREWDENN_TODO
            return false;
        }

        public bool TurnOn()
        {
            WakeOnLan.WakeUp(_physicalAddress.ToString());

            //ANDREWDENN_TODO: Need a way of confirming this succeeded
            return true;
        }

        public bool TurnOff()
        {
            return false;
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
            JObject jsonIn = GetDefaultJsonPayload();
            jsonIn.Add(new JProperty("method", "setAudioVolume"));

            var parameters = new JArray(
                new JObject(
                    new JProperty("target", "speaker"),
                    new JProperty("volume", volume.ToString())
                )
            );
            jsonIn.Add(new JProperty("params", parameters));

            var jsonOut = _jsonRpcDevice.Post(jsonIn, "sony/audio");

            //ANDREWDE_TODO
            return true;
        }

    }
}