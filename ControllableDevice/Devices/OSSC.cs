using System;
using System.Collections.Generic;
using System.Threading;
using ControllableDeviceTypes.OSSCTypes;

namespace ControllableDevice
{
    public class OSSC : IControllableDevice
    {
        private bool _disposed;
        private readonly SerialBlaster _serialBlaster;

        private readonly Dictionary<GenericCommandName, IrCommandCode> _genericCommandNameToCommandCode = new Dictionary<GenericCommandName, IrCommandCode>
        {
            {GenericCommandName.Number1, new IrCommandCode(0x6B94837C)},
            {GenericCommandName.Number2, new IrCommandCode(0x6A95837C)},
            {GenericCommandName.Number3, new IrCommandCode(0x6996837C)},
            {GenericCommandName.Number4, new IrCommandCode(0x6897837C)},
            {GenericCommandName.Number5, new IrCommandCode(0x6798837C)},
            {GenericCommandName.Number6, new IrCommandCode(0x6699837C)},
            {GenericCommandName.Number7, new IrCommandCode(0x659A837C)},
            {GenericCommandName.Number8, new IrCommandCode(0x649B837C)},
            {GenericCommandName.Number9, new IrCommandCode(0x639C837C)},
            {GenericCommandName.Number0, new IrCommandCode(0x6C93837C)},
            {GenericCommandName.Number10, new IrCommandCode(0x629D837C)},
            {GenericCommandName.ToggleReturn, new IrCommandCode(0x619E837C)},
            {GenericCommandName.PicCancel, new IrCommandCode(0x4EB1837C)},
            {GenericCommandName.Menu, new IrCommandCode(0x4DB2837C)},
            {GenericCommandName.Exit, new IrCommandCode(0x48B7837C)},
            {GenericCommandName.Info, new IrCommandCode(0x59A6837C)},
            {GenericCommandName.ClockEject, new IrCommandCode(0x748B837C)},
            {GenericCommandName.Rewind, new IrCommandCode(0x7986837C)},
            {GenericCommandName.Forward, new IrCommandCode(0x7887837C)},
            {GenericCommandName.LeftRight, new IrCommandCode(0x52AD837C)},
            {GenericCommandName.PauseZoom, new IrCommandCode(0x7C83837C)},
            {GenericCommandName.ChapterMinus, new IrCommandCode(0x46B9837C)},
            {GenericCommandName.ChapterPlus, new IrCommandCode(0x45BA837C)},
            {GenericCommandName.Stop, new IrCommandCode(0x7A85837C)},
            {GenericCommandName.Play, new IrCommandCode(0x7D82837C)},
            {GenericCommandName.Left, new IrCommandCode(0x4AB5837C)},
            {GenericCommandName.Right, new IrCommandCode(0x49B6837C)},
            {GenericCommandName.Up, new IrCommandCode(0x4BB4837C)},
            {GenericCommandName.Down, new IrCommandCode(0x4CB3837C)},
            {GenericCommandName.Ok, new IrCommandCode(0x47B8837C)},
            {GenericCommandName.Power, new IrCommandCode(0x7F80837C)},
            {GenericCommandName.VolMinus, new IrCommandCode(0xF00FC738)},
            {GenericCommandName.VolPlus, new IrCommandCode(0xF10EC738)},
            {GenericCommandName.Mute, new IrCommandCode(0xE718C738)},
            {GenericCommandName.ChPlus, new IrCommandCode(0xF50AC738)},
            {GenericCommandName.ChMinus, new IrCommandCode(0xF40BC738)},
            {GenericCommandName.TvAv, new IrCommandCode(0xEC13C738)},
            {GenericCommandName.Pns, new IrCommandCode(0xED12C738)},
            {GenericCommandName.ToneMinus, new IrCommandCode(0xE41B857A)},
            {GenericCommandName.TonePlus, new IrCommandCode(0xE51A857A)},
        };

        private readonly Dictionary<CommandName, GenericCommandName> _commandNameToGenericCommandName = new Dictionary<CommandName, GenericCommandName>
        {
            {CommandName.AV1RGBS, GenericCommandName.Number1},
            {CommandName.AV1RGsB, GenericCommandName.Number4},
            {CommandName.AV1YPbPr, GenericCommandName.Number7},
            {CommandName.AV2YPbPr, GenericCommandName.Number2},
            {CommandName.AV2RGsB, GenericCommandName.Number5},
            {CommandName.AV3RGBHV, GenericCommandName.Number3},
            {CommandName.AV3RGBS, GenericCommandName.Number6},
            {CommandName.AV3RGsB, GenericCommandName.Number9},
            {CommandName.Menu, GenericCommandName.Menu},
            {CommandName.Back, GenericCommandName.Exit},
            {CommandName.LineX, GenericCommandName.TvAv},
            {CommandName.PhaseMinus, GenericCommandName.ToneMinus},
            {CommandName.PhasePlus, GenericCommandName.TonePlus},
            {CommandName.ScanlineType, GenericCommandName.Mute},
            {CommandName.ScanlineMode, GenericCommandName.Pns},
            {CommandName.ScanlineSizeMinus, GenericCommandName.ChMinus},
            {CommandName.ScanlineSizePlus, GenericCommandName.ChPlus},
            {CommandName.LCDBacklightToggle, GenericCommandName.Power},
            {CommandName.SourceInfo, GenericCommandName.Info}
        };

        private readonly Dictionary<ProfileName, GenericCommandName> _profileNameToGenericCommandName = new Dictionary<ProfileName, GenericCommandName>
        {
            {ProfileName.Profile0, GenericCommandName.Number0},
            {ProfileName.Profile1, GenericCommandName.Number1},
            {ProfileName.Profile2, GenericCommandName.Number2},
            {ProfileName.Profile3, GenericCommandName.Number3},
            {ProfileName.Profile4, GenericCommandName.Number4},
            {ProfileName.Profile5, GenericCommandName.Number5},
            {ProfileName.Profile6, GenericCommandName.Number6},
            {ProfileName.Profile7, GenericCommandName.Number7},
            {ProfileName.Profile8, GenericCommandName.Number8},
            {ProfileName.Profile9, GenericCommandName.Number9},
            {ProfileName.Profile10, GenericCommandName.Number0},
            {ProfileName.Profile11, GenericCommandName.Number1},
            {ProfileName.Profile12, GenericCommandName.Number2},
            {ProfileName.Profile13, GenericCommandName.Number3},
            {ProfileName.Profile14, GenericCommandName.Number4}
        };

        public OSSC(SerialBlaster serialBlaster)
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

        public bool SendCommand(CommandName commandName, uint repeats = 0)
        {
            bool result = true;
            
            //To increase reliability of AV input changes, send command twice with a delay
            if (commandName.ToString().StartsWith("AV"))
            {
                result &= SendCommand(ConvertCommandNameToGenericCommandName(commandName), repeats);
                Thread.Sleep(TimeSpan.FromSeconds(2));
                result &= SendCommand(ConvertCommandNameToGenericCommandName(commandName), repeats);
            }
            else
            {
                result &= SendCommand(ConvertCommandNameToGenericCommandName(commandName), repeats);
            }

            return result;
        }

        public bool LoadProfile(ProfileName profileName)
        {
            bool result = true;

            //Access load profile menu
            result &= SendCommand(GenericCommandName.Number10);

            //Loading profiles 10-14 require an additional send of the load profile command
            if ((int)profileName >= 10)
            {
                result &= SendCommand(GenericCommandName.Number10);
            }

            GenericCommandName numberCommand = ConvertProfieNameToGenericCommandName(profileName);
            result &= SendCommand(numberCommand);

            return result;
        }

        private GenericCommandName ConvertCommandNameToGenericCommandName(CommandName commandName)
        {
            if (!_commandNameToGenericCommandName.ContainsKey(commandName))
            {
                throw new ArgumentException($"Unable to convert {commandName} to a GenericCommandName.", nameof(commandName));
            }

            return _commandNameToGenericCommandName[commandName];
        }

        private GenericCommandName ConvertProfieNameToGenericCommandName(ProfileName profileName)
        {
            if (!_profileNameToGenericCommandName.ContainsKey(profileName))
            {
                throw new ArgumentException($"Unable to convert {profileName} to a GenericCommandName.", nameof(profileName));
            }

            return _profileNameToGenericCommandName[profileName];
        }
    }
}
