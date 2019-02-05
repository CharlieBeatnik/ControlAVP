using System;
using System.Text.RegularExpressions;
using System.Diagnostics;
using ControllableDeviceTypes.ExtronDSC301HDTypes;
using System.Threading;

namespace ControllableDevice
{
    public class ExtronDSC301HD : IControllableDevice
    {
        private bool _disposed = false;
        private Rs232Device _rs232Device;

        private readonly string _cmdEsc = ('\x1B').ToString();
        private readonly string _cmdCr = "\r";
        //private readonly string _cmdCrLf = "\r\n";
        private readonly string _patternNumber = @"^[+-]*[0-9]+$";

        private readonly int _hPosMin = -2048;
        private readonly int _hPosMax = 2048;
        private readonly int _vPosMin = -1200;
        private readonly int _vPosMax = 1200;

        private readonly int _hSizeMin = 10;
        private readonly int _hSizeMax = 4096;
        private readonly int _vSizeMin = 10;
        private readonly int _vSizeMax = 2400;

        private ExtronDSC301HD() { Debug.Assert(true, "Default constructor should never be called"); }
        private ExtronDSC301HD(string portId)
        {
            _rs232Device = new Rs232Device(portId);
            _rs232Device.BaudRate = 9600;
        }

        public static ExtronDSC301HD Create(string portId)
        {
            try
            {
                return new ExtronDSC301HD(portId);
            }
            catch(Exception)
            {
                return null;
            }
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
                _rs232Device.Dispose();
            }

            _disposed = true;
        }

        public bool GetAvailable()
        {
            // Getting firmware as a good way to determine if device is on
            var firmware = GetFirmware();
            return firmware != null;
        }

        public Version GetFirmware()
        {
            string pattern = @"^([0-9]+).([0-9]+).([0-9]+)$";
            var result = _rs232Device.WriteWithResponse("*Q", pattern);
            if (result != null)
            {
                var match = Regex.Match(result, pattern);
                Debug.Assert(match.Success);
                int major = int.Parse(match.Groups[1].Value);
                int minor = int.Parse(match.Groups[2].Value);
                int build = int.Parse(match.Groups[3].Value);
                return new Version(major, minor, build);
            }

            return null;
        }

        public void SetPixelPerfectAndCentered()
        {
            var edid = OutputRate;

            int inputWidth = ActivePixels;
            int inputHeight = ActiveLines;

            HorizontalSize = inputWidth;
            VerticalSize = inputHeight;

            HorizontalPosition = ((edid.Width - inputWidth) / 2);
            VerticalPosition = ((edid.Height - inputHeight) / 2);
        }

        public int ActivePixels
        {
            get
            {
                var result = _rs232Device.WriteWithResponse($"{_cmdEsc}APIX{_cmdCr}", _patternNumber);
                Debug.Assert(result != null);
                if (result == null) return 0;
                return int.Parse(result);
            }
        }

        public int ActiveLines
        {
            get
            {
                var result = _rs232Device.WriteWithResponse($"{_cmdEsc}ALIN{_cmdCr}", _patternNumber);

                Debug.Assert(result != null);
                if (result == null) return 0;
                return int.Parse(result);
            }
        }

        public int HorizontalPosition
        {
            get
            {
                var result = _rs232Device.WriteWithResponse($"{_cmdEsc}HCTR{_cmdCr}", _patternNumber);

                Debug.Assert(result != null);
                if (result == null) return 0;

                int number = int.Parse(result);
                Debug.Assert(number >= _hPosMin);
                Debug.Assert(number <= _hPosMax);
                if (number >= _hPosMin && number <= _hPosMax)
                {
                    return number;
                }
                else return 0;
            }
            set
            {
                int newValue = Math.Clamp(value, _hPosMin, _hPosMax);
                var result = _rs232Device.WriteWithResponse($"{_cmdEsc}{newValue}HCTR{_cmdCr}", @"^Hctr[+-][0-9]+$");
                Debug.Assert(result != null);
            }
        }

        public int VerticalPosition
        {
            get
            {
                var result = _rs232Device.WriteWithResponse($"{_cmdEsc}VCTR{_cmdCr}", _patternNumber);

                Debug.Assert(result != null);
                if (result == null) return 0;

                int number = int.Parse(result);
                Debug.Assert(number >= _vPosMin);
                Debug.Assert(number <= _vPosMax);
                if (number >= _vPosMin && number <= _vPosMax)
                {
                    return number;
                }
                else return 0;
            }
            set
            {
                int newValue = Math.Clamp(value, _vPosMin, _vPosMax);
                var result = _rs232Device.WriteWithResponse($"{_cmdEsc}{newValue}VCTR{_cmdCr}", @"^Vctr[+-][0-9]+$");
                Debug.Assert(result != null);
            }
        }

        public int HorizontalSize
        {
            get
            {
                var result = _rs232Device.WriteWithResponse($"{_cmdEsc}HSIZ{_cmdCr}", _patternNumber);
                Debug.Assert(result != null);
                if (result == null) return 0;

                int number = int.Parse(result);
                Debug.Assert(number >= _hSizeMin);
                Debug.Assert(number <= _hSizeMax);
                if (number >= _hSizeMin && number <= _hSizeMax)
                {
                    return number;
                }
                else return 0;
            }
            set
            {
                int newValue = Math.Clamp(value, _hSizeMin, _hSizeMax);
                var result = _rs232Device.WriteWithResponse($"{_cmdEsc}{newValue}HSIZ{_cmdCr}", @"^Hsiz[0-9]+$");
                Debug.Assert(result != null);
            }
        }

        public int VerticalSize
        {
            get
            {
                var result = _rs232Device.WriteWithResponse($"{_cmdEsc}VSIZ{_cmdCr}", _patternNumber);

                int number = int.Parse(result);
                Debug.Assert(number >= _vSizeMin);
                Debug.Assert(number <= _vSizeMax);
                if (number >= _vSizeMin && number <= _vSizeMax)
                {
                    return number;
                }
                else return 0;
            }
            set
            {
                int newValue = Math.Clamp(value, _vSizeMin, _vSizeMax);
                var result = _rs232Device.WriteWithResponse($"{_cmdEsc}{newValue}VSIZ{_cmdCr}", @"^Vsiz[0-9]+$");
                Debug.Assert(result != null);
            }
        }

        public Edid OutputRate
        {
            get
            {
                var result = _rs232Device.WriteWithResponse($"{_cmdEsc}RATE{_cmdCr}", _patternNumber);
                var number = int.Parse(result);
                return Edid.GetEdid(number);
            }
            set
            {
                _rs232Device.Write($"{_cmdEsc}{value.Id}RATE{_cmdCr}");

                //Output Rate change needs longer than normal to take effect
                Thread.Sleep(TimeSpan.FromSeconds(2));

                var result = _rs232Device.Read(@"^Rate[0-9]+$");
                Debug.Assert(result != null);
            }
        }
    }
}
