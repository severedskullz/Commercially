using Commercially.Common.Util;
using System;
using Vintagestory.API.Common;

namespace Commercially.Common.Blocks.BlockEntityBehaviors
{
    public class BEBehaviorComponent : BlockEntityBehavior, IBlockEntityComponent
    {
        public BEBehaviorComponent(BlockEntity blockentity) : base(blockentity)
        {

        }

        public BlockEntity GetBlockEntity()
        {
            return this.Blockentity;
        }
    }
}
