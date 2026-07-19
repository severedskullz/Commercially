using Commercially.Common.Blocks.BlockEntities;
using Commercially.Common.Interfaces;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;

namespace Commercially.Common.Interactions
{
    public class OpenStallInteraction : IInteraction
    {
        public const string Key = "Vinconomy.OpenStall";

        public bool CanHandle(IWorldAccessor world, Caller caller, BlockEntity blockEntity, BlockSelection blockSel, string key = "default", ITreeAttribute activationArgs = null)
        {
            return true;
        }

        public int GetInteractionCount(IWorldAccessor world, Caller caller, BlockEntity blockEntity, BlockSelection blockSel, string key = "default", ITreeAttribute activationArgs = null)
        {
            return 1;
        }

        public WorldInteraction[] GetInteractions(IWorldAccessor world, Caller caller, BlockEntity blockEntity, BlockSelection blockSel, string key = "default", ITreeAttribute activationArgs = null)
        {
           return
           [
               new WorldInteraction()
                {
                    ActionLangCode = "vinconomy:open-stall",
                    MouseButton = EnumMouseButton.Right,
                }
            ];
        }

        public bool Interact(IWorldAccessor world, Caller caller, BlockEntity blockEntity, BlockSelection blockSel, string key = "default", ITreeAttribute activationArgs = null)
        {
            IGUIManager manager = blockEntity.GetBehavior<IGUIManager>();
            bool isOwner = blockEntity.GetBehavior<IOwnableReference>()?.OwnerUID == caller.Player?.PlayerUID;

            return manager.OpenGUI(blockEntity as BECommercialBase, caller, blockSel, key, isOwner ? "Vinconomy.ShopOwner" : "Vinconomy.ShopCustomer");
        }

        public bool ShouldHandle(IWorldAccessor world, Caller caller, BlockEntity blockEntity, BlockSelection blockSel, string key = "default", ITreeAttribute activationArgs = null)
        {
            return true;
        }
    }
}
