using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ports.Interop;
using System.IO;

namespace Ports
{
    public class ComPort
    {
        public string Name { get; }

        public ComPort(string name)
        {
            Name = name;
        }

        public void Open()
        {
            var file = Kernel.CreateFile(
                Name,
                EFileAccess.GenericRead | EFileAccess.GenericWrite,
                EFileShare.None,
                IntPtr.Zero,
                ECreationDisposition.OpenExisting,
                EFileAttributes.Normal | EFileAttributes.Overlapped,
                IntPtr.Zero);
        }

        static public string[] GetPortNames()
        {
            uint size;
            var comPortGuid = new Guid("86E0D1E0-8089-11D0-9CE4-08003E301F73");
            ConfigManager.CM_Get_Device_Interface_List_Size(out size, ref comPortGuid, null, 0);

            var buffer = new char[size];
            ConfigManager.CM_Get_Device_Interface_List(ref comPortGuid, null, buffer, size, 0);
            var interfaceList = new string(buffer).Split('\0');
            interfaceList = interfaceList.Where(s => !string.IsNullOrEmpty(s)).ToArray();

            return interfaceList;
        }
    }
}