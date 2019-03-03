using System;
using Windows.Devices.SerialCommunication;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.Threading;
using ControllableDeviceTypes.ExtronMVX44VGATypes;

namespace ControllableDevice
{
    public class ExtronMVX44VGA : IControllableDevice
    {
        private bool _disposed = false;
        private Rs232Device _rs232Device;

        private readonly string _cmdEsc = ('\x1B').ToString();
        private readonly string _cmdCr = "\r";

        public ExtronMVX44VGA(string portId)
        {
            _rs232Device = new Rs232Device(portId);
            Debug.Assert(_rs232Device != null);

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
            string pattern = @"^([0-9]+).([0-9]+)$";
            var result = _rs232Device.WriteWithResponse("Q", pattern);
            if (result != null)
            {
                var match = Regex.Match(result, pattern);
                Debug.Assert(match.Success);
                int major = int.Parse(match.Groups[1].Value);
                int minor = int.Parse(match.Groups[2].Value);
                return new Version(major, minor);
            }

            return null;
        }

        public bool Reset(ResetType resetType)
        {
            string result = null;
            var resetCommandWaitTime = TimeSpan.FromSeconds(2);

            switch(resetType)
            {
                case ResetType.GlobalPresets:
                    result = _rs232Device.WriteWithResponse($"{_cmdEsc}ZG{_cmdCr}", @"^Zpg$", resetCommandWaitTime);
                    break;
                case ResetType.AudioInputLevels:
                    result = _rs232Device.WriteWithResponse($"{_cmdEsc}ZA{_cmdCr}", @"^Zpa$", resetCommandWaitTime);
                    break;
                case ResetType.AudioOutputLevels:
                    result = _rs232Device.WriteWithResponse($"{_cmdEsc}ZV{_cmdCr}", @"^Zpv$", resetCommandWaitTime);
                    break;
                case ResetType.AllMutes:
                    result = _rs232Device.WriteWithResponse($"{_cmdEsc}ZZ{_cmdCr}", @"^Zpz$", resetCommandWaitTime);
                    break;
                case ResetType.AllRGBDelaySettings:
                    result = _rs232Device.WriteWithResponse($"{_cmdEsc}ZD{_cmdCr}", @"^Zpd$", resetCommandWaitTime);
                    break;
                case ResetType.Full:
                    result = _rs232Device.WriteWithResponse($"{_cmdEsc}ZXXX{_cmdCr}", @"^Zpx$", resetCommandWaitTime);
                    break;
            }
            
            return (result != null);
        }

        public InputTie? GetInputTieForOutputPort(OutputPort outputPort, TieType tieType)
        {
            string result = null;

            switch(tieType)
            {
                case TieType.Video:
                    result = _rs232Device.WriteWithResponse($"{(int)outputPort}&", @"^[0-9]+$");
                    break;
                case TieType.Audio:
                    result = _rs232Device.WriteWithResponse($"{(int)outputPort}$", @"^[0-9]+$");
                    break;
            }

            if(result == null) return null;

            InputTie tie = (InputTie)int.Parse(result);
            if(!tie.Valid()) return null;

            return tie;
        }

        public TieState GetTieState(int preset = 0)
        {
            //0 0 0 0 Vid 0 0 0 0 Aud 
            string pattern = @"^([0-4]) ([0-4]) ([0-4]) ([0-4]) Vid ([0-4]) ([0-4]) ([0-4]) ([0-4]) Aud $";
            var result = _rs232Device.WriteWithResponse($"{_cmdEsc}{preset:D2}VC{_cmdCr}", pattern);
            if (result == null) return null;

            var tieState = new TieState();

            var match = Regex.Match(result, pattern);
            if (!match.Success) return null;

            tieState.Video.Add(OutputPort.Port1, (InputTie)int.Parse(match.Groups[1].Value));
            tieState.Video.Add(OutputPort.Port2, (InputTie)int.Parse(match.Groups[2].Value));
            tieState.Video.Add(OutputPort.Port3, (InputTie)int.Parse(match.Groups[3].Value));
            tieState.Video.Add(OutputPort.Port4, (InputTie)int.Parse(match.Groups[4].Value));

            tieState.Audio.Add(OutputPort.Port1, (InputTie)int.Parse(match.Groups[5].Value));
            tieState.Audio.Add(OutputPort.Port2, (InputTie)int.Parse(match.Groups[6].Value));
            tieState.Audio.Add(OutputPort.Port3, (InputTie)int.Parse(match.Groups[7].Value));
            tieState.Audio.Add(OutputPort.Port4, (InputTie)int.Parse(match.Groups[8].Value));

            return tieState;
        }

    }
}
