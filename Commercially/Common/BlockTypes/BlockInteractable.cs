using Commercially.Common.Interfaces;
using Commercially.Common.Util;
using Vintagestory.API.Common;

namespace Commercially.Common.BlockTypes
{
    public class BlockInteractable : Block
    {
        public override bool OnBlockInteractStart(IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel)
        {
            IInteractableBlockEntity be = world.BlockAccessor.GetBlockEntity(blockSel.Position) as IInteractableBlockEntity;
            if (be != null)
            {
                return be.OnInteract(world, CallerUtils.ToCaller(byPlayer), blockSel);
            }
            return true;
        }

    }
}