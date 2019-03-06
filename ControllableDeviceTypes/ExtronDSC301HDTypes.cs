using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ControllableDeviceTypes
{
    namespace ExtronDSC301HDTypes
    {
        public enum InputPort //X1
        {
            Composite = 1,
            RGBYUV= 2,
            HDMI = 3
        }

        public static class InputPortExtensions
        {
            public static bool Valid(this InputPort inputPort)
            {
                return Enum.IsDefined(typeof(InputPort), inputPort);
            }
        }

        public enum InputVideoFormat //X3
        {
            RGB = 1,
            YUV = 2,
            Composite = 3,
            HDMI = 4
        }

        public static class InputVideoFormatExtensions
        {
            public static bool Valid(this InputVideoFormat inputVideoFormat)
            {
                return Enum.IsDefined(typeof(InputVideoFormat), inputVideoFormat);
            }
        }

        public class Edid //X17
        {
            static private readonly List<Edid> _edid = new List<Edid>()
            {
                //640x480
                new Edid(10, 640, 480, 50.0f),
                new Edid(11, 640, 480, 60.0f),
                new Edid(12, 640, 480, 75.0f),

                //800x600
                new Edid(13, 800, 600, 50.0f),
                new Edid(14, 800, 600, 60.0f),
                new Edid(15, 800, 600, 75.0f),

                //852x480
                new Edid(16, 852, 480, 50.0f),
                new Edid(17, 852, 480, 60.0f),
                new Edid(18, 852, 480, 75.0f),

                //1024x768
                new Edid(19, 1024, 768, 50.0f),
                new Edid(20, 1024, 768, 60.0f),
                new Edid(21, 1024, 768, 75.0f),

                //1024x852
                new Edid(22, 1024, 852, 50.0f),
                new Edid(23, 1024, 852, 60.0f),
                new Edid(24, 1024, 852, 75.0f),

                //1024x1024
                new Edid(25, 1024, 1024, 50.0f),
                new Edid(26, 1024, 1024, 60.0f),
                new Edid(27, 1024, 1024, 75.0f),

                //1280x768
                new Edid(28, 1280, 768, 50.0f),
                new Edid(29, 1280, 768, 60.0f),
                new Edid(30, 1280, 768, 75.0f),

                //1280x800
                new Edid(31, 1280, 800, 50.0f),
                new Edid(32, 1280, 800, 60.0f),
                new Edid(33, 1280, 800, 75.0f),
                
                //1280x1024
                new Edid(34, 1280, 1024, 50.0f),
                new Edid(35, 1280, 1024, 60.0f),
                new Edid(36, 1280, 1024, 75.0f),
                
                //1360x765
                new Edid(37, 1360, 765, 50.0f),
                new Edid(38, 1360, 765, 60.0f),
                new Edid(39, 1360, 765, 75.0f),
                
                //1360x768
                new Edid(40, 1360, 768, 50.0f),
                new Edid(41, 1360, 768, 60.0f),
                new Edid(42, 1360, 768, 75.0f),
                
                //1365x768
                new Edid(43, 1365, 768, 50.0f),
                new Edid(44, 1365, 768, 60.0f),
                new Edid(45, 1365, 768, 75.0f),
                
                //1366x768
                new Edid(46, 1366, 768, 50.0f),
                new Edid(47, 1366, 768, 60.0f),
                new Edid(48, 1366, 768, 75.0f),
                
                //1365x1024
                new Edid(49, 1365, 1024, 50.0f),
                new Edid(50, 1365, 1024, 60.0f),
                new Edid(51, 1365, 1024, 75.0f),
                
                //1440x900
                new Edid(52, 1440, 900, 50.0f),
                new Edid(53, 1440, 900, 60.0f),
                new Edid(54, 1440, 900, 75.0f),

                //1400x1050
                new Edid(55, 1400, 1050, 50.0f),
                new Edid(56, 1400, 1050, 60.0f),

                //1600x900
                new Edid(57, 1600, 900, 50.0f),
                new Edid(58, 1600, 900, 60.0f),
                
                //1680x1050
                new Edid(59, 1680, 1050, 50.0f),
                new Edid(60, 1680, 1050, 60.0f),

                //1600x1200
                new Edid(61, 1600, 1200, 50.0f),
                new Edid(62, 1600, 1200, 60.0f),
                
                //1920x1200
                new Edid(63, 1920, 1200, 50.0f),
                new Edid(64, 1920, 1200, 60.0f),
                
                //480p
                new Edid(65, 640, 480, 59.94f),
                new Edid(66, 640, 480, 60.0f),
                
                //576p
                new Edid(67, 720, 576, 50.0f),

                //720p
                new Edid(68, 1280, 720, 25.0f),
                new Edid(69, 1280, 720, 29.97f),
                new Edid(70, 1280, 720, 30.0f),
                new Edid(71, 1280, 720, 50.0f),
                new Edid(72, 1280, 720, 59.94f),
                new Edid(73, 1280, 720, 60.0f),
            
                //1080i
                new Edid(74, 1920, 1080, 50.0f, ScanMode.Interlaced),
                new Edid(75, 1920, 1080, 59.94f, ScanMode.Interlaced),
                new Edid(76, 1920, 1080, 60.0f, ScanMode.Interlaced),

                //1080p
                new Edid(77, 1920, 1080, 23.98f),
                new Edid(78, 1920, 1080, 24.0f),
                new Edid(79, 1920, 1080, 25.0f),
                new Edid(80, 1920, 1080, 29.97f),
                new Edid(81, 1920, 1080, 30.0f),
                new Edid(82, 1920, 1080, 50.0f),
                new Edid(83, 1920, 1080, 59.94f),
                new Edid(84, 1920, 1080, 60.0f),

                //2k
                new Edid(85, 2048, 1080, 23.98f),
                new Edid(86, 2048, 1080, 24.0f),
                new Edid(87, 2048, 1080, 25.0f),
                new Edid(88, 2048, 1080, 29.97f),
                new Edid(89, 2048, 1080, 30.0f),
                new Edid(90, 2048, 1080, 50.0f),
                new Edid(91, 2048, 1080, 59.94f),
                new Edid(92, 2048, 1080, 60.0f),
            };

            public enum ScanMode
            {
                Progressive,
                Interlaced
            }

            public int Id;
            public int Width;
            public int Height;
            public float RefreshRate;
            public ScanMode Scan;

            private Edid(int id, int width, int height, float refreshRate, ScanMode scan = ScanMode.Progressive)
            {
                Id = id;
                Width = width;
                Height = height;
                RefreshRate = refreshRate;
                Scan = scan;
            }

            static public Edid GetEdid(int id)
            {
                try
                {
                    return _edid.Single(s => s.Id == id);
                }
                catch(InvalidOperationException)
                {
                    return null;
                }
            }

            static public Edid GetEdid(int width, int height, float refreshRate, ScanMode scan = ScanMode.Progressive)
            {
                try
                {
                    return _edid.Single(s => (
                        s.Width == width &&
                        s.Height == height &&
                        s.RefreshRate == refreshRate &&
                        s.Scan == scan));
                }
                catch (InvalidOperationException)
                {
                    return null;
                }
            }
        }

        public enum ScaleType
        {
            Fit,
            PixelPerfect
        }

        public static class ScaleTypeExtensions
        {
            public static bool Valid(this ScaleType scaleType)
            {
                return Enum.IsDefined(typeof(ScaleType), scaleType);
            }
        }

        public enum PositionType
        {
            Centre
        }

        public static class PositionTypeExtensions
        {
            public static bool Valid(this PositionType positionType)
            {
                return Enum.IsDefined(typeof(PositionType), positionType);
            }
        }

        public class PositionAndSize
        {
            public static readonly int HPosMin = -2048;
            public static readonly int HPosMax = 2048;
            public static readonly int VPosMin = -1200;
            public static readonly int VPosMax = 1200;

            public static readonly int HSizeMin = 10;
            public static readonly int HSizeMax = 4096;
            public static readonly int VSizeMin = 10;
            public static readonly int VSizeMax = 2400;

            public PositionAndSize(int hPos, int vPos, int hSize, int vSize)
            {
                HPos = (hPos >= HPosMin && hPos <= HPosMax) ? hPos : 0;
                VPos = (vPos >= VPosMin && vPos <= VPosMax) ? vPos : 0;
                HSize = (hSize >= HSizeMin && hSize <= HSizeMax) ? hSize : 0;
                VSize = (vSize >= VSizeMin && vSize <= VSizeMax) ? vSize : 0;
            }

            public int HPos { get; private set; }
            public int VPos { get; private set; }
            public int HSize { get; private set; }
            public int VSize { get; private set; }

            public override bool Equals(object obj)
            {
                var r = obj as PositionAndSize;
                return r != null &&
                       HPos == r.HPos &&
                       VPos == r.VPos &&
                       HSize == r.HSize &&
                       VSize == r.VSize;
            }

            public override int GetHashCode()
            {
                var hashCode = 674255106;
                hashCode = hashCode * -1521134295 + HPos.GetHashCode();
                hashCode = hashCode * -1521134295 + VPos.GetHashCode();
                hashCode = hashCode * -1521134295 + HSize.GetHashCode();
                hashCode = hashCode * -1521134295 + VSize.GetHashCode();
                return hashCode;
            }
        }
    }
}
