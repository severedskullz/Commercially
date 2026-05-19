using ProtoBuf;

namespace Commercially.Common.Registry.Packets
{
    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class OwnableUpdatePacket
    {
        public long ID { get; internal set; } = -1;
        public string Type { get; internal set; }
        public string Name { get; internal set; }
        public string OwnerUID { get; internal set; }
        public string OwnerName { get; internal set; }
        public long ParentId { get; internal set; }
        public int X { get; internal set; }
        public int Y { get; internal set; }
        public int Z { get; internal set; }
        public bool BroadcastWaypoint { get; internal set; }
        public string WaypointIcon { get; internal set; }
        public int WaypointColor { get; internal set; }

        //public Dictionary<string, ShopAccess> Permissions { get; internal set; } = new Dictionary<string, ShopAccess>();
        //public bool StallPermissions { get; internal set; }
        //public string ShortDescription { get; set; }
        //public string Description { get; set; }
        //public string WebHook { get; set; }
        public bool IsRemoval { get; set; }

        public OwnableUpdatePacket() { }
        public OwnableUpdatePacket(OwnableRegistration reg, bool isOwner)
        {
            this.OwnerUID = reg.OwnerUID;
            this.OwnerName = reg.OwnerName;
            this.Type = reg.Type;

            this.ID = reg.ID;
            this.Name = reg.Name;
            this.OwnerName = reg.OwnerName;
            //this.Permissions = reg.Permissions;
            //this.StallPermissions = reg.StallPermissions;
            this.BroadcastWaypoint = reg.BroadcastWaypoint;

            // If we are the owner, we need to know the XYZ of the entity, so always send it to ourselves
            if (reg.BroadcastWaypoint || isOwner)
            {
                this.X = reg.X;
                this.Y = reg.Y;
                this.Z = reg.Z;
            }

            if (BroadcastWaypoint)
            {
                this.WaypointIcon = reg.WaypointIcon;
                this.WaypointColor = reg.WaypointColor;
            }

            /*
            this.ShortDescription = reg.ShortDescription;

            if (isOwner)
            {
                Description = reg.Description;
                WebHook = reg.WebHook;
            }
            */
        }

        public OwnableUpdatePacket(long ID)
        {
            this.ID = ID;
            this.IsRemoval = true;
        }
    }
}