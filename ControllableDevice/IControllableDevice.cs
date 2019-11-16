using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ControllableDevice
{
    public interface IControllableDevice : IDisposable
    {
        bool GetAvailable();
    }
}
