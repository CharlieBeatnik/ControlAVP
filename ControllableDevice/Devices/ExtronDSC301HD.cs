using System;
using System.Text.RegularExpressions;
using System.Diagnostics;
using ControllableDeviceTypes.ExtronDSC301HDTypes;
using System.Threading;
using System.Numerics;

namespace ControllableDevice
{
    public class ExtronDSC301HD : IControllableDevice
    {
        private bool _disposed = false;
        private Rs232Device _rs232Device;

        private readonly string _cmdEsc = ('\x1B').ToString();
        private readonly string _cmdCr = "\r";
        private readonly string _patternNumberLine = @"^[+-]*[0-9]+$";
        private readonly string _patternNumber = $@"[+-]*[0-9]+";
        private readonly TimeSpan _outputRateChangeWait = TimeSpan.FromSeconds(5);

        public ExtronDSC301HD(string portId)
        {
            _rs232Device = new Rs232Device(portId);
            _rs232Device.BaudRate = 9600;
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

        public void Scale(ScaleType scaleType, PositionType positionType = PositionType.Centre)
        {
            var edid = OutputRate;

            int inputWidth = ActivePixels;
            int inputHeight = ActiveLines;

            int vSize = 0;
            int hSize = 0;
            int vPos = 0;
            int hPos = 0;

            float edidRatio = (float)edid.Width / (float)edid.Height;
            float inputRatio = (float)inputWidth / (float)inputHeight;

            switch (scaleType)
            {
                case ScaleType.PixelPerfect:
                    vSize = inputHeight;
                    hSize = inputWidth;
                    break;

                case ScaleType.Fit:
                    if (inputHeight > inputWidth) //Portrait
                    {
                        vSize = edid.Height;
                        hSize = (int)(vSize * inputRatio);

                        float scale = (float)edid.Width / (float)hSize;
                        hSize = (int)((float)hSize * scale);
                        vSize = (int)((float)vSize * scale);
                    }
                    else //Landscape or Square
                    {
                        hSize = edid.Width;
                        vSize = (int)(hSize * (1 / inputRatio));

                        float scale = (float)edid.Height / (float)vSize;
                        hSize = (int)((float)hSize * scale);
                        vSize = (int)((float)vSize * scale);
                    }
                    break;
            }

            switch (positionType)
            {
                case PositionType.Centre:

                    hPos = ((edid.Width - hSize) / 2);
                    vPos = ((edid.Height - vSize) / 2);
                    break;
            }

            //Write changes
            ImagePositionAndSize = new PositionAndSize(hPos, vPos, hSize, vSize);
        }

        public int ActivePixels
        {
            get
            {
                var result = _rs232Device.WriteWithResponse($"{_cmdEsc}APIX{_cmdCr}", _patternNumberLine);
                Debug.Assert(result != null);
                if (result == null) return 0;
                return int.Parse(result);
            }
        }

        public int ActiveLines
        {
            get
            {
                var result = _rs232Device.WriteWithResponse($"{_cmdEsc}ALIN{_cmdCr}", _patternNumberLine);

                Debug.Assert(result != null);
                if (result == null) return 0;
                return int.Parse(result);
            }
        }

        public int HorizontalPosition
        {
            get
            {
                var result = _rs232Device.WriteWithResponse($"{_cmdEsc}HCTR{_cmdCr}", _patternNumberLine);

                Debug.Assert(result != null);
                if (result == null) return 0;

                int number = int.Parse(result);
                Debug.Assert(number >= PositionAndSize.HPosMin);
                Debug.Assert(number <= PositionAndSize.HPosMax);
                if (number >= PositionAndSize.HPosMin && number <= PositionAndSize.HPosMax)
                {
                    return number;
                }
                else return 0;
            }
            set
            {
                int newValue = Math.Clamp(value, PositionAndSize.HPosMin, PositionAndSize.HPosMax);
                var result = _rs232Device.WriteWithResponse($"{_cmdEsc}{newValue}HCTR{_cmdCr}", @"^Hctr[+-][0-9]+$");
                Debug.Assert(result != null);
            }
        }

        public int VerticalPosition
        {
            get
            {
                var result = _rs232Device.WriteWithResponse($"{_cmdEsc}VCTR{_cmdCr}", _patternNumberLine);

                Debug.Assert(result != null);
                if (result == null) return 0;

                int number = int.Parse(result);
                Debug.Assert(number >= PositionAndSize.VPosMin);
                Debug.Assert(number <= PositionAndSize.VPosMax);
                if (number >= PositionAndSize.VPosMin && number <= PositionAndSize.VPosMax)
                {
                    return number;
                }
                else return 0;
            }
            set
            {
                int newValue = Math.Clamp(value, PositionAndSize.VPosMin, PositionAndSize.VPosMax);
                var result = _rs232Device.WriteWithResponse($"{_cmdEsc}{newValue}VCTR{_cmdCr}", @"^Vctr[+-][0-9]+$");
                Debug.Assert(result != null);
            }
        }

        public int HorizontalSize
        {
            get
            {
                var result = _rs232Device.WriteWithResponse($"{_cmdEsc}HSIZ{_cmdCr}", _patternNumberLine);
                Debug.Assert(result != null);
                if (result == null) return 0;

                int number = int.Parse(result);
                Debug.Assert(number >= PositionAndSize.HSizeMin);
                Debug.Assert(number <= PositionAndSize.HSizeMax);
                if (number >= PositionAndSize.HSizeMin && number <= PositionAndSize.HSizeMax)
                {
                    return number;
                }
                else return 0;
            }
            set
            {
                int newValue = Math.Clamp(value, PositionAndSize.HSizeMin, PositionAndSize.HSizeMax);
                var result = _rs232Device.WriteWithResponse($"{_cmdEsc}{newValue}HSIZ{_cmdCr}", @"^Hsiz[0-9]+$");
                Debug.Assert(result != null);
            }
        }

        public int VerticalSize
        {
            get
            {
                var result = _rs232Device.WriteWithResponse($"{_cmdEsc}VSIZ{_cmdCr}", _patternNumberLine);

                int number = int.Parse(result);
                Debug.Assert(number >= PositionAndSize.VSizeMin);
                Debug.Assert(number <= PositionAndSize.VSizeMax);
                if (number >= PositionAndSize.VSizeMin && number <= PositionAndSize.VSizeMax)
                {
                    return number;
                }
                else return 0;
            }
            set
            {
                int newValue = Math.Clamp(value, PositionAndSize.VSizeMin, PositionAndSize.VSizeMax);
                var result = _rs232Device.WriteWithResponse($"{_cmdEsc}{newValue}VSIZ{_cmdCr}", @"^Vsiz[0-9]+$");
                Debug.Assert(result != null);
            }
        }

        public Edid OutputRate
        {
            get
            {
                var result = _rs232Device.WriteWithResponse($"{_cmdEsc}RATE{_cmdCr}", _patternNumberLine);
                var number = int.Parse(result);
                return Edid.GetEdid(number);
            }
            set
            {
                _rs232Device.Write($"{_cmdEsc}{value.Id}RATE{_cmdCr}");

                //Output Rate change needs longer than normal to take effect
                Thread.Sleep(_outputRateChangeWait);

                var result = _rs232Device.Read(@"^Rate[0-9]+$");
                Debug.Assert(result != null);
            }
        }

        public PositionAndSize ImagePositionAndSize
        {
            get
            {
                string pattern = $@"^({_patternNumber})[*]({_patternNumber})[*]({_patternNumber})[*]({_patternNumber})$";
                var result = _rs232Device.WriteWithResponse($"{_cmdEsc}XIMG{_cmdCr}", pattern);
                if (result != null)
                {
                    var match = Regex.Match(result, pattern);
                    Debug.Assert(match.Success);
                    return new PositionAndSize(
                        int.Parse(match.Groups[1].Value),
                        int.Parse(match.Groups[2].Value),
                        int.Parse(match.Groups[3].Value),
                        int.Parse(match.Groups[4].Value));
                }

                return null;
            }
            set
            {
                string pattern = $@"^Ximg({_patternNumber})[*]({_patternNumber})[*]({_patternNumber})[*]({_patternNumber})$";
                var result = _rs232Device.WriteWithResponse($"{_cmdEsc}{value.HPos}*{value.VPos}*{value.HSize}*{value.VSize}XIMG{_cmdCr}", pattern);
                Debug.Assert(result != null);
            }
        }
       
    }
}
