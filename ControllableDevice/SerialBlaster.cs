using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ControllableDeviceTypes.SerialBlasterTypes;

namespace ControllableDevice
{
    public class SerialBlaster : IDisposable
    {
        private bool _disposed;
        private Rs232Device _rs232Device;

        public SerialBlaster(string portId)
        {
            _rs232Device = new Rs232Device(portId);
            Initialise(_rs232Device);
        }

        public SerialBlaster(uint deviceIndex)
        {
            _rs232Device = new Rs232Device(deviceIndex);
            Initialise(_rs232Device);
        }

        private void Initialise(Rs232Device rs232Device)
        {
            rs232Device.BaudRate = 115200;

            rs232Device.PreWrite = (x) =>
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

        public bool SendCommand(Protocol protocol, uint command, uint repeats = 0)
        {
            string commandHex = command.ToString("X8");
            string result = _rs232Device.WriteWithResponse($"send {protocol.ToString().ToLower()} 0x{commandHex} {repeats}", "OK");

            return result != null;
        }

        public bool SendMessage(string message)
        {
            string result = _rs232Device.WriteWithResponse($"message {message}", "OK");
            return result != null;
        }

        public bool GetAvailable()
        {
            //Send a dummy message to see if it succeeds
            return SendMessage("GetAvailable");
        }
    }
}
