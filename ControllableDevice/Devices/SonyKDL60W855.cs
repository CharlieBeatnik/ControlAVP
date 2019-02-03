using System;
using System.Net;
using System.Net.NetworkInformation;

namespace ControllableDevice
{
    public class SonyKDL60W855 : IControllableDevice
    {
        private bool _disposed = false;
        private IPAddress _host;
        private  PhysicalAddress _physicalAddress;

        public SonyKDL60W855(IPAddress host, PhysicalAddress physicalAddress)
        {
            _host = host;
            _physicalAddress = physicalAddress;
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
            }

            _disposed = true;
        }

        public bool GetAvailable()
        {
            //ANDREWDENN_TODO
            return false;
        }

        public bool TurnOn()
        {
            WakeOnLan.WakeUp(_physicalAddress.ToString());

            //ANDREWDENN_TODO: Need a way of confirming this succeeded
            return true;
        }
    }
}