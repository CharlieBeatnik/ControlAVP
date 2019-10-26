using System.Globalization;
using System.Net;
using System.Net.Sockets;

namespace ControllableDevice
{
    public static class WakeOnLan
    {
        public static void WakeUp(string macAddress)
        {
            using (var sock = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp))
            {
                sock.EnableBroadcast = true;

                int payloadIndex = 0;

                // The magic packet is a broadcast frame containing anywhere within its payload 6 bytes of all 255 (FF FF FF FF FF FF in hexadecimal)
                // followed by sixteen repetitions of the target computer's 48-bit MAC address, for a total of 102 bytes.
                byte[] payload = new byte[1024];

                // Add 6 bytes with value 255 (FF) in our payload
                for (int i = 0; i < 6; i++)
                {
                    payload[payloadIndex] = 255;
                    payloadIndex++;
                }

                // Repeat the device MAC address sixteen times
                for (int j = 0; j < 16; j++)
                {
                    for (int k = 0; k < macAddress.Length; k += 2)
                    {
                        var s = macAddress.Substring(k, 2);
                        payload[payloadIndex] = byte.Parse(s, NumberStyles.HexNumber);
                        payloadIndex++;
                    }
                }

                // Broadcast our packet
                sock.SendTo(payload, new IPEndPoint(IPAddress.Parse("255.255.255.255"), 0));
                sock.Close(10000);
            }
        }
    }
}
