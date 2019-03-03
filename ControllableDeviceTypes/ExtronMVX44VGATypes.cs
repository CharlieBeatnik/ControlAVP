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
    }
}
