using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ControllableDeviceTypes.OSSCTypes;

namespace ControllableDevice
{
    public class OSSC : IControllableDevice
    {
        private bool _disposed = false;
        private Rs232Device _rs232Device;

        private class CommandCode
        {
            public ushort Code { get; }
            public uint CodeWithChecksum { get; }

            public CommandCode(ushort code)
            {
                Code = code;
                uint code32 = code;
                CodeWithChecksum = ((code32 << 16) & 0xFF000000) | (~(code32 << 8) & 0X00FF0000) | ((code32 << 8) & 0x0000FF00) | (~(code32) & 0X000000FF);
            }
        };

        private Dictionary<CommandName, CommandCode> _commands = new Dictionary<CommandName, CommandCode>
        {
            {CommandName.Number1, new CommandCode(0x3E29)},
            {CommandName.Number2, new CommandCode(0x3EA9)},
            {CommandName.Number3, new CommandCode(0x3E69)},
            {CommandName.Number4, new CommandCode(0x3EE9)},
            {CommandName.Number5, new CommandCode(0x3E19)},
            {CommandName.Number6, new CommandCode(0x3E99)},
            {CommandName.Number7, new CommandCode(0x3E59)},
            {CommandName.Number8, new CommandCode(0x3ED9)},
            {CommandName.Number9, new CommandCode(0x3E39)},
            {CommandName.Number0, new CommandCode(0x3EC9)},
            {CommandName.Number10, new CommandCode(0x3EB9)},
            {CommandName.ToggleReturn, new CommandCode(0x3E79)},
            {CommandName.PicCancel, new CommandCode(0x3E8D)},
            {CommandName.Menu, new CommandCode(0x3E4D)},
            {CommandName.Exit, new CommandCode(0x3EED)},
            {CommandName.Info, new CommandCode(0x3E65)},
            {CommandName.ClockEject, new CommandCode(0x3ED1)},
            {CommandName.Rewind, new CommandCode(0x3E61)},
            {CommandName.Forward, new CommandCode(0x3EE1)},
            {CommandName.LeftRight, new CommandCode(0x3EB5)},
            {CommandName.PauseZoom, new CommandCode(0x3EC1)},
            {CommandName.ChapterMinus, new CommandCode(0x3E9D)},
            {CommandName.ChapterPlus, new CommandCode(0x3E5D)},
            {CommandName.Stop, new CommandCode(0x3EA1)},
            {CommandName.Play, new CommandCode(0x3E41)},
            {CommandName.Left, new CommandCode(0x3EAD)},
            {CommandName.Right, new CommandCode(0x3E6D)},
            {CommandName.Up, new CommandCode(0x3E2D)},
            {CommandName.Down, new CommandCode(0x3ECD)},
            {CommandName.Ok, new CommandCode(0x3E1D)},
            {CommandName.Power, new CommandCode(0x3E01)},
            {CommandName.VolMinus, new CommandCode(0x1CF0)},
            {CommandName.VolPlus, new CommandCode(0x1C70)},
            {CommandName.Mute, new CommandCode(0x1C18)},
            {CommandName.ChPlus, new CommandCode(0x1C50)},
            {CommandName.ChMinus, new CommandCode(0x1CD0)},
            {CommandName.TvAv, new CommandCode(0x1CC8)},
            {CommandName.Pns, new CommandCode(0x1C48)},
            {CommandName.ToneMinus, new CommandCode(0x5ED8)},
            {CommandName.TonePlus, new CommandCode(0x5E58)},

        };

        public OSSC(string portId)
        {
            _rs232Device = new Rs232Device(portId);
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
            return true;
        }

        public bool SendCommand(uint command)
        {
            string commandHex = command.ToString("X8");
            string result = _rs232Device.WriteWithResponse($"send nec 0x{commandHex}", "OK");

            return result != null;
        }

        public bool SendCommand(CommandName commandName)
        {
            if (!_commands.ContainsKey(commandName))
            {
                throw new ArgumentException("Unknown command name.", "commandName");
            }

            return SendCommand(_commands[commandName].CodeWithChecksum);
        }
    }
}
