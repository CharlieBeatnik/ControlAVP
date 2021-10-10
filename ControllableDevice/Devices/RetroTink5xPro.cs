using System;
using System.Collections.Generic;
using ControllableDeviceTypes.RetroTink5xProTypes;


namespace ControllableDevice
{
    public class RetroTink5xPro : IControllableDevice
    {
        private bool _disposed;
        private SerialBlaster _serialBlaster;

        private Dictionary<GenericCommandName, IrCommandCode> _genericCommandNameToCommandCode = new Dictionary<GenericCommandName, IrCommandCode>
        {
            {GenericCommandName.Power, new IrCommandCode(0x0181)},
            {GenericCommandName.Menu, new IrCommandCode(0x01C1)},
            {GenericCommandName.Home, new IrCommandCode(0x01CE)},
            {GenericCommandName.Left, new IrCommandCode(0x01EC)},
            {GenericCommandName.Right, new IrCommandCode(0x019C)},
            {GenericCommandName.Up, new IrCommandCode(0x011C)},
            {GenericCommandName.Down, new IrCommandCode(0x0102)},
            {GenericCommandName.Ok, new IrCommandCode(0x01C8)},
            {GenericCommandName.Back, new IrCommandCode(0x01E4)},
            {GenericCommandName.VolMinus, new IrCommandCode(0x0191)},
            {GenericCommandName.VolPlus, new IrCommandCode(0x01E1)},
            {GenericCommandName.LeftMouse, new IrCommandCode(0x0112)},
        };

        private Dictionary<CommandName, GenericCommandName> _commandNameToGenericCommandName = new Dictionary<CommandName, GenericCommandName>
        {
            {CommandName.Power, GenericCommandName.Power },
            {CommandName.PageOutputResolution, GenericCommandName.Menu },
            {CommandName.PageInput, GenericCommandName.Home },
            {CommandName.PageScanlines, GenericCommandName.VolMinus },
            {CommandName.PageInterpolation, GenericCommandName.VolPlus },
            {CommandName.PageHorizontalSampling, GenericCommandName.LeftMouse },
            {CommandName.ShiftPictureUp, GenericCommandName.Up },
            {CommandName.ShiftPictureDown, GenericCommandName.Down },
            {CommandName.Left, GenericCommandName.Left },
            {CommandName.Right, GenericCommandName.Right },
            {CommandName.Up, GenericCommandName.Up },
            {CommandName.Down, GenericCommandName.Down },
            {CommandName.Ok, GenericCommandName.Ok },
            {CommandName.Back, GenericCommandName.Back }
        };

        public RetroTink5xPro(SerialBlaster serialBlaster)
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

        private GenericCommandName ConvertCommandNameToGenericCommandName(CommandName commandName)
        {
            if (!_commandNameToGenericCommandName.ContainsKey(commandName))
            {
                throw new ArgumentException($"Unable to convert {commandName.ToString()} to a GenericCommandName.", nameof(commandName));
            }

            return _commandNameToGenericCommandName[commandName];
        }
    }
}
