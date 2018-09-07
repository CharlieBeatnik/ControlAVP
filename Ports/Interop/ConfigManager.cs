using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace Ports.Interop
{
    internal class ConfigManager
    {
        //CMAPI
        //CONFIGRET
        //WINAPI
        //CM_Get_Device_Interface_List_SizeW(
        //    _Out_ PULONG         pulLen,
        //    _In_ LPGUID          InterfaceClassGuid,
        //    _In_opt_ DEVINSTID_W pDeviceID,
        //    _In_ ULONG           ulFlags
        //    );
        [DllImport("cfgmgr32.dll", CharSet = CharSet.Auto)]
        public static extern uint CM_Get_Device_Interface_List_Size(out uint size, ref Guid interfaceClassGuid, string deviceID, uint flags);

        //CMAPI
        //CONFIGRET
        //WINAPI
        //CM_Get_Device_Interface_ListW(
        //    _In_ LPGUID                     InterfaceClassGuid,
        //    _In_opt_ DEVINSTID_W            pDeviceID,
        //    _Out_writes_(BufferLen) PZZWSTR Buffer,
        //    _In_ ULONG                      BufferLen,
        //    _In_ ULONG                      ulFlags
        //    );
        [DllImport("cfgmgr32.dll", CharSet = CharSet.Auto)]
        public static extern uint CM_Get_Device_Interface_List(ref Guid interfaceClassGuid, string deviceID, [Out] char[] buffer, uint size, uint flags);

    }
}
