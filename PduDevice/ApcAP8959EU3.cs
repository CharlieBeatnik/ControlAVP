using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;

namespace PduDevice
{
    public class ApcAP8959EU3
    {
        public class Outlet
        {
            public enum PowerState
            {
                On,
                Off
            }

            public int Id { get; set; }
            public string Name { get; set; }
            public PowerState State { get; set; }
            public bool Pending { get; set; }
            public float Watts { get; set; }
            public float Amps { get; set; }
        }

        private PduSshClient _pduSshClient;

        public static readonly string TerminalPrompt = "apc>";

        public ApcAP8959EU3(PduSshClient pduSshClient)
        {
            _pduSshClient = pduSshClient;
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

            var olStatusAll = _pduSshClient.ExecuteCommand("olStatus all");
            var olReadingAllPower = _pduSshClient.ExecuteCommand("olReading all power");
            var olReadingAllCurrent = _pduSshClient.ExecuteCommand("olReading all current");

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

            return output.Values;
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
            _pduSshClient.ExecuteCommand(string.Format("olOn {0}", id.ToString()));
        }

        public void TurnOutletOff(int id)
        {
            _pduSshClient.ExecuteCommand(string.Format("olOff {0}", id.ToString()));
        }
    }
}
