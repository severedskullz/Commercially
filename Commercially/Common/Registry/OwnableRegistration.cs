using ProtoBuf;
using System;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;

namespace Commercially.Common.Registry
{
    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class OwnableRegistration
    {
        public OwnableRegistration()
        {
        }

        public OwnableRegistration(OwnableUpdatePacket item)
        {
            this.ID = item.ID;
            this.Type = item.Type;
            this.Name = item.Name;
            this.OwnerUID = item.OwnerUID;
            this.OwnerName = item.OwnerName;
            this.ParentId = item.ParentId;
            this.X = item.X;
            this.Y = item.Y;
            this.Z = item.Z;
            this.BroadcastWaypoint = item.BroadcastWaypoint;
            this.WaypointIcon = item.WaypointIcon;
            this.WaypointColor = item.WaypointColor;
        }

        public long ID { get; internal set; }
        public string Type { get; internal set; }
        public string Name { get; internal set; }
        public string OwnerUID { get; internal set; }
        public string OwnerName { get; internal set; }
        public long? ParentId { get; internal set; }
        public int X { get; internal set; }
        public int Y { get; internal set; }
        public int Z { get; internal set; }
        public bool BroadcastWaypoint { get; internal set; }
        public string WaypointIcon { get; internal set; }
        public int WaypointColor { get; internal set; }

        



        public BlockPos Position
        {
            get
            {
                if (Y == -1)
                {
                    return null;
                }
                return new BlockPos(X, Y, Z, 0);
            }

            set
            {
                if (value != null)
                {
                    this.X = value.X;
                    this.Y = value.Y;
                    this.Z = value.Z;
                }
                else
                {
                    this.X = 0;
                    this.Y = -1;
                    this.Z = 0;
                }

            }
        }

        public bool CanAccess(IPlayer player)
        {
            return player.PlayerUID == this.OwnerUID;
        }
    }
}