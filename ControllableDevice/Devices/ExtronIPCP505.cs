using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using PrimS.Telnet;
using Windows.Media.Protection.PlayReady;

namespace ControllableDevice
{
    public class ExtronIPCP505 : IControllableDevice
    {
        private bool _disposed;
        private Client _telnetDevice;

        public const string TerminalPrompt = "";

        public ExtronIPCP505(string host, int port)
        {
            _telnetDevice = new Client(host, port, new CancellationToken());
            //Client.IsWriteConsole = true;
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

        public async void Test()
        {
            string _cmdEsc = ('\x1B').ToString();
            string _cmdCr = ('\x0D').ToString();
            string _cmdCrLr = ('\x0D').ToString() + ('\x0A').ToString();

            double timeoutMs = 500;
            string s;

            if (_telnetDevice.IsConnected)
            {
                try
                {
                    s = await _telnetDevice.TerminatedReadAsync(_cmdCrLr, TimeSpan.FromMilliseconds(timeoutMs));
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }

            if (_telnetDevice.IsConnected)
            {
                try
                {
                    string send = $"{_cmdEsc}CT{_cmdCr}";
                    await _telnetDevice.WriteLineAsync(send);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }

            if (_telnetDevice.IsConnected)
            {
                try
                {
                    s = await _telnetDevice.TerminatedReadAsync(_cmdCrLr, TimeSpan.FromMilliseconds(timeoutMs));
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }
        }
    }
}
