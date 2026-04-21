
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;

namespace Commercially.Common.BlockBehaviors
{
    public class TestBehaviour : BlockBehavior
    {
        public TestBehaviour(Block block) : base(block)
        {
        }

        public override bool TryPlaceBlock(IWorldAccessor world, IPlayer byPlayer, ItemStack itemstack, BlockSelection blockSel, ref EnumHandling handling, ref string failureCode)
        {
            world.Api.Logger.Debug("Calling Step 1: TryPlaceBlock on {0}", world.Api.Side.ToString());
            return base.TryPlaceBlock(world, byPlayer, itemstack, blockSel, ref handling, ref failureCode);
        }
        public override bool CanPlaceBlock(IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel, ref EnumHandling handling, ref string failureCode)
        {
            world.Api.Logger.Debug("Calling Step 2: CanPlaceBlock on {0}", world.Api.Side.ToString());
            return base.CanPlaceBlock(world, byPlayer, blockSel, ref handling, ref failureCode);
        }

        public override bool DoPlaceBlock(IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel, ItemStack byItemStack, ref EnumHandling handling)
        {
            world.Api.Logger.Debug("Calling Step 3: DoPlaceBlock on {0}", world.Api.Side.ToString());
            return base.DoPlaceBlock(world, byPlayer, blockSel, byItemStack, ref handling);
        }

        public override void OnBlockPlaced(IWorldAccessor world, BlockPos blockPos, ref EnumHandling handling)
        {
            world.Api.Logger.Debug("Calling Step 4: OnBlockPlaced on {0}", world.Api.Side.ToString());
            base.OnBlockPlaced(world, blockPos, ref handling);
        }

    }
}
