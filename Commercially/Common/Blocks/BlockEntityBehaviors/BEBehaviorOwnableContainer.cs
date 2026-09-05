using Commercially.Common.Interfaces;
using Commercially.Common.Util;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;
using Vintagestory.API.Server;
using Vintagestory.API.Util;
using Vintagestory.GameContent;

namespace Commercially.Common.Blocks.BlockEntityBehaviors
{
    public abstract class BEBehaviorOwnableContainer : BlockEntityBehavior, IInventoryProvider
    {
        public abstract InventoryBase Inventory { get; }
        IOwnable Ownable;


        public BEBehaviorOwnableContainer(BlockEntity blockentity) : base(blockentity)
        {
            
        }

        public override void Initialize(ICoreAPI api, JsonObject properties)
        {
            base.Initialize(api, properties);
            Ownable = this.GetComponent<IOwnable>();
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

            else if (packetid <= 1000)
            {
                // Actual intent is inventory manipulation. Make sure they have permission!
                if (!CanAccess(player))
                {
                    IServerAPI server = ((ICoreServerAPI)Api).Server;
                    if (!server.IsDedicated && server.Players.Length == 1)
                    {
                        CommerciallyModSystem.PrintClientMessage(player, "Nice Try, but that isn't yours... If this wasn't single player, you would have been kicked.");
                    }
                    else
                    {
                        //((IServerPlayer)player).Disconnect("Nice try, but that wasn't yours. (Tried to access Inventory you didn't own)");
                        CommerciallyModSystem.DisconnectWithReason((IServerPlayer)player, "Tried to access Stall Inventory that they did not own", "Nice Try, but that isn't yours.");
                    }
                    return;
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
                else
                {
                    Inventory.InvNetworkUtil.HandleClientPacket(player, packetid, data);
                    // Tell server to save this chunk to disk, and tell client that there was a change in the inventory and they should update the meshes
                    this.Blockentity.MarkDirty(true);
                    Api.World.BlockAccessor.GetChunkAtBlockPos(Pos).MarkModified();
                }
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

        protected virtual bool CanAccess(IPlayer player)
        {
            return Ownable != null && Ownable.CanAccess(player);
        }

        public override void OnBlockBroken(IPlayer byPlayer = null)
        {
            base.OnBlockBroken(byPlayer);
            this.Inventory.DropAll(this.Pos.ToVec3d());
        }

        public BlockEntity GetBlockEntity()
        {
            return this.Blockentity;
        }
    }
}
