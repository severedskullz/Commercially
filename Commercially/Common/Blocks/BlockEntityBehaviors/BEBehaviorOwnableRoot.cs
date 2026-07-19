using Commercially.Common.Interfaces;
using System.Collections.Generic;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;

namespace Commercially.Common.Blocks.BlockEntityBehaviors
{
    public class BEBehaviorOwnableRoot : BEBehaviorOwnableReferenced, IOwnableRoot
    {
        public BEBehaviorOwnableRoot(BlockEntity blockentity) : base(blockentity)
        {
        }

        public override void Initialize(ICoreAPI api, JsonObject properties)
        {
            base.Initialize(api, properties);

        }

        public override void ToTreeAttributes(ITreeAttribute tree)
        {
            base.ToTreeAttributes(tree);
        }

        public override void FromTreeAttributes(ITreeAttribute tree, IWorldAccessor world)
        {
            base.FromTreeAttributes(tree, world);
        }

        public List<IOwnable> GetChildren()
        {
            throw new System.NotImplementedException();
        }
    }
}
