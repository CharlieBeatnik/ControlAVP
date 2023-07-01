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

        private CancellationToken _ct = new CancellationToken();

        public ExtronIPCP505(string host, int port)
        {
            Debug.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name);
            _ct = new CancellationToken();

            _telnetDevice = new Client(host, port, _ct);
            Client.IsWriteConsole = true;
        }

        public void Dispose()
        {
            Debug.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name);
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            Debug.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name);
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
            Debug.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name);
            return true;
        }

        public async Task<bool> Test()
        {
            Debug.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name);
            string _cmdEsc = ('\x1B').ToString();
            string _cmdCr = ('\x0D').ToString();
            string _cmdCrLr = ('\x0D').ToString() + ('\x0A').ToString();

            double timeoutMs = 500;
            string s;

            if (_telnetDevice.IsConnected)
            {
                try
                {
                    //Debug.WriteLine("Read 1");
                    s = await _telnetDevice.TerminatedReadAsync(_cmdCrLr, TimeSpan.FromMilliseconds(timeoutMs));

                    await _telnetDevice.TryLoginAsync("", "", 500);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }

            var sw = new Stopwatch();
            sw.Start();
            if (_telnetDevice.IsConnected)
            {
                try
                {
                    string send = $"{_cmdEsc}CT{_cmdCr}";
                    //send = "cn";

                    Thread.Sleep(1000);

                    await _telnetDevice.WriteAsync(_cmdCrLr);
                    await _telnetDevice.WriteAsync(_cmdCrLr);
                    await _telnetDevice.WriteAsync(_cmdCrLr);

                    Debug.WriteLine($"WriteAsync ({sw.ElapsedMilliseconds})");
                    await _telnetDevice.WriteAsync(send);
                    await _telnetDevice.WriteAsync(send);
                    await _telnetDevice.WriteAsync(send);
                    await _telnetDevice.WriteAsync(send);
                    await _telnetDevice.WriteAsync(send);

                    Debug.WriteLine($"TerminatedReadAsync ({sw.ElapsedMilliseconds})");
                    s = await _telnetDevice.TerminatedReadAsync(_cmdCrLr);
                    Debug.WriteLine(s + $" ({sw.ElapsedMilliseconds})");

                    s = await _telnetDevice.TerminatedReadAsync(_cmdCrLr);
                    Debug.WriteLine(s + $" ({sw.ElapsedMilliseconds})");

                    s = await _telnetDevice.TerminatedReadAsync(_cmdCrLr);
                    Debug.WriteLine(s + $" ({sw.ElapsedMilliseconds})");

                    s = await _telnetDevice.TerminatedReadAsync(_cmdCrLr);
                    Debug.WriteLine(s + $" ({sw.ElapsedMilliseconds})");
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }

            sw.Stop();
            return true;
        }
    }
}
