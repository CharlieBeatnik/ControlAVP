using System;
using System.Collections.Generic;
using System.Text;

namespace ControllableDeviceTypes
{
    namespace ExtronMVX44VGATypes
    {
        public enum ResetType
        {
            GlobalPresets,
            AudioInputLevels,
            AudioOutputLevels,
            AllMutes,
            AllRGBDelaySettings,
            Full
        }

        public static class ResetTypeExtensions
        {
            public static bool Valid(this ResetType resetType)
            {
                return Enum.IsDefined(typeof(ResetType), resetType);
            }
        }

        public enum InputTie
        {
            NoTie = 0,
            Port1 = 1,
            Port2 = 2,
            Port3 = 3,
            Port4 = 4
        }

        public static class InputTieExtensions
        {
            public static bool Valid(this InputTie inputTie)
            {
                return Enum.IsDefined(typeof(InputTie), inputTie);
            }
        }

        public enum OutputPort
        {
            Port1 = 1,
            Port2 = 2,
            Port3 = 3,
            Port4 = 4
        }

        public static class OutputPortExtensions
        {
            public static bool Valid(this OutputPort outputPort)
            {
                return Enum.IsDefined(typeof(OutputPort), outputPort);
            }
        }

        public enum TieType
        {
            Video,
            Audio
        }

        public static class TieTypeExtensions
        {
            public static bool Valid(this TieType tieType)
            {
                return Enum.IsDefined(typeof(TieType), tieType);
            }
        }

        public class TieState
        {
            public Dictionary<OutputPort, InputTie> Video = new Dictionary<OutputPort, InputTie>();
            public Dictionary<OutputPort, InputTie> Audio = new Dictionary<OutputPort, InputTie>();

        }
    }
}
