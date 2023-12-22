using Microsoft.Azure.Devices;
using ControllableDeviceTypes.SerialBlasterTypes;

namespace AVPCloudToDevice
{
    public class SerialBlaster(ServiceClient serviceClient, string deviceId, uint deviceIndex)
    {
        private readonly ServiceClient _serviceClient = serviceClient;
        private readonly string _deviceId = deviceId;
        private readonly uint _deviceIndex = deviceIndex;

        public bool GetAvailable()
        {
            try
            {
                var payload = new { _deviceIndex };

                var response = Utilities.InvokeMethodWithObjectPayload(_serviceClient, _deviceId, "SerialBlasterGetAvailable", payload);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool SendMessage(string message)
        {
            try
            {
                var payload = new{ _deviceIndex, message};

                var response = Utilities.InvokeMethodWithObjectPayload(_serviceClient, _deviceId, "SerialBlasterSendMessage", payload);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool SendCommand(Protocol protocol, uint command, uint repeats)
        {
            try
            {
                var payload = new { _deviceIndex, protocol, command, repeats };

                var response = Utilities.InvokeMethodWithObjectPayload(_serviceClient, _deviceId, "SerialBlasterSendCommand", payload);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static uint ConvertRawHexToNecHex(string rawHex)
        {
            // How to convert from raw hex to Nec hex codes
            // "Starting at the seventh block (after the lead in) you'll see "0015 0015" (this is for a binary 0) or
            // "0015 0040" (this is for a binary 1), do this till you have all 32 bits, the last numbers are a lead out."

            // How to convert from Nec hex to raw hex codes
            // https://www.yamaha.com/ypab/irhex_converter.asp

            uint command = 0;

            if (rawHex != null)
            {
                string[] tokens = rawHex.Split(' ');

                // Raw hex codes should contain 76 segments
                if (tokens.Length != 76) return 0;

                for (int i = 0; i < 32; ++i)
                {
                    uint value = 0;
                    if (tokens[6 + (i * 2) + 1] == "0040")
                    {
                        value = 1;
                    }

                    value = value << 31 - i;
                    command |= value;
                }

                return command;
            }
            else return 0;
        }
    }
}
