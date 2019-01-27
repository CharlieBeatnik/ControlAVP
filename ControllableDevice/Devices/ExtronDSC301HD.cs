using System;
using Windows.Devices.SerialCommunication;
using System.Text.RegularExpressions;
using System.Diagnostics;
using ControllableDeviceTypes.ExtronDSC301HDTypes;

namespace ControllableDevice
{
    public class ExtronDSC301HD : IControllableDevice
    {
        private Rs232Device _rs232Device;

        private readonly string _cmdEsc = ('\x1B').ToString();
        private readonly string _cmdCr = "\r";
        private readonly string _cmdCrLf = "\r\n";

        public ExtronDSC301HD(string portId)
        {
            _rs232Device = new Rs232Device(portId);
            Debug.Assert(_rs232Device != null);

            _rs232Device.BaudRate = 9600;
            _rs232Device.PostRead = (x) =>
            {
                Debug.WriteLine($"Last Read was: {x}");
                //Always take the last line in case there are other strings in the buffer
                var results = x.Split(_cmdCrLf, StringSplitOptions.RemoveEmptyEntries);
                return results[results.Length - 1];
            };

            _rs232Device.ZeroByteReadTimeout = TimeSpan.FromMilliseconds(1000);
            _rs232Device.WriteTimeout = TimeSpan.FromMilliseconds(750);
            _rs232Device.ReadTimeout = TimeSpan.FromMilliseconds(50);
        }

        private bool Success(string response)
        {
            if (!string.IsNullOrEmpty(response))
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
            else return false;
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
                return new Version(major, minor, build);
            }
            else return null;
        }

        public void SetPixelPerfectAndCentered()
        {
            uint inputWidth = ActivePixels;
            uint inputHeight = ActiveLines;

            HorizontalSize = inputWidth;
            VerticalSize = inputHeight;

            HorizontalPosition = ((1920 - inputWidth) / 2);
            VerticalPosition = ((1080 - inputHeight) / 2);
        }

        public void GetSetFormat(InputPort inputPort)
        {
            _rs232Device.WriteWithResponse($"{inputPort}\\");
        }

        public string GetAssignedEDIDData(InputPort inputPort)
        {
            return _rs232Device.WriteWithResponse($"{_cmdEsc}A{(int)inputPort:0}EDID{_cmdCr}");
        }

        public uint ActivePixels
        {
            get
            {
                var result = _rs232Device.WriteWithResponse($"{_cmdEsc}APIX{_cmdCr}");
                return uint.Parse(result);
            }
        }

        public uint ActiveLines
        {
            get
            {
                var result = _rs232Device.WriteWithResponse($"{_cmdEsc}ALIN{_cmdCr}");
                return uint.Parse(result);
            }
        }

        public uint HorizontalPosition
        {
            get
            {
                var result = _rs232Device.WriteWithResponse($"{_cmdEsc}HCTR{_cmdCr}");
                return uint.Parse(result);
            }
            set
            {
                var result = _rs232Device.WriteWithResponse($"{_cmdEsc}{value}HCTR{_cmdCr}");
            }
        }

        public uint VerticalPosition
        {
            get
            {
                var result = _rs232Device.WriteWithResponse($"{_cmdEsc}VCTR{_cmdCr}");
                return uint.Parse(result);
            }
            set
            {
                var result = _rs232Device.WriteWithResponse($"{_cmdEsc}{value}VCTR{_cmdCr}");
            }
        }

        public uint HorizontalSize
        {
            get
            {
                var result = _rs232Device.WriteWithResponse($"{_cmdEsc}HSIZ{_cmdCr}");
                return uint.Parse(result);
            }
            set
            {
                var result = _rs232Device.WriteWithResponse($"{_cmdEsc}{value}HSIZ{_cmdCr}");
            }
        }

        public uint VerticalSize
        {
            get
            {
                var result = _rs232Device.WriteWithResponse($"{_cmdEsc}VSIZ{_cmdCr}");
                return uint.Parse(result);
            }
            set
            {
                var result = _rs232Device.WriteWithResponse($"{_cmdEsc}{value}VSIZ{_cmdCr}");
            }
        }
    }
}
