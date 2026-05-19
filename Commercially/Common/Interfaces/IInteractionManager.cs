using Vintagestory.API.Common;

namespace Commercially.Common.Interfaces
{
    public interface IInteractionManager
    {
        IInteraction GetInteraction(string key, Caller caller, BlockSelection blockSel);
    }
}