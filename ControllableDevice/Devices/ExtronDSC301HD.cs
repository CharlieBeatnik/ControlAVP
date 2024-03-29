﻿using System;
using System.Text.RegularExpressions;
using System.Diagnostics;
using ControllableDeviceTypes.ExtronDSC301HDTypes;
using System.Threading;
using System.Numerics;

namespace ControllableDevice
{
    public class ExtronDSC301HD : IControllableDevice
    {
        private bool _disposed;
        private readonly Rs232Device _rs232Device;

        private readonly string _cmdEsc = ('\x1B').ToString();
        private const string _cmdCr = "\r";
        private const string _patternNumberLine = @"^[+-]*[0-9]+$";
        private const string _patternNumber = @"[+-]*[0-9]+";

        private const int _pictureAdjustmentMin = 0;
        private const int _pictureAdjustmentMax = 127;
        private const int _pictureAdjustmentDefault = 64;

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
            if (!_rs232Device.Enabled) return false;

            // Getting firmware as a good way to determine if device is on
            var firmware = GetFirmware();
            return firmware != null;
        }

        public Version GetFirmware()
        {
            if (!_rs232Device.Enabled) return null;

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

        public bool Scale(ScaleType scaleType, PositionType positionType, AspectRatio aspectRatio, Vector2 padding)
        {
            if (!_rs232Device.Enabled) return false;

            if(padding == null)
            {
                padding = new Vector2();
            }

            var outputEdid = GetOutputRate();

            int inputWidth = (int)GetActivePixels();
            int inputHeight = (int)GetActiveLines();

            int inputWidthWithoutPadding = inputWidth - (int)Math.Round(padding.X * 2);
            int inputHeightWithoutPadding = inputHeight - (int)Math.Round(padding.Y * 2);

            if (aspectRatio != AspectRatio.RatioPreserve)
            {
                inputHeightWithoutPadding = (int)(inputWidthWithoutPadding * (1.0f / aspectRatio.GetRatio()));
                inputHeight = (int)(inputWidth * (1.0f / aspectRatio.GetRatio()));
            }

            float edidRatio = (float)outputEdid.Width / (float)outputEdid.Height;
            float inputRatio = (float)inputWidth / (float)inputHeight;

            int vSize = 0;
            int hSize = 0;
            int vPos = 0;
            int hPos = 0;

            switch (scaleType)
            {
                case ScaleType.PixelPerfect:
                    vSize = inputHeight;
                    hSize = inputWidth;
                    break;

                case ScaleType.Fit:
                    if (inputHeight > inputWidth) //Portrait
                    {
                        vSize = outputEdid.Height;
                        hSize = (int)(vSize * inputRatio);

                        if (hSize > outputEdid.Width)
                        {
                            float scale = (float)outputEdid.Width / (float)hSize;
                            hSize = (int)((float)hSize * scale);
                            vSize = (int)((float)vSize * scale);
                        }
                    }
                    else //Landscape or Square
                    {
                        hSize = outputEdid.Width;
                        vSize = (int)(hSize * (1 / inputRatio));

                        if (vSize > outputEdid.Height)
                        {
                            float scale = (float)outputEdid.Height / (float)vSize;
                            hSize = (int)((float)hSize * scale);
                            vSize = (int)((float)vSize * scale);
                        }
                    }
                    break;

                case ScaleType.FitWidth:
                    {
                        var widthPaddingScale = (float)inputWidth / inputWidthWithoutPadding;

                        hSize = (int)(outputEdid.Width * (float)widthPaddingScale);
                        vSize = (int)(hSize * (1 / inputRatio));
                    }
                    break;
            }

            switch (positionType)
            {
                case PositionType.Centre:
                    hPos = ((outputEdid.Width - hSize) / 2);
                    vPos = ((outputEdid.Height - vSize) / 2);
                    break;
            }

            var result = SetImagePositionAndSize(new PositionAndSize(hPos, vPos, hSize, vSize));
            return result;
        }

        public int? GetActivePixels()
        {
            if (!_rs232Device.Enabled) return null;

            var result = _rs232Device.WriteWithResponse($"{_cmdEsc}APIX{_cmdCr}", _patternNumberLine);
            Debug.Assert(result != null);
            if (result == null) return 0;
            return int.Parse(result);
        }

        public int? GetActiveLines()
        {
            if (!_rs232Device.Enabled) return null;

            var result = _rs232Device.WriteWithResponse($"{_cmdEsc}ALIN{_cmdCr}", _patternNumberLine);

            Debug.Assert(result != null);
            if (result == null) return 0;
            return int.Parse(result);
        }

        public int? GetHorizontalPosition()
        {
            if (!_rs232Device.Enabled) return null;

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

        public bool SetHorizontalPosition(int value)
        {
            if (!_rs232Device.Enabled) return false;

            int newValue = Math.Clamp(value, PositionAndSize.HPosMin, PositionAndSize.HPosMax);
            var result = _rs232Device.WriteWithResponse($"{_cmdEsc}{newValue}HCTR{_cmdCr}", @"^Hctr[+-][0-9]+$");
            return (result != null);
        }

        public int? GetVerticalPosition()
        {
            if (!_rs232Device.Enabled) return null;

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

        public bool SetVerticalPosition(int value)
        {
            if (!_rs232Device.Enabled) return false;

            int newValue = Math.Clamp(value, PositionAndSize.VPosMin, PositionAndSize.VPosMax);
            var result = _rs232Device.WriteWithResponse($"{_cmdEsc}{newValue}VCTR{_cmdCr}", @"^Vctr[+-][0-9]+$");
            return (result != null);
        }

        public int? GetHorizontalSize()
        {
            if (!_rs232Device.Enabled) return null;

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

        public bool SetHorizontalSize(int value)
        {
            if (!_rs232Device.Enabled) return false;

            int newValue = Math.Clamp(value, PositionAndSize.HSizeMin, PositionAndSize.HSizeMax);
            var result = _rs232Device.WriteWithResponse($"{_cmdEsc}{newValue}HSIZ{_cmdCr}", @"^Hsiz[0-9]+$");
            return (result != null);
        }

        public int? GetVerticalSize()
        {
            if (!_rs232Device.Enabled) return null;

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

        public bool SetVerticalSize(int value)
        {
            if (!_rs232Device.Enabled) return false;

            int newValue = Math.Clamp(value, PositionAndSize.VSizeMin, PositionAndSize.VSizeMax);
            var result = _rs232Device.WriteWithResponse($"{_cmdEsc}{newValue}VSIZ{_cmdCr}", @"^Vsiz[0-9]+$");
            return (result != null);
        }

        public Edid GetOutputRate()
        {
            if (!_rs232Device.Enabled) return null;

            var result = _rs232Device.WriteWithResponse($"{_cmdEsc}RATE{_cmdCr}", _patternNumberLine);
            var number = int.Parse(result);
            return Edid.GetEdid(number);
        }

        public bool SetOutputRate(Edid value)
        {
            if(value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            if (!_rs232Device.Enabled) return false;

            var result = _rs232Device.WriteWithResponse($"{_cmdEsc}{value.Id}RATE{_cmdCr}", @"^Rate[0-9]+$", TimeSpan.FromSeconds(5));
            return (result != null);
        }

        public bool SetOutputRateWithoutEdid(int width, int height, float refreshRate)
        {
            var edid = Edid.GetEdid(width, height, refreshRate);
           
            //Handle null Edid gracefully, just return false
            if (edid != null)
            {
                return SetOutputRate(edid);
            }
            else return false;           
        }

        public PositionAndSize GetImagePositionAndSize()
        {
            if (!_rs232Device.Enabled) return null;

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

        public bool SetImagePositionAndSize(PositionAndSize value)
        {
            if(value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            if (!_rs232Device.Enabled) return false;

            string pattern = $@"^Ximg({_patternNumber})[*]({_patternNumber})[*]({_patternNumber})[*]({_patternNumber})$";
            var result = _rs232Device.WriteWithResponse($"{_cmdEsc}{value.HPos}*{value.VPos}*{value.HSize}*{value.VSize}XIMG{_cmdCr}", pattern);
            return (result != null);
        }

        public bool SetInputPort(InputPort inputPort)
        {
            if (!_rs232Device.Enabled) return false;

            var result = _rs232Device.WriteWithResponse($"{(int)inputPort}!", $@"^In{(int)inputPort} All");
            return (result != null);
        }

        public InputPort? GetInputPort()
        {
            if (!_rs232Device.Enabled) return null;

            var result = _rs232Device.WriteWithResponse($"!", _patternNumber);

            if (result != null)
            {
                var inputPort = (InputPort)int.Parse(result);
                if(inputPort.Valid())
                {
                    return inputPort;
                }
            }

            return null;
        }

        public float? GetTemperature()
        {
            if (!_rs232Device.Enabled) return null;

            var result = _rs232Device.WriteWithResponse($"{_cmdEsc}20STAT{_cmdCr}", _patternNumber);
            if (result != null)
            {
                return float.Parse(result);
            }

            return null;
        }

        public int? GetDetailFilter()
        {
            if (!_rs232Device.Enabled) return null;

            var result = _rs232Device.WriteWithResponse($"{_cmdEsc}HDET{_cmdCr}", _patternNumberLine);
            Debug.Assert(result != null);
            if (result == null) return 0;
            return int.Parse(result);
        }

        public bool SetDetailFilter(int value)
        {
            if (!_rs232Device.Enabled) return false;
            if (value < _pictureAdjustmentMin || value > _pictureAdjustmentMax) return false;

            var result = _rs232Device.WriteWithResponse($"{_cmdEsc}{value}HDET{_cmdCr}", $@"^Hdet[123]\*{value:000}$");
            return (result != null);
        }

        public bool SetDetailFilterDefault()
        {
            return SetDetailFilter(_pictureAdjustmentDefault);
        }

        public int? GetBrightness()
        {
            if (!_rs232Device.Enabled) return null;

            var result = _rs232Device.WriteWithResponse($"{_cmdEsc}BRIT{_cmdCr}", _patternNumberLine);
            Debug.Assert(result != null);
            if (result == null) return 0;
            return int.Parse(result);
        }

        public bool SetBrightness(int value)
        {
            if (!_rs232Device.Enabled) return false;
            if (value < _pictureAdjustmentMin || value > _pictureAdjustmentMax) return false;

            var result = _rs232Device.WriteWithResponse($"{_cmdEsc}{value}BRIT{_cmdCr}", $@"^Brit[123]\*{value:000}$");
            return (result != null);
        }

        public bool SetBrightnessDefault()
        {
            return SetBrightness(_pictureAdjustmentDefault);
        }

        public int? GetContrast()
        {
            if (!_rs232Device.Enabled) return null;

            var result = _rs232Device.WriteWithResponse($"{_cmdEsc}CONT{_cmdCr}", _patternNumberLine);
            Debug.Assert(result != null);
            if (result == null) return 0;
            return int.Parse(result);
        }

        public bool SetContrast(int value)
        {
            if (!_rs232Device.Enabled) return false;
            if (value < _pictureAdjustmentMin || value > _pictureAdjustmentMax) return false;

            var result = _rs232Device.WriteWithResponse($"{_cmdEsc}{value}CONT{_cmdCr}", $@"^Cont[123]\*{value:000}$");
            return (result != null);
        }

        public bool SetContrastDefault()
        {
            return SetContrast(_pictureAdjustmentDefault);
        }

        public bool? GetFreeze()
        {
            if (!_rs232Device.Enabled) return null;

            var result = _rs232Device.WriteWithResponse($"F", $@"^[01]$");
            Debug.Assert(result != null);
            if (result == null) return false;
            return result == "1";
        }

        public bool SetFreeze(bool freeze)
        {
            if (!_rs232Device.Enabled) return false;

            var result = _rs232Device.WriteWithResponse($"{(freeze ? "1" : "0")}F", $@"^Frz{(freeze ? "1" : "0")}$");
            return (result != null);
        }
    }
}
