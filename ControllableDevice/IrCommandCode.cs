
namespace ControllableDevice
{
    internal class IrCommandCode
    {
        public ushort Code { get; }
        public uint CodeWithChecksum { get; }

        public SerialBlaster.Protocol Protocol { get;  }

        public IrCommandCode(ushort code, SerialBlaster.Protocol protocol = SerialBlaster.Protocol.Nec)
        {
            Code = code;
            Protocol = protocol;

            uint code32 = code;
            CodeWithChecksum = ((code32 << 16) & 0xFF000000) | (~(code32 << 8) & 0X00FF0000) | ((code32 << 8) & 0x0000FF00) | (~(code32) & 0X000000FF);
        }
    }
}
