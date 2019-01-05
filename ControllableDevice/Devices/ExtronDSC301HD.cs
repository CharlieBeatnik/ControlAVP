using System;
using Windows.Devices.SerialCommunication;
using System.Text.RegularExpressions;
using System.Diagnostics;

namespace ControllableDevice
{
    public class ExtronDSC301HD : IControllableDevice
    {
        private Rs232Device _rs232Device;

        public ExtronDSC301HD(string portId)
        {
            _rs232Device = new Rs232Device(portId);
            Debug.Assert(_rs232Device != null);

            _rs232Device.BaudRate = 9600;
            _rs232Device.PostRead = (x) =>
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

        public bool GetAvailable()
        {
            // Getting firmware as a good way to determine if device is on
            var firmware = GetFirmware();
            return firmware != null;
        }

        public Version GetFirmware()
        {
            var result = _rs232Device.WriteWithResponse("*Q");
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
