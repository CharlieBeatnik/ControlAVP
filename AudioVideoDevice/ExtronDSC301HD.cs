using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.SerialCommunication;
using System.Threading;
using System.Text.RegularExpressions;
using System.Diagnostics;

namespace AudioVideoDevice
{
    public class ExtronDSC301HD : AudioVideoSerialDevice
    {
        protected override uint BaudRate { get; } = 9600;
        protected override SerialStopBitCount StopBits { get; } = SerialStopBitCount.One;
        protected override ushort DataBits { get; } = 8;
        protected override SerialParity Parity { get; } = SerialParity.None;

        public ExtronDSC301HD(string portId) : base(portId)
        {
            PostRead = (x) =>
            {
                return x.TrimEnd("\r\n".ToCharArray());
            };
        }

        private bool Success(string response)
        {
            //E01 — Invalid input number
            //E10 — Invalid command
            //E11 — Invalid preset number
            //E13 — Invalid parameter
            //E14 — Not valid for this configuration
            //E17 — Invalid command for signal type
            //E22 — Busy
            //E25 — Device not present

            var match = Regex.Match(response, @"E[0-9][0-9]");
            return !match.Success;
        }

        public Version GetFirmware()
        {
            var result = WriteWithResponse("*Q");
            if (Success(result))
            {
                var match = Regex.Match(result, @"^([0-9]+).([0-9]+).([0-9]+)$");
                Debug.Assert(match.Success);
                int major = int.Parse(match.Groups[1].Value);
                int minor = int.Parse(match.Groups[2].Value);
                int build = int.Parse(match.Groups[3].Value);
                return new Version(major, minor, build, 0);
            }
            else return null;
        }
    }
}
