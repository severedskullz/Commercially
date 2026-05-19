#nullable enable
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;

namespace Commercially.Common.Interfaces
{
    public interface IInteraction
    {

        public bool CanHandle(IWorldAccessor world, Caller caller, BlockSelection blockSel, string key = "default", ITreeAttribute? activationArgs = null);
        public bool Interact(IWorldAccessor world, Caller caller, BlockSelection blockSel, string key = "default", ITreeAttribute? activationArgs = null);
    }
}
