using Commercially.Common.Blocks.BlockEntityBehaviors;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;

namespace Commercially.Common.Interfaces
{
    public interface IInteractionManager
    {
        Interaction? GetInteraction(string key, Caller caller, BlockSelection blockSel);
        WorldInteraction[] GetInteractions(IWorldAccessor world, Caller caller, BlockEntity blockEntity, BlockSelection blockSel, string key = "default", ITreeAttribute? activationArgs = null);
        int GetInteractionCount(IWorldAccessor world, Caller caller, BlockEntity blockEntity, BlockSelection blockSel, string key = "default", ITreeAttribute? activationArgs = null);

    }
}