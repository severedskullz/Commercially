using Commercially.Common.BlockEntities;
using Commercially.Common.Interfaces;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Config;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;
using Vintagestory.GameContent;

namespace Commercially.Common.BlockBehaviors
{
    public class BehaviorCommercialEvents : BlockBehavior
    {
        private CommerciallyModSystem modSystem;
        public BehaviorCommercialEvents(Block block) : base(block)
        {
        }


        public override void OnLoaded(ICoreAPI api)
        {
            modSystem = api.ModLoader.GetModSystem<CommerciallyModSystem>();
        }

        public override void OnBlockBroken(IWorldAccessor world, BlockPos pos, IPlayer byPlayer, float dropQuantityMultiplier, ref EnumHandling handling)
        {
            handling = EnumHandling.PassThrough;
            if (byPlayer == null)
                return;

            BECommercialBase entity = world.BlockAccessor.GetBlockEntity<BECommercialBase>(pos);
            if (entity != null && entity.GetOwnerUID() == byPlayer.PlayerUID || byPlayer.WorldData.CurrentGameMode == EnumGameMode.Creative)
            {
                
                if (modSystem != null && !modSystem.OnBlockBroken(block.Code, world, pos, byPlayer, dropQuantityMultiplier))
                {
                    handling = EnumHandling.PreventSubsequent;
                    return;
                }
            }

            else if (byPlayer.Entity.Api.Side == EnumAppSide.Server)
            {
                ((IServerPlayer)byPlayer).SendMessage(0, Lang.Get("commercially:doesnt-own", []), EnumChatType.CommandError, null);
                handling = EnumHandling.PreventSubsequent;
                return;
            }

            if (entity != null)
            {
                entity.OnBlockRemoved();
            }
        }

        public override void OnBlockPlaced(IWorldAccessor world, BlockPos blockPos, ref EnumHandling handling)
        {
            world.Api.Logger.Debug("Calling Step 4: OnBlockPlaced on {0}", world.Api.Side.ToString());
            //We need to set the ownership info here for newly created blocks, otherwise when we try to add the Ownable to the DB, this info will be null
            //world.BlockAccessor.GetBlockEntity(blockPos)?.GetBehavior<IOwnable>()?.SetOwner(lastPlayer);
        }

        //TODO: Another Tyron Inconsistency (tm)! Behavior doesnt have the item stack as a part of the method signature!
        /*
        public void OnBlockPlaced(IWorldAccessor world, BlockPos blockPos, ItemStack byItemStack, ref EnumHandling handling)
        {
            if (modSystem != null)
            {
                modSystem.OnBlockPlaced(block.Code, world, blockPos, byItemStack);
            }

        }
        */

        public override bool CanPlaceBlock(IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel, ref EnumHandling handling, ref string failureCode)
        {
            world.Api.Logger.Debug("Calling Step 2: CanPlaceBlock on {0}", world.Api.Side.ToString());
            return base.CanPlaceBlock(world, byPlayer, blockSel, ref handling, ref failureCode);
        }

        public override bool TryPlaceBlock(IWorldAccessor world, IPlayer byPlayer, ItemStack itemstack, BlockSelection blockSel, ref EnumHandling handling, ref string failureCode)
        {
            world.Api.Logger.Debug("Calling Step 1: TryPlaceBlock on {0}", world.Api.Side.ToString());
            if (modSystem != null && !modSystem.TryPlaceBlock(world, byPlayer, itemstack, blockSel))
            {
                handling = EnumHandling.PreventSubsequent;
                failureCode = "__ignore__";
                return false;
            }
            handling = EnumHandling.PassThrough;
            return true;
        }

        public override bool DoPlaceBlock(IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel, ItemStack byItemStack, ref EnumHandling handling)
        {
            world.Api.Logger.Debug("Calling Step 3: DoPlaceBlock on {0}", world.Api.Side.ToString());
            byItemStack.Attributes.SetString("OwnerUID", byPlayer.PlayerUID);
            byItemStack.Attributes.SetString("OwnerName", byPlayer.PlayerName);
            world.Api.Logger.Debug("byItemStack attributes were set on {0}", world.Api.Side.ToString());
            return base.DoPlaceBlock(world, byPlayer, blockSel, byItemStack, ref handling);
        }

        public override bool OnBlockInteractStart(IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel, ref EnumHandling handling)
        {
            handling = EnumHandling.PreventDefault;
            //return base.OnBlockInteractStart(world, byPlayer, blockSel);
            BECommercialBase ent = world.BlockAccessor.GetBlockEntity<BECommercialBase>(blockSel.Position);
            if (ent == null)
            {
                world.Api.Logger.Debug("Tried to interact with block at {0} but is not a BECommercialBase entity", blockSel.Position);
                return false;
            }
            return ent.OnInteract(world, CallerUtils.ToCaller(byPlayer), blockSel);

        }

        public override bool CanCreatureSpawnOn(IBlockAccessor blockAccessor, BlockPos pos, EntityProperties type, BaseSpawnConditions sc, ref EnumHandling handling)
        {
            handling = EnumHandling.Handled;
            return false;
        }

        public override ItemStack OnPickBlock(IWorldAccessor world, BlockPos pos, ref EnumHandling handling)
        {
            ItemStack stack = new ItemStack(this.block);
            BlockBehaviorHorizontalOrientable orientable = block.GetBehavior<BlockBehaviorHorizontalOrientable>();
            if (orientable != null)
            {
                EnumHandling orientableHandling = EnumHandling.PassThrough;
                stack = orientable.OnPickBlock(world, pos, ref orientableHandling);
            }

            bool applied = ApplyAttributesToItemStack(stack, world, pos);

            if (applied)
            {
                handling = EnumHandling.PreventSubsequent;
            }

            return stack;
        }

        public override ItemStack[] GetDrops(IWorldAccessor world, BlockPos pos, IPlayer byPlayer, ref float dropChanceMultiplier, ref EnumHandling handling)
        {
            bool preventDrops = false;
            ItemStack stack = null;

            // If the block has the BlockBehaviorHorizontalOrientable behavior, we need to call its GetDrops method to ensure that the correct item stack is dropped based on the block's orientation.
            // Otherwise we will simply drop the block ID of whats in the world and not the correct orientation
            BlockBehaviorHorizontalOrientable orientable = block.GetBehavior<BlockBehaviorHorizontalOrientable>();
            if (orientable != null)
            {
                EnumHandling orientableHandling = EnumHandling.PassThrough;
                ItemStack[] drops = orientable.GetDrops(world, pos, byPlayer, ref dropChanceMultiplier, ref orientableHandling);
                if (drops.Length > 0)
                {
                    stack = drops[0];
                    preventDrops = true;
                } else
                {
                    handling = orientableHandling;
                    return [];
                }
            } else
            {
                stack = new ItemStack(this.block);
            }

            bool applied = ApplyAttributesToItemStack(stack, world, pos);
            preventDrops = preventDrops || applied;

            if (preventDrops && stack.ResolveBlockOrItem(world))
            {
                handling = EnumHandling.PreventSubsequent;
                return [stack];
            }

            return null;
        }

        public bool ApplyAttributesToItemStack(ItemStack stack, IWorldAccessor world, BlockPos pos)
        {
            bool applied = false;
            // Check the collectible behaviors for any additional attributes we need to add to the item stack when it drops
            foreach (var behavior in this.block.CollectibleBehaviors)
            {
                if (behavior is IPersistableStackAttributes persistable)
                {
                    persistable.AddAttributes(stack);
                    applied = true;
                }
            }

            // Check the block behaviors for any additional attributes we need to add to the item stack when it drops
            foreach (var behavior in this.block.BlockBehaviors)
            {
                if (behavior is IPersistableStackAttributes persistable)
                {
                    persistable.AddAttributes(stack);
                    applied = true;
                }
            }

            // Check the block entity for any additional attributes we need to add to the item stack when it drops
            // Supports both BlockEntityBehavrious and the BlockEntity itself implementing the IPersistableDropAttributes interface
            BlockEntity commercialEntity = world.BlockAccessor.GetBlockEntity(pos);
            if (commercialEntity != null)
            {
                foreach (var behavior in commercialEntity.Behaviors)
                {
                    if (behavior is IPersistableStackAttributes persistable)
                    {
                        persistable.AddAttributes(stack);
                        applied = true;
                    }
                }

                if (commercialEntity is IPersistableStackAttributes persistableEntity)
                {
                    persistableEntity.AddAttributes(stack);
                    applied = true;
                }
            }

            return applied;
        }
    }
}
