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
        private PasswordAuthenticationMethod _authMethod;
        private ConnectionInfo _connectionInfo;
        private SshClient _sshClient;
        private ShellStream _shellStream;

        private bool _connectionValid = false;
        private readonly Object _lock = new Object();

        private string _host;
        private int _port;
        private string _username;
        private string _password;
        private string _terminalPrompt = String.Empty;

        public SshDevice()
        {

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

        public bool Connect(string host, int port, string username, string password, string terminalPrompt)
        {
            lock (_lock)
            {
                _host = host;
                _port = port;
                _username = username;
                _password = password;
                _terminalPrompt = terminalPrompt;

                if (Connected)
                {
                    Disconnect();
                }

                try
                {
                    _authMethod = new PasswordAuthenticationMethod(_username, _password);
                    _connectionInfo = new ConnectionInfo(_host, _port, _username, _authMethod);

                    _sshClient = new SshClient(_connectionInfo);
                    _sshClient.Connect();
                    _shellStream = _sshClient.CreateShellStream("xterm", 80, 24, 800, 600, 1024);

                    // This will pull the initial login prompt off the stream
                    _shellStream.Expect(new Regex(_terminalPrompt));

                    _connectionValid = true;
                }
                catch (Exception)
                {
                    if (_shellStream != null)
                    {
                        _shellStream.Close();
                        _shellStream = null;
                    }

                    if (_sshClient != null)
                    {
                        if (_sshClient.IsConnected)
                        {
                            _sshClient.Disconnect();
                        }
                        _sshClient = null;
                    }

                    _connectionInfo = null;
                    _authMethod = null;

                    _connectionValid = false;

                    return false;
                }
                return true;
            }
        }

        private bool Connect()
        {
            return Connect(_host, _port, _username, _password, _terminalPrompt);
        }

        public void Disconnect()
        {
            lock (_lock)
            {
                if (Connected)
                {
                    _shellStream.Close();
                    _sshClient.Disconnect();
                }

                _shellStream = null;
                _sshClient = null;

                _connectionValid = false;
            }
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

        public void Dispose()
        {
            lock (_lock)
            {
                Disconnect();
            }
        }
    }
}
