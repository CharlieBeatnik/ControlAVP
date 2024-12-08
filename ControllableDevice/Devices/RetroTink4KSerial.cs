using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ControllableDeviceTypes.RetroTink4KTypes;
using Newtonsoft.Json.Linq;
using Windows.UI.Xaml.Media.Animation;

namespace ControllableDevice
{
    public class RetroTink4KSerial : IControllableDevice
    {
        private bool _disposed;
        private readonly Rs232Device _rs232Device;

        private readonly Dictionary<GenericCommandName, string> _genericCommandNameToCommandCode = new Dictionary<GenericCommandName, string>
        {
            {GenericCommandName.Power, "pwr"},
            {GenericCommandName.Input, "input"},
            {GenericCommandName.Out, "output"},
            {GenericCommandName.Scl, "scaler"},
            {GenericCommandName.Sfx, "sfx"},
            {GenericCommandName.Adc, "adc"},
            {GenericCommandName.Prof, "prof"},
            {GenericCommandName.Number1, "prof1"},
            {GenericCommandName.Number2, "prof2"},
            {GenericCommandName.Number3, "prof3"},
            {GenericCommandName.Number4, "prof4"},
            {GenericCommandName.Number5, "prof5"},
            {GenericCommandName.Number6, "prof6"},
            {GenericCommandName.Number7, "prof7"},
            {GenericCommandName.Number8, "prof8"},
            {GenericCommandName.Number9, "prof9"},
            {GenericCommandName.Number10, "prof10"},
            {GenericCommandName.Number11, "prof11"},
            {GenericCommandName.Number12, "prof12"},
            {GenericCommandName.Menu, "menu"},
            {GenericCommandName.Back,"back"},
            {GenericCommandName.Diag, "diag"},
            {GenericCommandName.Stat, "stat"},
            {GenericCommandName.Up, "up"},
            {GenericCommandName.Down, "down"},
            {GenericCommandName.Left, "left"},
            {GenericCommandName.Right, "right"},
            {GenericCommandName.Enter, "ok"},
            {GenericCommandName.AutoGain, "gain"},
            {GenericCommandName.AutoPha, "phase"},
            {GenericCommandName.GenSync, "genlock"},
            {GenericCommandName.GenBuf, "buffer"},
            {GenericCommandName.PlayPause, "pause"},
            {GenericCommandName.Safe, "safe"},
            {GenericCommandName.Res4K, "res4k"},
            {GenericCommandName.Res1080p, "res1080p"},
            {GenericCommandName.Res1440p, "res1440p"},
            {GenericCommandName.Res480p, "res480p"},
            {GenericCommandName.Res1, "res1"},
            {GenericCommandName.Res2, "res2"},
            {GenericCommandName.Res3, "res3"},
            {GenericCommandName.Res4, "res4"},
            {GenericCommandName.Aux1, "aux1"},
            {GenericCommandName.Aux2, "aux2"},
            {GenericCommandName.Aux3, "aux3"},
            {GenericCommandName.Aux4, "aux4"},
            {GenericCommandName.Aux5, "aux5"},
            {GenericCommandName.Aux6, "aux6"},
            {GenericCommandName.Aux7, "aux7"},
            {GenericCommandName.Aux8, "aux8"}
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
            {CommandName.SyncLockTripleBuffer, GenericCommandName.GenBuf},
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

        public RetroTink4KSerial(string portId)
        {
            _rs232Device = new Rs232Device(portId);
            Debug.Assert(_rs232Device != null);

            _rs232Device.BaudRate = 115200;
            _rs232Device.PreWrite = (x) =>
            {
                return x + "\r";
            };
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
                _rs232Device.Dispose();
            }

            _disposed = true;
        }

        public bool GetAvailable()
        {
            return _rs232Device.Enabled;
        }

        private bool SendCommand(GenericCommandName genericCommandName)
        {
            if (!_genericCommandNameToCommandCode.TryGetValue(genericCommandName, out string value))
            {
                throw new ArgumentException("Unknown command name.", nameof(genericCommandName));
            }

            _rs232Device.Write($"remote {value}");
            return true;
        }

        public bool SendCommand(CommandName commandName)
        {
            if (!_rs232Device.Enabled) return false;

            return SendCommand(ConvertCommandNameToGenericCommandName(commandName));
        }

        private GenericCommandName ConvertCommandNameToGenericCommandName(CommandName commandName)
        {
            if (!_commandNameToGenericCommandName.TryGetValue(commandName, out GenericCommandName value))
            {
                throw new ArgumentException($"Unable to convert {commandName} to a GenericCommandName.", nameof(commandName));
            }

            return value;
        }

        public bool TurnOn()
        {
            if (!_rs232Device.Enabled) return false;
            _rs232Device.Write("pwr on");
            return true;
        }

        public bool TurnOff()
        {
            if (!_rs232Device.Enabled) return false;
            _rs232Device.Write("pwr off");
            return true;
        }

        public bool LoadProfile(uint profileIndex)
        {
            if (!_rs232Device.Enabled) return false;
            _rs232Device.Write($"SVS input {profileIndex}");
            return true;
        }
    }
}
