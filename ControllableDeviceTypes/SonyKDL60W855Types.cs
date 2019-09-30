using System;
using System.Collections.Generic;
using System.Text;

namespace ControllableDeviceTypes
{
    namespace SonyKDL60W855Types
    {
        public enum PowerStatus
        {
            Off,
            Standby,
            On
        }

        public enum InputPort
        {
            Hdmi1,
            Hdmi2,
            Hdmi3,
            Hdmi4,
            Component1,
            Composite1,
            Scart1
        }

        public static class InputPortExtensions
        {
            public static bool Valid(this InputPort inputPort)
            {
                return Enum.IsDefined(typeof(InputPort), inputPort);
            }
        }
    }
}
