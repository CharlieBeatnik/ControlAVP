using PrimS.Telnet;
using System.Threading.Tasks;

namespace ControllableDevice
{
    internal static class PrimSTelnetClientExtensions
    {
        public static void WriteLine(this Client client, string command)
        {
            Task.Run(async () => await client.WriteLineAsync(command).ConfigureAwait(false));
        }

        public static string Read(this Client client)
        {
            return Task.Run(async () => await client.ReadAsync().ConfigureAwait(false)).Result;
        }
    }
}
