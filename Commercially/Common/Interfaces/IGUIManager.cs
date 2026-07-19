using Commercially.Common.Blocks.BlockEntities;
using Vintagestory.API.Common;

namespace Commercially.Common.Interfaces
{
    public interface IGUIManager
    {
        bool OpenGUI(BECommercialBase bECommercialBase, Caller caller, BlockSelection blockSel, string key, string defaultTab = null);
    }
}
