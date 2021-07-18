using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using ControllableDeviceTypes.ApcAP8959EU3Types;

namespace ControllableDevice
{
    public class ApcAP8959EU3 : IControllableDevice
    {
        private bool _disposed;
        private SshDevice _sshDevice;

        private const int _outletCount = 24;
        public const string TerminalPrompt = "apc>";

        public ApcAP8959EU3(string host, int port, string username, string password)
        {
            _sshDevice = new SshDevice(host, port, username, password, ApcAP8959EU3.TerminalPrompt);
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
                _sshDevice?.Dispose();
                _sshDevice = null;
            }

            _disposed = true;
        }

        public bool GetAvailable()
        {
            var outlets = GetOutlets();
            return outlets.Count() == _outletCount;
        }

        public IEnumerable<Outlet> GetOutletsWaitForPending(bool getPower = false, bool getCurrent = false)
        {
            IEnumerable<Outlet> outlets;
            bool pendingFound;

            do
            {
                pendingFound = false;
                outlets = GetOutlets(getPower, getCurrent);

                // If any ids are found to have a pending state then run the query again
                var pendingSearch = outlets.ToList().FindAll(o => o.Pending == true);

                if (pendingSearch.Count > 0)
                {
                    pendingFound = true;
                }

            } while (pendingFound);

            return outlets;
        }

        public IEnumerable<Outlet> GetOutletsWaitForPending(List<int> outletIds, bool getPower = false, bool getCurrent = false)
        {
            IEnumerable<Outlet> outlets;
            bool pendingFound;

            do
            {
                pendingFound = false;
                outlets = GetOutlets(getPower, getCurrent);

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

        public IEnumerable<Outlet> GetOutlets(bool getPower = false, bool getCurrent = false)
        {
            Dictionary<int, Outlet> output = new Dictionary<int, Outlet>();

            int id;
            string name;
            string tail;

            var olStatusAll = _sshDevice.ExecuteCommand("olStatus all");
            // Added asserts as ExecuteCommand has been oberved to return null
            Debug.Assert(olStatusAll != null);

            // Initially populate Outlet dictionary with information parsed from olStatus all
            foreach (string line in olStatusAll)
            {
                bool success = ParseOlReadingCommonLine(line, out id, out name, out tail);
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

            if (getPower)
            {
                var olReadingAllPower = _sshDevice.ExecuteCommand("olReading all power");
                Debug.Assert(olReadingAllPower != null);

                // Populate additional information parsed from power command
                foreach (string line in olReadingAllPower)
                {
                    bool success = ParseOlReadingCommonLine(line, out id, out name, out tail);
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
            }

            if (getCurrent)
            {
                var olReadingAllCurrent = _sshDevice.ExecuteCommand("olReading all current");
                Debug.Assert(olReadingAllCurrent != null);

                // Populate additional information parsed from current command
                foreach (string line in olReadingAllCurrent)
                {
                    bool success = ParseOlReadingCommonLine(line, out id, out name, out tail);
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

        public IEnumerable<Phase> GetPhases()
        {
            Dictionary<int, Phase> phase = new Dictionary<int, Phase>();

            int id;
            string tail;

            //Get power of all phases in Watts
            var phReadingAllPower = _sshDevice.ExecuteCommand("phReading all power");
            Debug.Assert(phReadingAllPower != null);
            foreach (string line in phReadingAllPower)
            {
                bool success = ParsePhReadingCommonLine(line, out id, out tail);
                if (success)
                {
                    Match match = Regex.Match(tail, @"^([0-9.]+) kW*$");
                    if (match.Success)
                    {
                        float watts = (float.Parse(match.Groups[1].Value) * 1000); //kW to W
                        phase.Add(id, new Phase() { Id = id, Watts = watts });
                    }
                }
            }

            //Get current of all phases in Amps
            var phReadingAllCurrent = _sshDevice.ExecuteCommand("phReading all current");
            Debug.Assert(phReadingAllCurrent != null);
            foreach (string line in phReadingAllCurrent)
            {
                bool success = ParsePhReadingCommonLine(line, out id, out tail);
                if (success)
                {
                    Match match = Regex.Match(tail, @"^([0-9.]+) A*$");
                    if (match.Success)
                    {
                        Phase foundPhase;
                        if (phase.TryGetValue(id, out foundPhase))
                        {
                            foundPhase.Amps = float.Parse(match.Groups[1].Value);
                        }
                    }
                }
            }

            //Get voltage of all phases in Volts
            var phReadingAllVoltage = _sshDevice.ExecuteCommand("phReading all voltage");
            Debug.Assert(phReadingAllVoltage != null);
            foreach (string line in phReadingAllVoltage)
            {
                bool success = ParsePhReadingCommonLine(line, out id, out tail);
                if (success)
                {
                    Match match = Regex.Match(tail, @"^([0-9.]+) V*$");
                    if (match.Success)
                    {
                        Phase foundPhase;
                        if (phase.TryGetValue(id, out foundPhase))
                        {
                            foundPhase.Voltage = float.Parse(match.Groups[1].Value);
                        }
                    }
                }
            }

            if (phase.Values.Count > 0)
            {
                return phase.Values;
            }
            else
            {
                return null;
            }
        }

        private static bool ParseOlReadingCommonLine(string line, out int id, out string name, out string tail)
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

        private static bool ParsePhReadingCommonLine(string line, out int id, out string tail)
        {
            // Examples
            // 1: 0.13 kVA
            // 2: 0.04 kW
            // 3: 257 V
            // 4: 0.5 A
            Match match = Regex.Match(line, @"^ *([0-9]+): (.+)$");
            if (match.Success)
            {
                id = int.Parse(match.Groups[1].Value);
                tail = match.Groups[2].Value;
                return true;
            }
            else
            {
                id = -1;
                tail = string.Empty;
                return false;
            }
        }

        public bool TurnOutletOn(int id)
        {
            var output = _sshDevice.ExecuteCommand(string.Format("olOn {0}", id.ToString()));
            return CommandSuccessful(output);
        }

        public bool TurnOutletOff(int id)
        {
            var output = _sshDevice.ExecuteCommand(string.Format("olOff {0}", id.ToString()));
            return CommandSuccessful(output);
        }

        private static bool CommandSuccessful(IEnumerable<string> commandOutput)
        {
            //Look for success string in command output
            string found = commandOutput.FirstOrDefault(o => o == "E000: Success");
            return found != null;
        }

        public static bool OutletIdValid(int id)
        {
            return (id >= 1 && id <= _outletCount);
        }
    }
}
