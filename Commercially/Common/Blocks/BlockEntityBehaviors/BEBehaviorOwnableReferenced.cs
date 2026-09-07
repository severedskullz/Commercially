using Commercially.Common.Interfaces;
using Commercially.Common.ModSystems;
using Commercially.Common.Util;
using System.IO;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;

namespace Commercially.Common.Blocks.BlockEntityBehaviors
{
    public class BEBehaviorOwnableReferenced : BEBehaviorOwnable, IOwnableReference, IPersistableStackAttributes
    {
        public long ID { get; private set; }

        public BlockPos Position => this.Pos;
        public virtual bool MaintainOwnershipOnBreak => false;

        public BEBehaviorOwnableReferenced(BlockEntity blockentity) : base(blockentity)
        {
        }

        public override void Initialize(ICoreAPI api, JsonObject properties)
        {
            base.Initialize(api, properties);

        }

        public override void ToTreeAttributes(ITreeAttribute tree)
        {
            base.ToTreeAttributes(tree);
            tree.SetLong("ID", ID);
        }

        public override void FromTreeAttributes(ITreeAttribute tree, IWorldAccessor world)
        {
            base.FromTreeAttributes(tree, world);
            ID = tree.GetLong("ID");
        }

        
        public override void OnBlockPlaced(ItemStack byItemStack)
        {
            base.OnBlockPlaced(byItemStack);
            if (Api.Side == EnumAppSide.Server)
            {
                CommerciallyModSystem modSystem = Api.ModLoader.GetModSystem<CommerciallyModSystem>();
                if (byItemStack.Attributes.HasAttribute("ID"))
                {
                    ID = byItemStack.Attributes.GetLong("ID");
                    modSystem.UpdateOwnable(this);
                }
                else
                {
                    modSystem.AddOwnable(this);
                }
            }
            
        }
        

        public override void OnBlockRemoved()
        {
            ModSystem.RemoveOwnable(this);
            base.OnBlockRemoved();
        }

        public void SetIDInternal(long newId)
        {
            ID = newId;
        }


        public virtual void AddAttributes(ItemStack stack)
        {
            if (MaintainOwnershipOnBreak)
            {
                stack.Attributes.SetLong("ID", ID);
                stack.Attributes.SetString("OwnerUID", OwnerUID);
                stack.Attributes.SetString("OwnerName", OwnerName);
                stack.Attributes.SetString("Name", Name);
            }
        }

        public override void UpdateOwnership(string ownerUID, string ownerName, string name, bool isAdminOwned)
        {
            base.UpdateOwnership(ownerUID, ownerName, name, isAdminOwned);
            ModSystem.UpdateOwnable(this);
        }

        public override void OnReceivedClientPacket(IPlayer player, int packetid, byte[] data)
        {

            if (packetid == CommerciallyConstants.SET_WAYPOINT)
            {
                using (MemoryStream ms = new MemoryStream(data))
                {

                    if (player.PlayerUID == OwnerUID)
                    {
                        BinaryReader reader = new BinaryReader(ms);
                        bool enabled = reader.ReadBoolean();
                        string icon = reader.ReadString();
                        int color = reader.ReadInt32();
                        ModSystem.UpdateOwnableWaypoint(this, enabled, icon, color);
                    }
                    else
                    {
                        ((IServerPlayer)player).SendMessage(0, Lang.Get("viconomy:doesnt-own", []), EnumChatType.OwnMessage);
                    }
                }
                return;
            } 
            else
            {
                base.OnReceivedClientPacket(player, packetid, data);
            }            
         }
    }
}
