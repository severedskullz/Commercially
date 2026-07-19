using Vintagestory.API.Common;
using Vintagestory.GameContent;

namespace Commercially.Common.Blocks.BlockEntityBehaviors
{
    public class BEBehaviorInventoryProvider : BlockEntityBehavior
    {
        InWorldContainer container;

        public BEBehaviorInventoryProvider(BlockEntity blockentity) : base(blockentity)
        {
        }
    }
}
