//using PrimS.Telnet;
//using System;
//using System.Threading.Tasks;

//namespace ControllableDevice
//{
//    internal static class PrimSTelnetClientExtensions
//    {
//        public static void WriteLine(this Client client, string command)
//        {
//            Task.Run(async () => await client.WriteLineAsync(command).ConfigureAwait(false));
//        }

//        public static void Write(this Client client, string command)
//        {
//            Task.Run(async () => await client.WriteAsync(command).ConfigureAwait(false));
//        }

//        public static string Read(this Client client)
//        {
//            return Task.Run(async () => await client.ReadAsync().ConfigureAwait(false)).Result;
//        }

//        public static string TerminatedRead(this Client client, string terminator)
//        {
//            return Task.Run(async () => await client.TerminatedReadAsync(terminator).ConfigureAwait(false)).Result;
//        }
//    }
//}
