using System;
using System.Collections.Generic;
using System.Text;

namespace ControllableDeviceTypes
{
    namespace AtenVS0801HTypes
    {
        public enum InputPort
        {
            Port1 = 1,
            Port2 = 2,
            Port3 = 3,
            Port4 = 4,
            Port5 = 5,
            Port6 = 6,
            Port7 = 7,
            Port8 = 8

        }

        public static class InputPortExtensions
        {
            public static bool Valid(this InputPort inputPort)
            {
                return Enum.IsDefined(typeof(InputPort), inputPort);
            }
        }

        public enum SwitchMode
        {
            Default,
            Next,
            Auto
        };

        public static class SwitchModeExtensions
        {
            public static bool Valid(this SwitchMode switchMode)
            {
                return Enum.IsDefined(typeof(SwitchMode), switchMode);
            }
        }

        public class State
        {
            public InputPort InputPort { get; set; }

            public bool Output { get; set; }

            public SwitchMode Mode { get; set; }

            public bool GoTo { get; set; }

            public Version Firmware { get; set; }
        }
    }
}
