using Commercially.Common.BlockEntities;
using Commercially.Common.Interfaces;
using Commercially.Common.Util;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Config;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;

namespace Commercially.Common.BlockTypes
{
    public class BlockCommercialBase : Block
    {

        public override void OnBlockBroken(IWorldAccessor world, BlockPos pos, IPlayer byPlayer, float dropQuantityMultiplier = 1)
        {
            if (byPlayer == null)
                return;

            BECommercialBase entity = world.BlockAccessor.GetBlockEntity<BECommercialBase>(pos);
            if (entity != null && entity.GetOwnerUID() == byPlayer.PlayerUID || byPlayer.WorldData.CurrentGameMode == EnumGameMode.Creative)
            {
                CommerciallyModSystem modSystem = world.Api.ModLoader.GetModSystem<CommerciallyModSystem>();
                if (modSystem != null && !modSystem.OnBlockBroken(this.Code, world, pos, byPlayer, dropQuantityMultiplier))
                {
                    return;
                }
                base.OnBlockBroken(world, pos, byPlayer, dropQuantityMultiplier);
            }

            else if (api.Side == EnumAppSide.Server)
                ((IServerPlayer)byPlayer).SendMessage(0, Lang.Get("commercially:doesnt-own", []), EnumChatType.CommandError, null);
        }

        public override void OnBlockPlaced(IWorldAccessor world, BlockPos blockPos, ItemStack byItemStack = null)
        {
            CommerciallyModSystem modSystem = world.Api.ModLoader.GetModSystem<CommerciallyModSystem>();
            if (modSystem != null)
            {
                modSystem.OnBlockPlaced(this.Code, world, blockPos, byItemStack);
            }
            base.OnBlockPlaced(world, blockPos, byItemStack);
        }

        public override bool TryPlaceBlock(IWorldAccessor world, IPlayer byPlayer, ItemStack itemstack, BlockSelection blockSel, ref string failureCode)
        {
            CommerciallyModSystem modSystem = world.Api.ModLoader.GetModSystem<CommerciallyModSystem>();
            if (modSystem != null && !modSystem.TryPlaceBlock(world, byPlayer, itemstack, blockSel))
            {
                failureCode = "__ignore__";
                return false;
            }
            return base.TryPlaceBlock(world, byPlayer, itemstack, blockSel, ref failureCode);
        }

        public override bool OnBlockInteractStart(IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel)
        {
            //return base.OnBlockInteractStart(world, byPlayer, blockSel);
            BECommercialBase ent = world.BlockAccessor.GetBlockEntity<BECommercialBase>(blockSel.Position);
            if (ent != null)
            {
                return ent.OnInteract(world, CallerUtils.ToCaller(byPlayer), blockSel);
            }
            return false;

        }

        public override bool DoPlaceBlock(IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel, ItemStack byItemStack)
        {
            bool result = base.DoPlaceBlock(world, byPlayer, blockSel, byItemStack);
            if (result)
            {
                BlockEntity entity = world.BlockAccessor.GetBlockEntity(blockSel.Position);
                IOwnable ownable = entity?.GetBehavior<IOwnable>();
                if (ownable != null)
                {
                    ownable.SetOwner(byPlayer);
                }
            }

            return result;
        }

        /*
        public override ItemStack[] GetDrops(IWorldAccessor world, BlockPos pos, IPlayer byPlayer, float dropQuantityMultiplier = 1)
        {
            ItemStack stack = new ItemStack(this);
            return base.GetDrops(world, pos, byPlayer, dropQuantityMultiplier);
        }

        public override ItemStack OnPickBlock(IWorldAccessor world, BlockPos pos)
        {
            return base.OnPickBlock(world, pos);
        }

        */

        public override bool CanCreatureSpawnOn(IBlockAccessor blockAccessor, BlockPos pos, EntityProperties type, BaseSpawnConditions sc)
        {
            return false;
        }

       

        public override Vec4f GetSelectionColor(ICoreClientAPI capi, BlockPos pos)
        {
            var i = capi.World.Player.CurrentBlockSelection.SelectionBoxIndex;
            return i switch
            {
                0 => new Vec4f(1, 1, 1, 1), // White
                _ => new Vec4f(1, 0, 0, 1) // Red
            };
        }

        public override bool DoPartialSelection(IWorldAccessor world, BlockPos pos)
        {
            var i = ((IClientWorldAccessor) world).Player.CurrentBlockSelection.SelectionBoxIndex;
            return i != 0;
        }

        public override Cuboidf[] GetSelectionBoxes(IBlockAccessor blockAccessor, BlockPos pos)
        {
            return base.GetSelectionBoxes(blockAccessor, pos);
            /*
            if (RandomDrawOffset != 0 && SelectionBoxes?.Length >= 1)
            {
                float x = (GameMath.oaatHash(pos.X, 0, pos.Z) % 12) / (24f + 12f * RandomDrawOffset);
                float z = (GameMath.oaatHash(pos.X, 1, pos.Z) % 12) / (24f + 12f * RandomDrawOffset);

                return new Cuboidf[] { SelectionBoxes[0].OffsetCopy(x, 0, z) };
            }

            if (SelectionBoxes?.Length != 1) return SelectionBoxes;

            var chunk = blockAccessor.GetChunkAtBlockPos(pos);
            if (chunk == null) return SelectionBoxes;

            return chunk.AdjustSelectionBoxForDecor(blockAccessor, pos, SelectionBoxes);
            */
        }
    }
}
