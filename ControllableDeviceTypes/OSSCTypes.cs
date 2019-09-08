using System;

namespace ControllableDeviceTypes
{
    namespace OSSCTypes
    {
        //http://junkerhq.net/xrgb/index.php?title=OSSC_L336
        public enum CommandName
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

        public static class CommandNameExtensions
        {
            public static bool Valid(this CommandName commandName)
            {
                return Enum.IsDefined(typeof(CommandName), commandName);
            }
        }

    }
}
