using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.SerialCommunication;

namespace ComControl
{
    internal class AtenVS0801H : AudioVideoDevice
    {
        public class State
        {
            public State(List<string> deviceState)
            {
                Input = deviceState[0];
            }

            public string Input { get; }
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

        public bool NextInput()
        {
            var result = WriteWithResponse("sw+");
            return Success(result);
        }

        public bool PreviousInput()
        {
            var result = WriteWithResponse("sw-");
            return Success(result);
        }

        public bool SwitchInput(int input)
        {
            Debug.Assert(input >= _lowestHdmiInputIdx && input <= _highestHdmiInputIdx);

            var result = WriteWithResponse($"sw i{input:00}");
            return Success(result);
        }

        public bool Output(bool enable)
        {
            var write = string.Format("sw {0}", enable ? "on" : "off");
            var result = WriteWithResponse(write);
            return Success(result);
        }

        public State GetState()
        {
            string result = WriteWithResponse("read");
            var lines = result.Split("\r\n", StringSplitOptions.RemoveEmptyEntries).ToList();

            if(Success(lines[0]))
            {
                lines.GetRange(1, 5);

                var state = new State(lines.GetRange(1, 5));
                return state;
            }
            return null;
        }
    }
}
