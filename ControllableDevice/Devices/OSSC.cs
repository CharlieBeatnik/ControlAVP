using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ControllableDevice
{
    public class OSSC : IControllableDevice
    {
        private bool _disposed = false;
        private Rs232Device _rs232Device;

        public OSSC(string portId)
        {
            _rs232Device = new Rs232Device(portId);
            _rs232Device.BaudRate = 115200;
            _rs232Device.MessageTerminator = "\r";
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
            return true;
        }

        public bool SendCommand(uint command)
        {
            string commandHex = command.ToString("X");
            string result = _rs232Device.WriteWithResponse($"send nec 0x{commandHex}", ".*");

            return result != null;
        }
    }
}
