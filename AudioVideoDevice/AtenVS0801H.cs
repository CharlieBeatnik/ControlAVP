using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.SerialCommunication;

namespace ComControl
{
    internal class AtenVS0801H : AudioVideoDevice
    {
        protected override string _sendLineEnding
        {
            get { return "\r"; }
        }

        public AtenVS0801H(string portId) : base(portId)
        {

        }

        protected override void SetSerialParameters()
        {
            _serialPort.WriteTimeout = TimeSpan.FromMilliseconds(1000);
            _serialPort.ReadTimeout = TimeSpan.FromMilliseconds(1000);
            _serialPort.BaudRate = 19200;
            _serialPort.StopBits = SerialStopBitCount.One;
            _serialPort.DataBits = 8;
            _serialPort.Parity = SerialParity.None;
        }
    }
}
