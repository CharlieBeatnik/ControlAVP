using Renci.SshNet;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;

namespace PduDevice
{
    public class PduSshClient : IDisposable
    {
        private PasswordAuthenticationMethod _authMethod;
        private ConnectionInfo _connectionInfo;
        private SshClient _sshClient;
        private ShellStream _shellStream;

        private bool _connectionValid = false;
        private readonly Object _lock = new Object();

        private string _terminalPrompt = String.Empty;

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
                if (Connected)
                {
                    Disconnect();
                }

                try
                {
                    _terminalPrompt = terminalPrompt;
                    _authMethod = new PasswordAuthenticationMethod(username, password);
                    _connectionInfo = new ConnectionInfo(host, port, username, _authMethod);
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
                _connectionInfo = null;
                _authMethod = null;

                _connectionValid = false;
            }
        }

        public IEnumerable<string> ExecuteCommand(string command)
        {
            lock (_lock)
            {
                if (Connected)
                {
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

                return null;
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
