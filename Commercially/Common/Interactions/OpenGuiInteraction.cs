using Commercially.Common.Interfaces;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;

namespace Commercially.Common.Interactions
{
    internal class OpenGuiInteraction : IInteraction
    {
        public bool CanHandle(IWorldAccessor world, Caller caller, BlockSelection blockSel, string key = "default", ITreeAttribute activationArgs = null)
        {
            throw new System.NotImplementedException();
        }

        public bool Interact(IWorldAccessor world, Caller caller, BlockSelection blockSel, string key = "default", ITreeAttribute activationArgs = null)
        {
            throw new System.NotImplementedException();
        }
    }
}
