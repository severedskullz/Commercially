using Commercially.Common.Interfaces;
using Commercially.Common.Util;
using Vintagestory.API.Client;
using Vintagestory.API.Common;

namespace Commercially.Common.Renderer
{
    public interface IItemRenderer
    {
        public MeshData CreateMesh(IComponent stall, ItemSlot slot, int index);

        public bool CanHandle(ItemStack stack);
        public int GetPriority();
        public EnumItemClass GetRendererClass();

        public bool ShouldCache(ItemStack stack);

    }
}
