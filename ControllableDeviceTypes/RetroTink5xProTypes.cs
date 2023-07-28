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
            LeftMouse,
            Number0,
            Number1,
            Number2,
            Number3,
            Number4,
            Number5,
            Number6,
            Number7,
            Number8,
            Number9,
            NumberPlus10
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
            Back,
            LoadProfileDefault,
            LoadProfile1,
            LoadProfile2,
            LoadProfile3,
            LoadProfile4,
            LoadProfile5,
            LoadProfile6,
            LoadProfile7,
            LoadProfile8,
            LoadProfile9,
            LoadProfile10
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
