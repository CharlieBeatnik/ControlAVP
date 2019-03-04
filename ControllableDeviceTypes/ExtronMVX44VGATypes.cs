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

        public enum InputPort
        {
            NoTie = 0,
            Port1 = 1,
            Port2 = 2,
            Port3 = 3,
            Port4 = 4
        }

        public static class InputPortExtensions
        {
            public static bool Valid(this InputPort inputPort)
            {
                return Enum.IsDefined(typeof(InputPort), inputPort);
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
            Audio,
            AudioVideo
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
            public Dictionary<OutputPort, InputPort> Video = new Dictionary<OutputPort, InputPort>();
            public Dictionary<OutputPort, InputPort> Audio = new Dictionary<OutputPort, InputPort>();

        }
    }
}
