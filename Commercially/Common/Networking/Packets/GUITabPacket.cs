using ProtoBuf;

namespace Commercially.Common.Networking.Packets
{
    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class GUITabPacket
    {
        public string TabCode;
        public byte[] Data;
    }
}
