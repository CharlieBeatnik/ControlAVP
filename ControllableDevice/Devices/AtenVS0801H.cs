using System;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using Windows.Devices.SerialCommunication;
using ControllableDeviceTypes.AtenVS0801HTypes;

namespace ControllableDevice
{
    public class AtenVS0801H : IControllableDevice
    {
        private Rs232Device _rs232Device;

        public AtenVS0801H(string portId)
        {
            _rs232Device = new Rs232Device(portId);
            Debug.Assert(_rs232Device != null);

            _rs232Device.BaudRate = 19200;
            _rs232Device.PreWrite = (x) =>
            {
                return x + "\r";
            };
        }

        private bool Success(string response)
        {
            return response.Contains("Command OK");
        }

        public bool GetAvailable()
        {
            // Getting state as a good way to determine if device is on
            var state = GetState();
            return state != null;
        }

        public bool GoToNextInput()
        {
            var result = _rs232Device.WriteWithResponse("sw+");
            return Success(result);
        }

        public bool GoToPreviousInput()
        {
            var result = _rs232Device.WriteWithResponse("sw-");
            return Success(result);
        }

        public bool SetInput(InputPort inputPort)
        {
            var result = _rs232Device.WriteWithResponse($"sw i{(int)inputPort:00}");
            return Success(result);
        }

        public bool SetOutput(bool enable)
        {
            var write = string.Format("sw {0}", enable ? "on" : "off");
            var result = _rs232Device.WriteWithResponse(write);
            return Success(result);
        }

        public bool SetMode(SwitchMode mode, InputPort inputPort)
        {
            string result = string.Empty;

            switch(mode)
            {
                case SwitchMode.Default:
                    result = _rs232Device.WriteWithResponse("swmode default");
                    break;
                case SwitchMode.Next:
                    result = _rs232Device.WriteWithResponse("swmode next");
                    break;
                case SwitchMode.Auto:
                    result = _rs232Device.WriteWithResponse($"swmode i{inputPort:00} auto");
                    break;
                default:
                    Debug.Assert(false, "Unkown SwitchMode");
                    return false;
            }

            return Success(result);
        }

        public bool SetGoTo(bool enable)
        {
            var write = string.Format("swmode goto {0}", enable ? "on" : "off");
            var result = _rs232Device.WriteWithResponse(write);
            return Success(result);
        }
       
        public State GetState()
        {
            var responses = _rs232Device.WriteWithResponses("read", 6);
            Debug.Assert(responses.Count == 6);

            if (Success(responses[0]))
            {
                Match match;
                State state = new State();

                //Input 
                match = Regex.Match(responses[1], @"^Input: port([0-9]+)$");
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
                    case "Default": state.Mode = SwitchMode.Default; break;
                    case "Next":    state.Mode = SwitchMode.Next;    break;
                    case "Auto":    state.Mode = SwitchMode.Auto;    break;
                    default:
                        Debug.Assert(false, "Unknown SwitchMode");
                        break;
                }

                //GoTo
                match = Regex.Match(responses[4], @"^Goto: ([A-Z]+)$");
                Debug.Assert(match.Success);
                state.GoTo = match.Groups[1].Value == "ON";

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
