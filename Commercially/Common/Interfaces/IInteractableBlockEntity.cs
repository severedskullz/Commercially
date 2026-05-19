using Vintagestory.API.Common;

namespace Commercially.Common.Interfaces
{
    public interface IInteractableBlockEntity
    {
        public bool OnInteract(IWorldAccessor world, Caller caller, BlockSelection blockSel, string key = "default");
    }
}
