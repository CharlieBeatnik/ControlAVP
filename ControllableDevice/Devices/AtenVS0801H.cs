﻿using System;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using Windows.Devices.SerialCommunication;
using ControllableDeviceTypes.AtenVS0801HTypes;

namespace ControllableDevice
{
    public class AtenVS0801H : IControllableDevice
    {
        private bool _disposed;
        private readonly Rs232Device _rs232Device;
        private const string _respSuccess = "Command OK";

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

            var result = _rs232Device.WriteWithResponse("sw+", $"^sw[+] {_respSuccess}$");
            return result != null;
        }

        public bool GoToPreviousInput()
        {
            if (!_rs232Device.Enabled) return false;

            var result = _rs232Device.WriteWithResponse("sw-", $"^sw[-] {_respSuccess}$");
            return result != null;
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
                case SwitchMode.Default:
                    result = _rs232Device.WriteWithResponse("swmode default", $"^swmode default {_respSuccess}$");
                    break;
                case SwitchMode.Next:
                    result = _rs232Device.WriteWithResponse("swmode next", $"^swmode next {_respSuccess}$");
                    break;
                case SwitchMode.Auto:
                    result = _rs232Device.WriteWithResponse($"swmode i{inputPort:00} auto", $"^swmode i{inputPort:00} auto {_respSuccess}$");
                    break;
                default:
                    Debug.Assert(false, "Unkown SwitchMode");
                    return false;
            }

            return result != null;
        }

        public bool SetGoTo(bool enable)
        {
            if (!_rs232Device.Enabled) return false;

            var write = string.Format("swmode goto {0}", enable ? "on" : "off");
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
                    case "Next": state.Mode = SwitchMode.Next; break;
                    case "Auto": state.Mode = SwitchMode.Auto; break;
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
