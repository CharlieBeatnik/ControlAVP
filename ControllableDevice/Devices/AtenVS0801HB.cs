using System;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using Windows.Devices.SerialCommunication;
using ControllableDeviceTypes.AtenVS0801HBTypes;

namespace ControllableDevice
{
    public class AtenVS0801HB : IControllableDevice
    {
        private bool _disposed;
        private Rs232Device _rs232Device;
        private const string _respSuccess = "Command OK";

        public AtenVS0801HB(string portId)
        {
            _rs232Device = new Rs232Device(portId);
            Debug.Assert(_rs232Device != null);

            _rs232Device.BaudRate = 19200;
            _rs232Device.PreWrite = (x) =>
            {
                return x + "\r";
            };
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
            if(!_rs232Device.Enabled) return false;

            // Getting state as a good way to determine if device is on
            var state = GetState();
            return state != null;
        }

        public bool GoToNextInput()
        {
            if (!_rs232Device.Enabled) return false;

            var state = GetState();
            if (state == null) return false;

            int portCount = state.InputPort.Count();
            InputPort nextInput = (InputPort)((int)state.InputPort % portCount) + 1;
            return SetInputPort(nextInput);
        }

        public bool GoToPreviousInput()
        {
            if (!_rs232Device.Enabled) return false;

            var state = GetState();
            if (state == null) return false;

            int portCount = state.InputPort.Count();
            InputPort previousInput = (InputPort)(portCount - (((portCount - (int)state.InputPort) + 1) % portCount));
            return SetInputPort(previousInput);
        }

        public bool SetInputPort(InputPort inputPort)
        {
            if (!_rs232Device.Enabled) return false;

            var result = _rs232Device.WriteWithResponse($"sw i{(int)inputPort:00}", $"^sw i{(int)inputPort:00} {_respSuccess}$");
            return result != null;
        }

        public bool SetOutput(bool enable)
        {
            if (!_rs232Device.Enabled) return false;

            var write = string.Format("sw {0}", enable ? "on" : "off");
            var result = _rs232Device.WriteWithResponse(write, $"{write} {_respSuccess}");
            return result != null;
        }

        public bool SetMode(SwitchMode mode, InputPort inputPort)
        {
            if (!_rs232Device.Enabled) return false;

            string result = string.Empty;
            switch (mode)
            {
                case SwitchMode.Off:
                    result = _rs232Device.WriteWithResponse("swmode default", $"^swmode off {_respSuccess}$");
                    break;
                case SwitchMode.Next:
                    result = _rs232Device.WriteWithResponse("swmode next", $"^swmode next {_respSuccess}$");
                    break;
                case SwitchMode.Priority:
                    result = _rs232Device.WriteWithResponse($"swmode i{inputPort:00} priority", $"^swmode i{inputPort:00} priority {_respSuccess}$");
                    break;
                default:
                    Debug.Assert(false, "Unkown SwitchMode");
                    return false;
            }

            return result != null;
        }

        public bool SetPowerOnDetection(bool enable)
        {
            if (!_rs232Device.Enabled) return false;

            var write = string.Format("swmode pod {0}", enable ? "on" : "off");
            var result = _rs232Device.WriteWithResponse(write, $"{write} {_respSuccess}");
            return result != null;
        }
       
        public State GetState()
        {
            if (!_rs232Device.Enabled) return null;

            var responses = _rs232Device.WriteWithResponses("read", 6);
            Debug.Assert(responses.Count == 6);

            if (responses[0] == $"read {_respSuccess}")
            {
                Match match;
                State state = new State();

                //Input 
                match = Regex.Match(responses[1], @"^Input: port *([0-9]+)$");
                Debug.Assert(match.Success);
                var inputParseSuccess = int.TryParse(match.Groups[1].Value, out int inputPort);
                Debug.Assert(inputParseSuccess);
                state.InputPort = (InputPort)inputPort;
                Debug.Assert((state.InputPort >= InputPort.Port1) && (state.InputPort <= InputPort.Port8));

                //Output
                match = Regex.Match(responses[2], @"^Output: ([A-Z]+)$");
                Debug.Assert(match.Success);
                state.Output = match.Groups[1].Value == "ON";

                //Mode
                match = Regex.Match(responses[3], @"^Mode: ([A-Za-z]+)$");
                Debug.Assert(match.Success);
                switch (match.Groups[1].Value)
                {
                    case "OFF": state.Mode = SwitchMode.Off; break;
                    case "NEXT": state.Mode = SwitchMode.Next; break;
                    case "PRIORITY": state.Mode = SwitchMode.Priority; break;
                    default:
                        Debug.Assert(false, "Unknown SwitchMode");
                        break;
                }

                //Power On Detection (POD)
                match = Regex.Match(responses[4], @"^Pod: ([A-Z]+)$");
                Debug.Assert(match.Success);
                state.PowerOnDetection = match.Groups[1].Value == "ON";

                //Firmware
                match = Regex.Match(responses[5], @"^F/W: V([0-9]+).([0-9]+).([0-9]+)$");
                Debug.Assert(match.Success);
                int major = int.Parse(match.Groups[1].Value);
                int minor = int.Parse(match.Groups[2].Value);
                int build = int.Parse(match.Groups[3].Value);
                state.Firmware = new Version(major, minor, build, 0);

                return state;
            }

            return null;
        }
    }
}
