using System;

namespace ControllableDeviceTypes
{
    namespace RetroTink4KTypes
    {
        public enum GenericCommandName
        {
            Power,
            Input,
            Out,
            Scl,
            Sfx,
            Adc,
            Prof,
            Number1,
            Number2,
            Number3,
            Number4,
            Number5,
            Number6,
            Number7,
            Number8,
            Number9,
            Number10,
            Number11,
            Number12,
            Menu,
            Back,
            Diag,
            Stat,
            Up,
            Down,
            Left,
            Right,
            Enter,
            AutoGain,
            AutoPha,
            GenSync,
            GenBuf,
            PlayPause,
            Safe,
            Res4K,
            Res1080p,
            Res1440p,
            Res480p,
            Res1,
            Res2,
            Res3,
            Res4,
            Aux1,
            Aux2,
            Aux3,
            Aux4,
            Aux5,
            Aux6,
            Aux7,
            Aux8
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
            ShowInputSourceMenu,
            ShowHdmiOutputMenu,
            ShowScalingAndCroppingMenu,
            ShowProcessingEffectsMenu,
            ShowAdcMenu,
            ShowProfilesMenu,
            LoadProfile1,
            LoadProfile2,
            LoadProfile3,
            LoadProfile4,
            LoadProfile5,
            LoadProfile6,
            LoadProfile7,
            LoadProfile8,
            LoadProfile9,
            LoadProfile10,
            LoadProfile11,
            LoadProfile12,
            ShowMainMenu,
            Back,
            ShowDiagnosticsConsoleScreen,
            ShowStatusScreen,
            Up,
            Down,
            Left,
            Right,
            Enter,
            AutoCalibrateGain,
            AutoCalibratePhase,
            SyncLockGenLock,
            SyncLockTripleBuffer,
            PauseOrUnpauseCurrentFrame,
            SafeMode,
            SwitchResolutionTo4K60,
            SwitchResolutionTo1080p60,
            SwitchResolutionTo1440p60,
            SwitchResolutionTo480p60,
            Res1,
            Res2,
            Res3,
            Res4,
            AutoCropVerticalOnly,
            AutoCrop43PAR,
            AutoCrop169PAR,
            Aux4,
            Aux5,
            Aux6,
            Aux7,
            Aux8
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
            Profile11 = 11,
            Profile12 = 12
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
