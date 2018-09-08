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
        public AtenVS0801H(string portId) : base(portId)
        {

        }

        protected override void SetSerialParameters()
        {
            _serialPort.BaudRate = 19200;
            _serialPort.StopBits = SerialStopBitCount.One;
            _serialPort.DataBits = 8;
            _serialPort.Parity = SerialParity.None;
        }
    }
}
