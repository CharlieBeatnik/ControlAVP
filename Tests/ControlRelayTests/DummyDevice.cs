using ControllableDevice;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Tests
{
    public enum DummyDeviceSetting
    {
        Setting1 = 1,
        Setting2 = 2
    }

    public class DummyDeviceFieldType
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1051:Do not declare visible instance fields", Justification = "Test setting public fields")]
        public int fI;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1051:Do not declare visible instance fields", Justification = "Test setting public fields")]
        public DummyDeviceSetting fE;
    }

    public class DummyDevicePropertyType
    {
        public int pI { get; set; }
        public DummyDeviceSetting pE { get; set; }
    }

    public class DummyDevice : IControllableDevice
    {
        private bool _disposed = false;

        private bool _invalid = false;

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

        public bool SumValuesAndReturnEquality(string a, int b, float c, float answer)
        {
            if (_invalid)
            {
                return false;
            }
            else return(float.Parse(a) + b + c) == answer;
        }

        public int? SumValuesAndReturnAnswer(string a, int b, float c)
        {
            if (_invalid)
            {
                return null;
            }
            else return (int)(float.Parse(a) + b + c);
        }

        public bool PassFieldTypeAndReturnEquality(DummyDeviceFieldType fieldType, int i, DummyDeviceSetting e)
        {
            if (_invalid)
            {
                return false;
            }
            else return fieldType?.fI == i && fieldType.fE == e;
        }

        public bool PassPropertyTypeAndReturnEquality(DummyDevicePropertyType propertyType, int i, DummyDeviceSetting e)
        {
            if (_invalid)
            {
                return false;
            }
            else return propertyType?.pI == i && propertyType.pE == e;
        }

        public bool NoParameters()
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
