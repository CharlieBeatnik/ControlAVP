using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ControllableDevice
{
    public class TimestampedMessage
    {
        public TimestampedMessage(string message)
        {
            Recieved = DateTime.Now;
            Message = message;
        }
        public DateTime Recieved { private set; get; }
        public string Message { private set; get; }

        public TimeSpan Age { get { return DateTime.Now - Recieved; } }
    }
}
