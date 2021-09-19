using System;

namespace ControllableDeviceTypes
{
    namespace RetroTink5xProTypes
    {
        public enum GenericCommandName
        {
            Power,
            Menu,
            Home,
            Left,
            Right,
            Up,
            Down,
            Ok,
            Back,
            VolMinus,
            VolPlus,
            LeftMouse
        };

        public static class GenericCommandNameExtensions
        {
            public static bool Valid(this GenericCommandName genericCommandName)
            {
                return Enum.IsDefined(typeof(GenericCommandName), genericCommandName);
            }
        }
    }
}
