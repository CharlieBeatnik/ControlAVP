using System;
using System.Collections.Generic;
using ControllableDeviceTypes.OSSCTypes;

namespace ControllableDevice
{
    public class OSSC : IControllableDevice
    {
        private bool _disposed;
        private SerialBlaster _serialBlaster;

        private Dictionary<GenericCommandName, IrCommandCode> _genericCommandNameToCommandCode = new Dictionary<GenericCommandName, IrCommandCode>
        {
            {GenericCommandName.Number1, new IrCommandCode(0x3E29)},
            {GenericCommandName.Number2, new IrCommandCode(0x3EA9)},
            {GenericCommandName.Number3, new IrCommandCode(0x3E69)},
            {GenericCommandName.Number4, new IrCommandCode(0x3EE9)},
            {GenericCommandName.Number5, new IrCommandCode(0x3E19)},
            {GenericCommandName.Number6, new IrCommandCode(0x3E99)},
            {GenericCommandName.Number7, new IrCommandCode(0x3E59)},
            {GenericCommandName.Number8, new IrCommandCode(0x3ED9)},
            {GenericCommandName.Number9, new IrCommandCode(0x3E39)},
            {GenericCommandName.Number0, new IrCommandCode(0x3EC9)},
            {GenericCommandName.Number10, new IrCommandCode(0x3EB9)},
            {GenericCommandName.ToggleReturn, new IrCommandCode(0x3E79)},
            {GenericCommandName.PicCancel, new IrCommandCode(0x3E8D)},
            {GenericCommandName.Menu, new IrCommandCode(0x3E4D)},
            {GenericCommandName.Exit, new IrCommandCode(0x3EED)},
            {GenericCommandName.Info, new IrCommandCode(0x3E65)},
            {GenericCommandName.ClockEject, new IrCommandCode(0x3ED1)},
            {GenericCommandName.Rewind, new IrCommandCode(0x3E61)},
            {GenericCommandName.Forward, new IrCommandCode(0x3EE1)},
            {GenericCommandName.LeftRight, new IrCommandCode(0x3EB5)},
            {GenericCommandName.PauseZoom, new IrCommandCode(0x3EC1)},
            {GenericCommandName.ChapterMinus, new IrCommandCode(0x3E9D)},
            {GenericCommandName.ChapterPlus, new IrCommandCode(0x3E5D)},
            {GenericCommandName.Stop, new IrCommandCode(0x3EA1)},
            {GenericCommandName.Play, new IrCommandCode(0x3E41)},
            {GenericCommandName.Left, new IrCommandCode(0x3EAD)},
            {GenericCommandName.Right, new IrCommandCode(0x3E6D)},
            {GenericCommandName.Up, new IrCommandCode(0x3E2D)},
            {GenericCommandName.Down, new IrCommandCode(0x3ECD)},
            {GenericCommandName.Ok, new IrCommandCode(0x3E1D)},
            {GenericCommandName.Power, new IrCommandCode(0x3E01)},
            {GenericCommandName.VolMinus, new IrCommandCode(0x1CF0)},
            {GenericCommandName.VolPlus, new IrCommandCode(0x1C70)},
            {GenericCommandName.Mute, new IrCommandCode(0x1C18)},
            {GenericCommandName.ChPlus, new IrCommandCode(0x1C50)},
            {GenericCommandName.ChMinus, new IrCommandCode(0x1CD0)},
            {GenericCommandName.TvAv, new IrCommandCode(0x1CC8)},
            {GenericCommandName.Pns, new IrCommandCode(0x1C48)},
            {GenericCommandName.ToneMinus, new IrCommandCode(0x5ED8)},
            {GenericCommandName.TonePlus, new IrCommandCode(0x5E58)},
        };

        private Dictionary<CommandName, GenericCommandName> _commandNameToGenericCommandName = new Dictionary<CommandName, GenericCommandName>
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

        private Dictionary<ProfileName, GenericCommandName> _profileNameToGenericCommandName = new Dictionary<ProfileName, GenericCommandName>
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

        private bool SendCommand(GenericCommandName genericCommandName)
        {
            if (!_genericCommandNameToCommandCode.ContainsKey(genericCommandName))
            {
                throw new ArgumentException("Unknown command name.", nameof(genericCommandName));
            }

            return _serialBlaster.SendCommand(_genericCommandNameToCommandCode[genericCommandName].Protocol, _genericCommandNameToCommandCode[genericCommandName].CodeWithChecksum);
        }

        public bool SendCommand(CommandName commandName)
        {
            return SendCommand(ConvertCommandNameToGenericCommandName(commandName));
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
                throw new ArgumentException($"Unable to convert {commandName.ToString()} to a GenericCommandName.", nameof(commandName));
            }

            return _commandNameToGenericCommandName[commandName];
        }

        private GenericCommandName ConvertProfieNameToGenericCommandName(ProfileName profileName)
        {
            if (!_profileNameToGenericCommandName.ContainsKey(profileName))
            {
                throw new ArgumentException($"Unable to convert {profileName.ToString()} to a GenericCommandName.", nameof(profileName));
            }

            return _profileNameToGenericCommandName[profileName];
        }
    }
}
