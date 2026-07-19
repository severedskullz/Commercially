using Commercially.Common.Interfaces;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;
using Vintagestory.API.Server;
using Vintagestory.API.Util;
using Vintagestory.GameContent;

namespace Commercially.Common.Blocks.BlockEntityBehaviors
{
    public abstract class BEBehaviorAbstractContainer : BlockEntityBehavior, IInventoryProvider
    {
        public abstract InventoryBase Inventory { get; }

        public BEBehaviorAbstractContainer(BlockEntity blockentity) : base(blockentity)
        {
            
        }

        public override void FromTreeAttributes(ITreeAttribute tree, IWorldAccessor worldAccessForResolve)
        {
            Inventory.FromTreeAttributes(tree);
        }

        public override void ToTreeAttributes(ITreeAttribute tree)
        {
            Inventory.ToTreeAttributes(tree);
        }

        public override void OnReceivedClientPacket(IPlayer player, int packetid, byte[] data)
        {
            //TODO: Fix ordering for this. Need to be checking the owner UUID instead of land claim access and do our "kick if not owner and they modified the inventory" logic

            if (packetid == (int)EnumBlockEntityPacketId.Close)
            {
                player.InventoryManager?.CloseInventory(Inventory);
                data = SerializerUtil.Serialize(new OpenContainerLidPacket(player.Entity.EntityId, false));
                ((ICoreServerAPI)Api).Network.BroadcastBlockEntityPacket(
                    Pos,
                    (int)EnumBlockContainerPacketId.OpenLidOthers,
                    data,
                    (IServerPlayer)player
                );
            }

            if (packetid == (int)EnumBlockEntityPacketId.Open)
            {
                player.InventoryManager?.OpenInventory(Inventory);
                data = SerializerUtil.Serialize(new OpenContainerLidPacket(player.Entity.EntityId, true));
                ((ICoreServerAPI)Api).Network.BroadcastBlockEntityPacket(
                    Pos,
                    (int)EnumBlockContainerPacketId.OpenLidOthers,
                    data,
                    (IServerPlayer)player
                );
            }



            if (!Api.World.Claims.TryAccess(player, Pos, EnumBlockAccessFlags.Use))
            {
                Api.World.Logger.Audit("Player {0} sent an inventory packet to openable container at {1} but has no claim access. Rejected.", player.PlayerName, Pos);
                return;
            }

            if (packetid < 1000)
            {
                Inventory.InvNetworkUtil.HandleClientPacket(player, packetid, data);

                // Tell server to save this chunk to disk, and tell client that there was a change in the inventory and they should update the meshes
                this.Blockentity.MarkDirty(true);
                Api.World.BlockAccessor.GetChunkAtBlockPos(Pos).MarkModified();

                return;
            }
        }

        public override void OnReceivedServerPacket(int packetid, byte[] data)
        {
            IClientWorldAccessor clientWorld = (IClientWorldAccessor)Api.World;

            if (packetid == (int)EnumBlockContainerPacketId.OpenInventory)
            {
                var blockContainer = BlockEntityContainerOpen.FromBytes(data);
                Inventory.FromTreeAttributes(blockContainer.Tree);
                Inventory.ResolveBlocksOrItems();
            }

            if (packetid == (int)EnumBlockEntityPacketId.Close)
            {
                clientWorld.Player.InventoryManager.CloseInventory(Inventory);

            }
        }

        public override void OnBlockBroken(IPlayer byPlayer = null)
        {
            base.OnBlockBroken(byPlayer);
            this.Inventory.DropAll(this.Pos.ToVec3d());
        }

    }
}
