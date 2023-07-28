using ControllableDeviceTypes.SerialBlasterTypes;

namespace ControllableDevice
{
    internal class IrCommandCode
    {
        public uint Code { get; }

        public Protocol Protocol { get;  }

        public IrCommandCode(ushort code, Protocol protocol = Protocol.Nec)
        {
            Protocol = protocol;

            //Generate 32bit code from a 16bit by generating the checksums
            uint code32 = code;
            Code = ((code32 << 16) & 0xFF000000) | (~(code32 << 8) & 0X00FF0000) | ((code32 << 8) & 0x0000FF00) | (~(code32) & 0X000000FF);
        }

        public IrCommandCode(uint code, Protocol protocol = Protocol.Nec)
        {
            Protocol = protocol;
            Code = code;
        }
    }
}
