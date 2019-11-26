using ControllableDevice;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tests
{
    public enum DummyDeviceSetting
    {
        Setting1,
        Setting2
    }

    public class DummyDevice : IControllableDevice
    {
        private bool _disposed = false;

        private bool _invalid = false;

        private string _x;
        private int _y;
        private float _z;

        public DummyDevice(bool invalid = false)
        {
            _invalid = invalid;
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
            if(_invalid)
            {
                return false;
            }
            else return true;
        }

        public bool SetSomething(string x, int y, float z)
        {
            _x = x; _y = y; _z = z;

            if (_invalid)
            {
                return false;
            }
            else return true;
        }

        public int? GetSomething(string x, int y, float z)
        {
            _x = x; _y = y; _z = z;

            if (_invalid)
            {
                return null;
            }
            else return 0;
        }

        public bool SetSomethingNoParameters()
        {
            if (_invalid)
            {
                return false;
            }
            else return true;
        }

        public DummyDeviceSetting? GetEnum(DummyDeviceSetting dummyDeviceSetting)
        {
            if (_invalid)
            {
                return null;
            }
            else return dummyDeviceSetting;
        }
    }
}
