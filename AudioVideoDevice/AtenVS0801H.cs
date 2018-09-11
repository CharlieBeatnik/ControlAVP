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
    }
}
