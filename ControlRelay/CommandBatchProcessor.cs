using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ControlRelay
{
    internal class CommandBatchProcessor
    {
        List<object> _devices;

        public CommandBatchProcessor(List<object> devices)
        {
            _devices = devices;
        }
    }
}
