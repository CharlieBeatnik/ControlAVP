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

        public enum CommandName
        {
            Power,
            PageOutputResolution,
            PageInput,
            PageScanlines,
            PageInterpolation,
            PageHorizontalSampling,
            ShiftPictureUp,
            ShiftPictureDown,
            Left,
            Right,
            Up,
            Down,
            Ok,
            Back
        };

        public static class CommandNameExtensions
        {
            public static bool Valid(this CommandName commandName)
            {
                return Enum.IsDefined(typeof(CommandName), commandName);
            }
        }
    }
}
