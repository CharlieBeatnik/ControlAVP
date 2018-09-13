using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.Devices.SerialCommunication;

namespace ComControl
{
    internal class AtenVS0801H : AudioVideoDevice
    {
        public enum SwitchMode
        {
            Default,
            Next,
            Auto
        };

        public class State
        {
            public State(string input, string output, string mode, string goTo, string firmware)
            {
                Match match;

                //Input 
                match = Regex.Match(input, @"^Input: port([0-9]+)$");
                Debug.Assert(match.Success);
                if(match.Success)
                {
                    int inputPort = int.Parse(match.Groups[1].Value);
                    Debug.Assert(inputPort >= _lowestHdmiInputIdx && inputPort <= _highestHdmiInputIdx);
                    Input = inputPort;
                }

                //Output
                match = Regex.Match(output, @"^Output: ([A-Z]+)$");
                Debug.Assert(match.Success);
                if (match.Success)
                {
                    Output = match.Groups[1].Value == "ON";
                }

                //Mode
                match = Regex.Match(mode, @"^Mode: ([A-Za-z]+)$");
                Debug.Assert(match.Success);
                if (match.Success)
                {
                    switch(match.Groups[1].Value)
                    {
                        case "Default": Mode = SwitchMode.Default; break;
                        case "Next":    Mode = SwitchMode.Next;    break;
                        case "Auto":    Mode = SwitchMode.Auto;    break;
                    }
                }

                //GoTo
                match = Regex.Match(goTo, @"^Goto: ([A-Z]+)$");
                Debug.Assert(match.Success);
                if (match.Success)
                {
                    GoTo = match.Groups[1].Value == "ON";
                }

                //Firmware
                match = Regex.Match(firmware, @"^F/W: V([0-9]+).([0-9]+).([0-9]+)$");
                Debug.Assert(match.Success);
                if (match.Success)
                {
                    int major = int.Parse(match.Groups[1].Value);
                    int minor = int.Parse(match.Groups[2].Value);
                    int build = int.Parse(match.Groups[3].Value);
                    Firmware = new Version(major, minor, build);
                }
            }

            int Input { get; }

            bool Output { get; }

            public SwitchMode Mode { get; }

            public bool GoTo { get; }

            public Version Firmware { get;} 
        }

        private const int _lowestHdmiInputIdx = 1;
        private const int _highestHdmiInputIdx = 8;
        private const int _numHdmiInputs = 8;

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

        public bool SetInput(int inputPort)
        {
            Debug.Assert(inputPort >= _lowestHdmiInputIdx && inputPort <= _highestHdmiInputIdx);

            var result = WriteWithResponse($"sw i{inputPort:00}");
            return Success(result);
        }

        public bool SetOutput(bool enable)
        {
            var write = string.Format("sw {0}", enable ? "on" : "off");
            var result = WriteWithResponse(write);
            return Success(result);
        }

        public bool SetMode(SwitchMode mode, int inputPort = _lowestHdmiInputIdx)
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
                    Debug.Assert(inputPort >= _lowestHdmiInputIdx && inputPort <= _highestHdmiInputIdx);
                    result = WriteWithResponse($"swmode i{inputPort:00} auto");
                    break;
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

                if (Success(lines[0]))
                {
                    lines.GetRange(1, 5);

                    var state = new State(lines[1], lines[2], lines[3], lines[4], lines[5]);
                    return state;
                }
            }
            
            return null;
        }
    }
}
