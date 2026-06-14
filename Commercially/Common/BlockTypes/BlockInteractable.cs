using Commercially.Common.Interfaces;
using Commercially.Common.Util;
using Vintagestory.API.Client;
using Vintagestory.API.Common;

namespace Commercially.Common.BlockTypes
{
    public class BlockInteractable : Block
    {
        public override void OnLoaded(ICoreAPI api)
        {
            base.OnLoaded(api);
            this.PlacedPriorityInteract = true;
        }

        public override bool OnBlockInteractStart(IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel)
        {
            IInteractableBlockEntity be = world.BlockAccessor.GetBlockEntity(blockSel.Position) as IInteractableBlockEntity;
            if (be != null)
            {
                return be.OnInteract(world, CallerUtils.ToCaller(byPlayer), blockSel);
            }
            return true;
        }

        public override WorldInteraction[] GetPlacedBlockInteractionHelp(IWorldAccessor world, BlockSelection selection, IPlayer forPlayer)
        {
            BlockEntity commercialEntity = world.BlockAccessor.GetBlockEntity(selection.Position);
            IInteractionManager manager = commercialEntity?.GetBehavior<IInteractionManager>();
            if (manager != null)
            {
                return manager.GetInteractions(world, CallerUtils.ToCaller(forPlayer), commercialEntity, selection);
            }
            return base.GetPlacedBlockInteractionHelp(world, selection, forPlayer);
        }
    }
}