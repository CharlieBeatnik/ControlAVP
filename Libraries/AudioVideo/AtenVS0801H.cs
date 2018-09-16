using System;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using Windows.Devices.SerialCommunication;

namespace AudioVideo
{
    public class AtenVS0801H : AudioVideoDevice
    {
        public enum SwitchMode
        {
            Default,
            Next,
            Auto
        };

        public enum InputPort
        {
            Port1 = 1,
            Port2 = 2,
            Port3 = 3,
            Port4 = 4,
            Port5 = 5,
            Port6 = 6,
            Port7 = 7,
            Port8 = 8
        }

        public class State
        {
            public InputPort InputPort { get; set;  }

            public bool Output { get; set;  }

            public SwitchMode Mode { get; set;  }

            public bool GoTo { get; set;  }

            public Version Firmware { get; set; } 
        }

        protected override string _sendLineEnding
        {
            get { return "\r"; }
        }

        public AtenVS0801H(string portId) : base(portId)
        {
            BaudRate = 19200;
            StopBits = SerialStopBitCount.One;
            DataBits = 8;
            Parity = SerialParity.None;
        }

        private bool Success(string response)
        {
            return response.Contains("Command OK");
        }

        public bool GoToNextInput()
        {
            var result = WriteWithResponse("sw+");
            return Success(result);
        }

        public bool GoToPreviousInput()
        {
            var result = WriteWithResponse("sw-");
            return Success(result);
        }

        public bool SetInput(InputPort inputPort)
        {
            var result = WriteWithResponse($"sw i{(int)inputPort:00}");
            return Success(result);
        }

        public bool SetOutput(bool enable)
        {
            var write = string.Format("sw {0}", enable ? "on" : "off");
            var result = WriteWithResponse(write);
            return Success(result);
        }

        public bool SetMode(SwitchMode mode, InputPort inputPort)
        {
            string result = string.Empty;

            switch(mode)
            {
                case SwitchMode.Default:
                    result = WriteWithResponse("swmode default");
                    break;
                case SwitchMode.Next:
                    result = WriteWithResponse("swmode next");
                    break;
                case SwitchMode.Auto:
                    result = WriteWithResponse($"swmode i{inputPort:00} auto");
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
            var result = WriteWithResponse(write);
            return Success(result);
        }
       
        public State GetState()
        {
            string result = WriteWithResponse("read");
            if (Success(result))
            {
                var lines = result.Split("\r\n", StringSplitOptions.RemoveEmptyEntries).ToList();
                Debug.Assert(lines.Count == 6);

                Match match;
                State state = new State();

                //Input 
                match = Regex.Match(lines[1], @"^Input: port([0-9]+)$");
                Debug.Assert(match.Success);
                int inputPort;
                var inputParse = int.TryParse(match.Groups[1].Value, out inputPort);
                state.InputPort = (InputPort)inputPort;
                Debug.Assert(inputParse);

                //Output
                match = Regex.Match(lines[2], @"^Output: ([A-Z]+)$");
                Debug.Assert(match.Success);
                state.Output = match.Groups[1].Value == "ON";

                //Mode
                match = Regex.Match(lines[3], @"^Mode: ([A-Za-z]+)$");
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
                match = Regex.Match(lines[4], @"^Goto: ([A-Z]+)$");
                Debug.Assert(match.Success);
                state.GoTo = match.Groups[1].Value == "ON";

                //Firmware
                match = Regex.Match(lines[5], @"^F/W: V([0-9]+).([0-9]+).([0-9]+)$");
                Debug.Assert(match.Success);
                int major = int.Parse(match.Groups[1].Value);
                int minor = int.Parse(match.Groups[2].Value);
                int build = int.Parse(match.Groups[3].Value);
                state.Firmware = new Version(major, minor, build);

                return state;
            }
            
            return null;
        }
    }
}
