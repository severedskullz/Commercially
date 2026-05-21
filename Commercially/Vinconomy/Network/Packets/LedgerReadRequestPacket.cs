using ProtoBuf;

namespace Vinconomy.Network.Packets
{
    [ProtoContract]
    public class LedgerReadRequestPacket
    {
        [ProtoMember(1)]
        public int shopId { get; set; }

        public LedgerReadRequestPacket() { }
    }
}