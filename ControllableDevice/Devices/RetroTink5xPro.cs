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
        private readonly SerialBlaster _serialBlaster;

        private readonly Dictionary<GenericCommandName, IrCommandCode> _genericCommandNameToCommandCode = new Dictionary<GenericCommandName, IrCommandCode>
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
            {GenericCommandName.Number0, new IrCommandCode(0xE144728D)},
            {GenericCommandName.Number1, new IrCommandCode(0xE144A25D)},
            {GenericCommandName.Number2, new IrCommandCode(0xE144629D)},
            {GenericCommandName.Number3, new IrCommandCode(0xE144E21D)},
            {GenericCommandName.Number4, new IrCommandCode(0xE14412ED)},
            {GenericCommandName.Number5, new IrCommandCode(0xE144926D)},
            {GenericCommandName.Number6, new IrCommandCode(0xE14452AD)},
            {GenericCommandName.Number7, new IrCommandCode(0xE144D22D)},
            {GenericCommandName.Number8, new IrCommandCode(0xE14432CD)},
            {GenericCommandName.Number9, new IrCommandCode(0xE144B24D)},
            {GenericCommandName.NumberPlus10, new IrCommandCode(0xE1448A75)},
        };

        private readonly Dictionary<CommandName, GenericCommandName> _commandNameToGenericCommandName = new Dictionary<CommandName, GenericCommandName>
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
            {CommandName.Back, GenericCommandName.Back },
            {CommandName.LoadProfileDefault, GenericCommandName.NumberPlus10 },
            {CommandName.LoadProfile1, GenericCommandName.Number1 },
            {CommandName.LoadProfile2, GenericCommandName.Number2 },
            {CommandName.LoadProfile3, GenericCommandName.Number3 },
            {CommandName.LoadProfile4, GenericCommandName.Number4 },
            {CommandName.LoadProfile5, GenericCommandName.Number5 },
            {CommandName.LoadProfile6, GenericCommandName.Number6 },
            {CommandName.LoadProfile7, GenericCommandName.Number7 },
            {CommandName.LoadProfile8, GenericCommandName.Number8 },
            {CommandName.LoadProfile9, GenericCommandName.Number9 },
            {CommandName.LoadProfile10, GenericCommandName.Number0 }
        };

        private readonly Dictionary<ProfileName, CommandName> _profileNameToCommandName = new Dictionary<ProfileName, CommandName>
        {
            {ProfileName.ProfileDefault, CommandName.LoadProfileDefault},
            {ProfileName.Profile1, CommandName.LoadProfile1},
            {ProfileName.Profile2, CommandName.LoadProfile2},
            {ProfileName.Profile3, CommandName.LoadProfile3},
            {ProfileName.Profile4, CommandName.LoadProfile4},
            {ProfileName.Profile5, CommandName.LoadProfile5},
            {ProfileName.Profile6, CommandName.LoadProfile6},
            {ProfileName.Profile7, CommandName.LoadProfile7},
            {ProfileName.Profile8, CommandName.LoadProfile8},
            {ProfileName.Profile9, CommandName.LoadProfile9},
            {ProfileName.Profile10, CommandName.LoadProfile10}
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
                throw new ArgumentException($"Unable to convert {commandName} to a GenericCommandName.", nameof(commandName));
            }

            return _commandNameToGenericCommandName[commandName];
        }

        public bool LoadProfile(ProfileName profileName)
        {
            bool result = true;
            TimeSpan postSendDelay = TimeSpan.FromMilliseconds(500);

            //Send multiple times for reliability
            for (int i = 0; i < 5; i++)
            { 
                result &= SendCommand(_profileNameToCommandName[profileName], postSendDelay);
            }

            return result;
        }
    }
}
