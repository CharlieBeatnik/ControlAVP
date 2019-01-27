using System;
using System.Collections.Generic;
using System.Text;

namespace ControllableDeviceTypes
{
    namespace ExtronDSC301HDTypes
    {
        public enum InputPort
        {
            Composite = 1,
            RGBYUV= 2,
            HDMI = 3
        }

        public enum InputVideoFormat
        {
            RGB = 1,
            YUV = 2,
            Composite = 3,
            HDMI = 4
        }

    }
}
