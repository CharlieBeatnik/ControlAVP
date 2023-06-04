using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using PrimS.Telnet;

namespace ControllableDevice
{
    public class ExtronIPL250 : IControllableDevice
    {
        private bool _disposed;
        private Client _telnetDevice;

        public const string TerminalPrompt = "";

        public ExtronIPL250(string host, int port)
        {
            _telnetDevice = new Client(host, port, new CancellationToken());
            Client.IsWriteConsole = true;
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
                _telnetDevice?.Dispose();
                _telnetDevice = null;
            }

            _disposed = true;
        }

        public bool GetAvailable()
        {
            return true;
        }

        public void Test()
        {
            if(_telnetDevice.IsConnected)
            {
                Task.Run(async () => await _telnetDevice.WriteLineAsync("i").ConfigureAwait(false));

                var result = Task.Run(async () => await _telnetDevice.ReadAsync().ConfigureAwait(false)).Result;
            }
        }
    }
}
