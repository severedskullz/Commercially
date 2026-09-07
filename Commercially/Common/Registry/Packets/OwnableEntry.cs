using ProtoBuf;

namespace Commercially.Common.Registry.Packets
{
    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class OwnableEntry
    {
        public long ID { get; set; } = -1;
        public string OwnableType { get; set; }
        public string Name { get; set; }
        public string OwnerName { get; set; }
        public string Description { get; set; }
        public string ShortDescription { get; set; }
        public string ImageURL { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public int Z { get; set; }
        public int WorldX { get; set; }
        public int WorldZ { get; set; }
        public bool IsWaypointBroadcasted { get; set; }
    }
}
