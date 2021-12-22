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
            ShowMainMenu,
            ShowInputSourceMenu,
            ShowPostProcessingMenu,
            ShowScalingAndCroppingMenu,
            ShowHorizontalSamplingMenu,
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

        public enum ProfileName
        {
            Profile1 = 1,
            Profile2 = 2,
            Profile3 = 3,
            Profile4 = 4,
            Profile5 = 5,
            Profile6 = 6,
            Profile7 = 7,
            Profile8 = 8,
            Profile9 = 9,
            Profile10 = 10,
            ProfileDefault = 11,
        };

        public static class ProfileNameExtensions
        {
            public static bool Valid(this ProfileName profileName)
            {
                return Enum.IsDefined(typeof(ProfileName), profileName);
            }
        }
    }
}
