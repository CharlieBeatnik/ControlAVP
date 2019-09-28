using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ControllableDevice
{
    public class SerialBlaster : IDisposable
    {
        private bool _disposed = false;
        private Rs232Device _rs232Device;

        public enum Protocol
        {
            Nec
        };

        public SerialBlaster(string portId)
        {
            _rs232Device = new Rs232Device(portId);
            _rs232Device.BaudRate = 115200;

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

        public bool Enabled
        {
            get
            {
                return _rs232Device.Enabled;
            }
        }

        public bool SendCommand(Protocol protocol, uint command)
        {
            string commandHex = command.ToString("X8");
            string result = _rs232Device.WriteWithResponse($"send {protocol.ToString().ToLower()} 0x{commandHex}", "OK");

            return result != null;
        }

        public bool SendMessage(string message)
        {
            string result = _rs232Device.WriteWithResponse($"message {message}", "OK");
            return result != null;
        }
    }
}
