using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading;
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
            {CommandName.ShowMainMenu, GenericCommandName.Menu },
            {CommandName.ShowInputSourceMenu, GenericCommandName.Home },
            {CommandName.ShowPostProcessingMenu, GenericCommandName.VolMinus },
            {CommandName.ShowScalingAndCroppingMenu, GenericCommandName.VolPlus },
            {CommandName.ShowHorizontalSamplingMenu, GenericCommandName.LeftMouse },
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

        private bool SendCommand(GenericCommandName genericCommandName, uint repeats = 0)
        {
            if (!_genericCommandNameToCommandCode.ContainsKey(genericCommandName))
            {
                throw new ArgumentException("Unknown command name.", nameof(genericCommandName));
            }

            return _serialBlaster.SendCommand(_genericCommandNameToCommandCode[genericCommandName].Protocol, _genericCommandNameToCommandCode[genericCommandName].CodeWithChecksum, repeats);
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

            for(int i = 0; i < count; i++)
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
                throw new ArgumentException($"Unable to convert {commandName.ToString()} to a GenericCommandName.", nameof(commandName));
            }

            return _commandNameToGenericCommandName[commandName];
        }

        public bool LoadProfile(ProfileName profileName)
        {
            bool result = true;
            TimeSpan postSendDelay = TimeSpan.FromMilliseconds(1000);

            //Reset the device to ensure the main menu is a in a known state
            //Power Off
            result &= SendCommand(CommandName.Power, TimeSpan.FromSeconds(10));
            //Power On
            result &= SendCommand(CommandName.Power, TimeSpan.FromSeconds(10));

            //Enter main menu, cursor should now be in default top-left postion
            result &= SendCommand(CommandName.ShowMainMenu, postSendDelay);

            //Navigate to Load Profile Screen
            result &= SendCommand(CommandName.Up, postSendDelay);
            result &= SendCommand(CommandName.Up, postSendDelay);
            result &= SendCommand(CommandName.Up, postSendDelay);

            //Calculate how many presses are needed to move the cursor to the correct positon
            const int entriesPerColumn = 8;
            int profileIdx = (int)profileName - 1;
            int posX = (int)Math.Floor((float)profileIdx / entriesPerColumn);
            int posY = profileIdx % entriesPerColumn;

            for (int i = 0; i < posX; i++)
            {
                result &= SendCommand(CommandName.Right, postSendDelay);
            }

            for (int i = 0; i < posY; i++)
            {
                result &= SendCommand(CommandName.Down, postSendDelay);
            }

            //Press Ok to load the profile
            result &= SendCommand(CommandName.Ok, postSendDelay);

            //Exit the menu
            result &= SendCommand(CommandName.Back,postSendDelay);
            result &= SendCommand(CommandName.Back);

            return result;
        }
    }
}
