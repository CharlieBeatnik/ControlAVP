using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading;
using ControllableDeviceTypes.RetroTink4KTypes;


namespace ControllableDevice
{
    public class RetroTink4K : IControllableDevice
    {
        private bool _disposed;
        private readonly SerialBlaster _serialBlaster;

        private readonly Dictionary<GenericCommandName, IrCommandCode> _genericCommandNameToCommandCode = new Dictionary<GenericCommandName, IrCommandCode>
        {
            {GenericCommandName.Power, new IrCommandCode(0xE51AB649)},
            {GenericCommandName.Input, new IrCommandCode(0xEE11B649)},
            {GenericCommandName.Out, new IrCommandCode(0xDF20B649)},
            {GenericCommandName.Scl, new IrCommandCode(0xDE21B649)},
            {GenericCommandName.Sfx, new IrCommandCode(0xDD22B649)},
            {GenericCommandName.Adc, new IrCommandCode(0xDC23B649)},
            {GenericCommandName.Prof, new IrCommandCode(0xDB24B649)},
            {GenericCommandName.Number1, new IrCommandCode(0xF40BB649)},
            {GenericCommandName.Number2, new IrCommandCode(0xF807B649)},
            {GenericCommandName.Number3, new IrCommandCode(0xFC03B649)},
            {GenericCommandName.Number4, new IrCommandCode(0xF50AB649)},
            {GenericCommandName.Number5, new IrCommandCode(0xF906B649)},
            {GenericCommandName.Number6, new IrCommandCode(0xFD02B649)},
            {GenericCommandName.Number7, new IrCommandCode(0xF609B649)},
            {GenericCommandName.Number8, new IrCommandCode(0xFA05B649)},
            {GenericCommandName.Number9, new IrCommandCode(0xFE01B649)},
            {GenericCommandName.Number10, new IrCommandCode(0xDA25B649)},
            {GenericCommandName.Number11, new IrCommandCode(0xD926B649)},
            {GenericCommandName.Number12, new IrCommandCode(0xD827B649)},
            {GenericCommandName.Menu, new IrCommandCode(0xA35CB649)},
            {GenericCommandName.Back, new IrCommandCode(0xBD42B649)},
            {GenericCommandName.Diag, new IrCommandCode(0xD728B649)},
            {GenericCommandName.Stat, new IrCommandCode(0xD629B649)},
            {GenericCommandName.Up, new IrCommandCode(0xE718B649)},
            {GenericCommandName.Down, new IrCommandCode(0xEF10B649)},
            {GenericCommandName.Left, new IrCommandCode(0xA857B649)},
            {GenericCommandName.Right, new IrCommandCode(0xB04FB649)},
            {GenericCommandName.Enter, new IrCommandCode(0xAC53B649)},
            {GenericCommandName.AutoGain, new IrCommandCode(0xD42BB649)},
            {GenericCommandName.AutoPha, new IrCommandCode(0xD22DB649)},
            {GenericCommandName.GenSync, new IrCommandCode(0xD32CB649)},
            {GenericCommandName.GenBug, new IrCommandCode(0xD02FB649)},
            {GenericCommandName.PlayPause, new IrCommandCode(0xA956B649)},
            {GenericCommandName.Safe, new IrCommandCode(0xD12EB649)},
            {GenericCommandName.Res4K, new IrCommandCode(0xCF30B649)},
            {GenericCommandName.Res1080p, new IrCommandCode(0xCE31B649)},
            {GenericCommandName.Res1440p, new IrCommandCode(0xCD32B649)},
            {GenericCommandName.Res480p, new IrCommandCode(0xCC33B649)},
            {GenericCommandName.Res1, new IrCommandCode(0xCB34B649)},
            {GenericCommandName.Res2, new IrCommandCode(0xCA35B649)},
            {GenericCommandName.Res3, new IrCommandCode(0xC936B649)},
            {GenericCommandName.Res4, new IrCommandCode(0xC837B649)},
            {GenericCommandName.Aux1, new IrCommandCode(0xC738B649)},
            {GenericCommandName.Aux2, new IrCommandCode(0xC639B649)},
            {GenericCommandName.Aux3, new IrCommandCode(0xC53AB649)},
            {GenericCommandName.Aux4, new IrCommandCode(0xC43BB649)},
            {GenericCommandName.Aux5, new IrCommandCode(0xC33CB649)},
            {GenericCommandName.Aux6, new IrCommandCode(0xC23DB649)},
            {GenericCommandName.Aux7, new IrCommandCode(0xC13EB649)},
            {GenericCommandName.Aux8, new IrCommandCode(0xC03FB649)}
        };

        private readonly Dictionary<CommandName, GenericCommandName> _commandNameToGenericCommandName = new Dictionary<CommandName, GenericCommandName>
        {
            {CommandName.Power, GenericCommandName.Power},
            {CommandName.ShowInputSourceMenu, GenericCommandName.Input},
            {CommandName.ShowHdmiOutputMenu, GenericCommandName.Out},
            {CommandName.ShowScalingAndCroppingMenu, GenericCommandName.Scl},
            {CommandName.ShowProcessingEffectsMenu, GenericCommandName.Sfx},
            {CommandName.ShowAdcMenu, GenericCommandName.Adc},
            {CommandName.ShowProfilesMenu, GenericCommandName.Prof},
            {CommandName.LoadProfile1, GenericCommandName.Number1},
            {CommandName.LoadProfile2, GenericCommandName.Number2},
            {CommandName.LoadProfile3, GenericCommandName.Number3},
            {CommandName.LoadProfile4, GenericCommandName.Number4},
            {CommandName.LoadProfile5, GenericCommandName.Number5},
            {CommandName.LoadProfile6, GenericCommandName.Number6},
            {CommandName.LoadProfile7, GenericCommandName.Number7},
            {CommandName.LoadProfile8, GenericCommandName.Number8},
            {CommandName.LoadProfile9, GenericCommandName.Number9},
            {CommandName.LoadProfile10, GenericCommandName.Number10},
            {CommandName.LoadProfile11, GenericCommandName.Number11},
            {CommandName.LoadProfile12, GenericCommandName.Number12},
            {CommandName.ShowMainMenu, GenericCommandName.Menu},
            {CommandName.Back, GenericCommandName.Back},
            {CommandName.ShowDiagnosticsConsoleScreen, GenericCommandName.Diag},
            {CommandName.ShowStatusScreen, GenericCommandName.Stat},
            {CommandName.Up, GenericCommandName.Up},
            {CommandName.Down, GenericCommandName.Down},
            {CommandName.Left, GenericCommandName.Left},
            {CommandName.Right, GenericCommandName.Right},
            {CommandName.Enter, GenericCommandName.Enter},
            {CommandName.AutoCalibrateGain, GenericCommandName.AutoGain},
            {CommandName.AutoCalibratePhase, GenericCommandName.AutoPha},
            {CommandName.SyncLockGenLock, GenericCommandName.GenSync},
            {CommandName.SyncLockTripleBuffer, GenericCommandName.GenBug},
            {CommandName.PauseOrUnpauseCurrentFrame, GenericCommandName.PlayPause},
            {CommandName.SafeMode, GenericCommandName.Safe},
            {CommandName.SwitchResolutionTo4K60, GenericCommandName.Res4K},
            {CommandName.SwitchResolutionTo1080p60, GenericCommandName.Res1080p},
            {CommandName.SwitchResolutionTo1440p60, GenericCommandName.Res1440p},
            {CommandName.SwitchResolutionTo480p60, GenericCommandName.Res480p},
            {CommandName.Res1, GenericCommandName.Res1},
            {CommandName.Res2, GenericCommandName.Res2},
            {CommandName.Res3, GenericCommandName.Res3},
            {CommandName.Res4, GenericCommandName.Res4},
            {CommandName.AutoCropVerticalOnly, GenericCommandName.Aux1},
            {CommandName.AutoCrop43PAR, GenericCommandName.Aux2},
            {CommandName.AutoCrop169PAR, GenericCommandName.Aux3},
            {CommandName.Aux4, GenericCommandName.Aux4},
            {CommandName.Aux5, GenericCommandName.Aux5},
            {CommandName.Aux6, GenericCommandName.Aux6},
            {CommandName.Aux7, GenericCommandName.Aux7},
            {CommandName.Aux8, GenericCommandName.Aux8}
        };

        private readonly Dictionary<ProfileName, CommandName> _profileNameToCommandName = new Dictionary<ProfileName, CommandName>
        {
            {ProfileName.Profile1, CommandName.LoadProfile1},
            {ProfileName.Profile2, CommandName.LoadProfile2},
            {ProfileName.Profile3, CommandName.LoadProfile3},
            {ProfileName.Profile4, CommandName.LoadProfile4},
            {ProfileName.Profile5, CommandName.LoadProfile5},
            {ProfileName.Profile6, CommandName.LoadProfile6},
            {ProfileName.Profile7, CommandName.LoadProfile7},
            {ProfileName.Profile8, CommandName.LoadProfile8},
            {ProfileName.Profile9, CommandName.LoadProfile9},
            {ProfileName.Profile10, CommandName.LoadProfile10},
            {ProfileName.Profile11, CommandName.LoadProfile11},
            {ProfileName.Profile12, CommandName.LoadProfile12}
        };

        public RetroTink4K(SerialBlaster serialBlaster)
        {
            _serialBlaster = serialBlaster;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
            }

            _disposed = true;
        }

        public bool GetAvailable()
        {
            return _serialBlaster.Enabled;
        }

        private bool SendCommand(GenericCommandName genericCommandName, uint repeats = 0)
        {
            if (!_genericCommandNameToCommandCode.ContainsKey(genericCommandName))
            {
                throw new ArgumentException("Unknown command name.", nameof(genericCommandName));
            }

            return _serialBlaster.SendCommand(_genericCommandNameToCommandCode[genericCommandName].Protocol, _genericCommandNameToCommandCode[genericCommandName].Code, repeats);
        }

        private bool SendCommand(CommandName commandName, TimeSpan postSendDelay, uint repeats = 0)
        {
            bool result = SendCommand(ConvertCommandNameToGenericCommandName(commandName), repeats);
            Thread.Sleep(postSendDelay);

            return result;
        }

        public bool SendCountOfCommandWithDelay(CommandName commandName, int count, TimeSpan postSendDelay, uint repeats = 0)
        {
            bool result = true;

            for (int i = 0; i < count; i++)
            {
                result &= SendCommand(ConvertCommandNameToGenericCommandName(commandName), repeats);
                Thread.Sleep(postSendDelay);
            }

            return result;
        }

        public bool SendCommand(CommandName commandName, uint repeats = 0)
        {
            return SendCommand(ConvertCommandNameToGenericCommandName(commandName), repeats);
        }

        private GenericCommandName ConvertCommandNameToGenericCommandName(CommandName commandName)
        {
            if (!_commandNameToGenericCommandName.ContainsKey(commandName))
            {
                throw new ArgumentException($"Unable to convert {commandName} to a GenericCommandName.", nameof(commandName));
            }

            return _commandNameToGenericCommandName[commandName];
        }

        public bool LoadProfile(ProfileName profileName)
        {
            bool result = true;
            TimeSpan postSendDelay = TimeSpan.FromMilliseconds(500);

            //Send multiple times for reliability
            for (int i = 0; i < 1; i++)
            {
                result &= SendCommand(_profileNameToCommandName[profileName], postSendDelay);
            }

            return result;
        }
    }
}
