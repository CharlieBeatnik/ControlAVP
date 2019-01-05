using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using ControllableDeviceTypes.ApcAP8959EU3Types;

namespace ControllableDevice
{
    public class ApcAP8959EU3 : IControllableDevice
    {
        private SshDevice _sshDevice;
        private string _host;
        private int _port;
        private string _username;
        private string _password;

        public static readonly string TerminalPrompt = "apc>";

        public ApcAP8959EU3(string host, int port, string username, string password)
        {
            _sshDevice = new SshDevice();

            _host = host;
            _port = port;
            _username = username;
            _password = password;

            Connect();
        }

        public void Connect()
        {
            _sshDevice.Connect(_host, _port, _username, _password, ApcAP8959EU3.TerminalPrompt);
        }

        public bool Available
        {
            get
            {
                return _sshDevice.Connected;
            }
        }

        public IEnumerable<Outlet> GetOutletsWaitForPending()
        {
            IEnumerable<Outlet> outlets;
            bool pendingFound;

            do
            {
                pendingFound = false;
                outlets = GetOutlets();

                // If any ids are found to have a pending state then run the query again
                var pendingSearch = outlets.ToList().FindAll(o => o.Pending == true);

                if (pendingSearch.Count > 0)
                {
                    pendingFound = true;
                }

            } while (pendingFound);

            return outlets;
        }

        public IEnumerable<Outlet> GetOutletsWaitForPending(List<int> outletIds)
        {
            IEnumerable<Outlet> outlets;
            bool pendingFound;

            do
            {
                pendingFound = false;
                outlets = GetOutlets();

                // If any ids from outletIds are found to have a pending state then run the query again
                var pendingSearch = outlets.ToList().FindAll(o =>
                    (o.Pending == true) && 
                    (outletIds.Contains(o.Id))
                );

                if (pendingSearch.Count > 0)
                {
                    pendingFound = true;
                }

            } while (pendingFound);

            return outlets;
        }

        public IEnumerable<Outlet> GetOutlets()
        {
            Dictionary<int, Outlet> output = new Dictionary<int, Outlet>();

            int id;
            string name;
            string tail;

            if(!_sshDevice.Connected)
            {
                Connect();
            }
            Debug.Assert(_sshDevice.Connected);

            var olStatusAll = _sshDevice.ExecuteCommand("olStatus all");
            var olReadingAllPower = _sshDevice.ExecuteCommand("olReading all power");
            var olReadingAllCurrent = _sshDevice.ExecuteCommand("olReading all current");

            // Added asserts as ExecuteCommand has been oberved to return null
            Debug.Assert(olStatusAll != null);
            Debug.Assert(olReadingAllPower != null);
            Debug.Assert(olReadingAllCurrent != null);

            // Initially populate Outlet dictionary with information parsed from olStatus all
            foreach (string line in olStatusAll)
            {
                bool success = ParseCommonLine(line, out id, out name, out tail);
                if (success)
                {
                    Match match = Regex.Match(tail, @"^([A-Za-z]+)([*]*) *$");
                    if (match.Success)
                    {
                        Outlet.PowerState state = (match.Groups[1].Value == "On") ? Outlet.PowerState.On : Outlet.PowerState.Off;
                        bool pending = (match.Groups[2].Value == "*");

                        output.Add(id, new Outlet() { Id = id, Name = name, State = state, Pending = pending });
                    }
                }
            }

            // Populate additional information parsed from power command
            foreach (string line in olReadingAllPower)
            {
                bool success = ParseCommonLine(line, out id, out name, out tail);
                if (success)
                {
                    Match match = Regex.Match(tail, @"^([0-9.]+) W *$");
                    if (match.Success)
                    {
                        Outlet foundOutlet;
                        if (output.TryGetValue(id, out foundOutlet))
                        {
                            foundOutlet.Watts = float.Parse(match.Groups[1].Value);
                        }
                    }
                }
            }

            // Populate additional information parsed from current command
            foreach (string line in olReadingAllCurrent)
            {
                bool success = ParseCommonLine(line, out id, out name, out tail);
                if (success)
                {
                    Match match = Regex.Match(tail, @"^([0-9.]+) A *$");
                    if (match.Success)
                    {
                        Outlet foundOutlet;
                        if (output.TryGetValue(id, out foundOutlet))
                        {
                            foundOutlet.Amps = float.Parse(match.Groups[1].Value);
                        }
                    }
                }
            }

            if(output.Values.Count > 0)
            {
                return output.Values;
            }
            else
            {
                return null;
            }
        }

        private bool ParseCommonLine(string line, out int id, out string name, out string tail)
        {
            // Examples
            // 21: Outlet 21: Off
            // 22: PlayStation 4: On*
            // 23: Network Switch: 0 W
            // 24: Rack: 40 W
            Match match = Regex.Match(line, @"^ *([0-9]+): (.+): (.+)$");
            if (match.Success)
            {
                id = int.Parse(match.Groups[1].Value);
                name = match.Groups[2].Value;
                tail = match.Groups[3].Value;
                return true;
            }
            else
            {
                id = -1;
                name = string.Empty;
                tail = string.Empty;
                return false;
            }
        }

        public void TurnOutletOn(int id)
        {
            _sshDevice.ExecuteCommand(string.Format("olOn {0}", id.ToString()));
        }

        public void TurnOutletOff(int id)
        {
            _sshDevice.ExecuteCommand(string.Format("olOff {0}", id.ToString()));
        }
    }
}
