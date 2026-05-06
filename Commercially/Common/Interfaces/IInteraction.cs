using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;

namespace Commercially.Common.Interfaces
{
    public interface IInteraction
    {
        public bool Interact(IWorldAccessor world, Caller caller, BlockSelection blockSel, string key = "default", ITreeAttribute? activationArgs = null);
    }
}
