﻿using System.Net;
using System.Net.NetworkInformation;

namespace ControllableDevice
{
    public class SonyKDL60W855
    {
        private IPAddress _host;
        private  PhysicalAddress _physicalAddress;

        public SonyKDL60W855(IPAddress host, PhysicalAddress physicalAddress)
        {
            _host = host;
            _physicalAddress = physicalAddress;
        }

        public bool TurnOn()
        {
            WakeOnLan.WakeUp(_physicalAddress.ToString());

            //ANDREWDENN_TODO: Need a way of confirming this succeeded
            return true;
        }
    }
}