using System;

namespace ControllableDeviceTypes
{
    namespace OSSCTypes
    {
        public enum ProfileName
        {
            Profile0 = 0,
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
            Profile11 = 11,
            Profile12 = 12,
            Profile13 = 13,
            Profile14 = 14
        };

        public static class ProfileNameExtensions
        {
            public static bool Valid(this ProfileName profileName)
            {
                return Enum.IsDefined(typeof(ProfileName), profileName);
            }
        }

        //http://junkerhq.net/xrgb/index.php?title=OSSC_L336
        public enum GenericCommandName
        {
            Number1,
            Number2,
            Number3,
            Number4,
            Number5,
            Number6,
            Number7,
            Number8,
            Number9,
            Number0,
            Number10,
            ToggleReturn,
            PicCancel,
            Menu,
            Exit,
            Info,
            ClockEject,
            Rewind,
            Forward,
            LeftRight,
            PauseZoom,
            ChapterMinus,
            ChapterPlus,
            Stop,
            Play,
            Left,
            Right,
            Up,
            Down,
            Ok,
            Power,
            VolMinus,
            VolPlus,
            Mute,
            ChPlus,
            ChMinus,
            TvAv,
            Pns,
            ToneMinus,
            TonePlus
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
            AV1RGBS,
            AV1RGsB,
            AV1YPbPr,
            AV2YPbPr,
            AV2RGsB,
            AV3RGBHV,
            AV3RGBS,
            AV3RGsB,
            Menu,
            Back,
            LineX,
            PhaseMinus,
            PhasePlus,
            ScanlineType,
            ScanlineMode,
            ScanlineSizeMinus,
            ScanlineSizePlus,
            LCDBacklightToggle,
            SourceInfo
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
