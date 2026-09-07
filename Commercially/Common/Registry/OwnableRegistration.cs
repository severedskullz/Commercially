using Commercially.Common.Registry.Packets;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;

namespace Commercially.Common.Registry
{
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

        public long ID { get;  set; }
        public string Type { get; set; }
        public string Name { get; set; }
        public string OwnerUID { get; set; }
        public string OwnerName { get; set; }
        public long? ParentId { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public int Z { get; set; }
        public bool BroadcastWaypoint { get; set; }
        public string WaypointIcon { get; set; }
        public int WaypointColor { get; set; }

        



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

        /// <summary>
        /// Loads any additional data that might be needed for this type of Ownable
        /// </summary>
        public virtual void Initialize() {

        }
    }
}