using Commercially.Common.Interfaces;
using System.IO;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Datastructures;
using Vintagestory.API.Server;

namespace Commercially.Common.Blocks.BlockEntityBehaviors
{
    public class BEBehaviorOwnableChild : BEBehaviorOwnableReferenced, IOwnableChild
    {
        public BEBehaviorOwnableChild(BlockEntity blockentity) : base(blockentity)
        {
        }

        public long? ParentID { get; set; }

        public string ParentType {  get; set; }

        public string[] AllowedTypes { get; private set; }

        public override void Initialize(ICoreAPI api, JsonObject properties)
        {
            base.Initialize(api, properties);
            AllowedTypes = properties["allowedTypes"].AsArray<string>(["Generic"]);
        }

        public override void ToTreeAttributes(ITreeAttribute tree)
        {
            base.ToTreeAttributes(tree);
            if (ParentID.HasValue)
            {
                tree.SetLong("ParentID", ParentID.Value);
            }
            if (ParentType != null)
            {
                tree.SetString("ParentType", ParentType);
            }
        }

        public override void FromTreeAttributes(ITreeAttribute tree, IWorldAccessor world)
        {
            base.FromTreeAttributes(tree, world);
            ParentID = tree.TryGetLong("ParentID");
            ParentType = tree.GetString("ParentType");
        }

        public IOwnable GetParent()
        {
            //TODO: This will be very expensive. Lets figure out if we actually want to implement this or not.
            throw new System.NotImplementedException();
        }


        public void SetParent(IOwnableRoot root)
        {
            SetParent(root.ID);
        }

        public void SetParent(long parentId)
        {
            this.ParentID = parentId;
            this.GetBlockEntity().MarkDirty();
        }

        public string[] GetAllowedParentTypes()
        {
            return AllowedTypes;
        }

        public override void OnReceivedClientPacket(IPlayer player, int packetid, byte[] data)
        {
            if (packetid ==  CommerciallyConstants.SET_PARENT_ID)
            {
                using (MemoryStream ms = new MemoryStream(data))
                {

                    if (player.PlayerUID == OwnerUID)
                    {
                        BinaryReader reader = new BinaryReader(ms);
                        int parentId = reader.ReadInt32();
                        SetParent(parentId);
                        UpdateOwnership(OwnerUID, OwnerName, Name, IsAdminOwned);
                    }
                    else
                    {
                        ((IServerPlayer)player).SendMessage(0, Lang.Get("commercially:doesnt-own", []), EnumChatType.OwnMessage);
                    }
                }
            } else
            {
                base.OnReceivedClientPacket(player, packetid, data);
            }
        }
    }
}
