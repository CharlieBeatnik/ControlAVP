using Renci.SshNet;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;

namespace ControllableDevice
{
    public class SshDevice : IDisposable
    {
        private bool _disposed;

        private SshClient _sshClient;
        private ShellStream _shellStream;

        private bool _connectionValid;
        private readonly Object _lock = new Object();

        private readonly string _host;
        private readonly int _port;
        private readonly string _username;
        private readonly string _password;
        private readonly string _terminalPrompt = String.Empty;

        public SshDevice(string host, int port, string username, string password, string terminalPrompt)
        {
            _host = host;
            _port = port;
            _username = username;
            _password = password;
            _terminalPrompt = terminalPrompt;

            Connect();
        }

        public bool Connected
        {
            get
            {
                if (_sshClient != null)
                {
                    return _sshClient.IsConnected && _connectionValid;
                }
                else return false;
            }
        }

        private void Connect()
        {
            lock (_lock)
            {
                if (Connected)
                {
                    Disconnect();
                }

                try
                {
                    using (var authMethod = new PasswordAuthenticationMethod(_username, _password))
                    {
                        var connectionInfo = new ConnectionInfo(_host, _port, _username, authMethod);

                        _sshClient = new SshClient(connectionInfo);
                        _sshClient.Connect();
                        _shellStream = _sshClient.CreateShellStream("xterm", 80, 24, 800, 600, 1024);

                        // This will pull the initial login prompt off the stream
                        _shellStream.Expect(new Regex(_terminalPrompt));
                    }

                    _connectionValid = true;
                }
                catch (Exception)
                {
                    _connectionValid = false;

                    _shellStream?.Close();
                    _shellStream = null;

                    _sshClient?.Disconnect();
                    _sshClient = null;

                    throw;
                }
            }
        }

        private void Disconnect()
        {
            lock (_lock)
            {
                if (Connected)
                {
                    _shellStream?.Close();
                    _shellStream = null;

                    _sshClient?.Disconnect();
                    _sshClient = null;
                }

                _connectionValid = false;
            }
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
                _shellStream?.Dispose();
                _shellStream = null;

                _sshClient?.Dispose();
                _sshClient = null;
            }

            _disposed = true;
        }

        public IEnumerable<string> ExecuteCommand(string command)
        {
            lock (_lock)
            {
                if (!Connected)
                {
                    Connect();
                    if (!Connected) return null;
                }

                //Split Write and WriteLine as it looked like WriteLine was truncating commands
                _shellStream.Write(command);
                _shellStream.WriteLine("");

                string streamOutput = _shellStream.Expect(new Regex(_terminalPrompt));
                Debug.Assert(!string.IsNullOrEmpty(streamOutput));

                using (StringReader sr = new StringReader(streamOutput))
                {
                    var result = new List<string>();
                    string line;
                    while ((line = sr.ReadLine()) != null)
                    {
                        result.Add(line);
                    }

                    return result;
                }
            }
        }
    }
}
