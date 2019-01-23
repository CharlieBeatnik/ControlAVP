using System;
using Windows.Devices.SerialCommunication;
using System.Text.RegularExpressions;
using System.Diagnostics;

namespace ControllableDevice
{
    public class ExtronMVX44VGA : IControllableDevice
    {
        private Rs232Device _rs232Device;

        public ExtronMVX44VGA(string portId)
        {
            _rs232Device = new Rs232Device(portId);
            Debug.Assert(_rs232Device != null);

            _rs232Device.BaudRate = 9600;
            _rs232Device.PostRead = (x) =>
            {
                return x.TrimEnd("\r\n".ToCharArray());
            };

            _rs232Device.ZeroByteReadTimeout = TimeSpan.FromMilliseconds(350);
            _rs232Device.WriteTimeout = TimeSpan.FromMilliseconds(300);
            _rs232Device.ReadTimeout = TimeSpan.FromMilliseconds(300);
            _rs232Device.UseFastClearBeforeEveryWrite = true;
        }

        private bool Success(string response)
        {
            //E01 — Invalid input number
            //E10 — Invalid command
            //E11 — Invalid preset number
            //E13 — Invalid parameter
            //E14 — Not valid for this configuration

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
            var result = _rs232Device.WriteWithResponse("Q");
            if (Success(result))
            {
                var match = Regex.Match(result, @"^([0-9]+).([0-9]+)$");
                Debug.Assert(match.Success);
                int major = int.Parse(match.Groups[1].Value);
                int minor = int.Parse(match.Groups[2].Value);
                return new Version(major, minor);
            }
            else return null;
        }
    }
}
