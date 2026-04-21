using Commercially.Common;
using Vintagestory.API.Client;
using Vintagestory.API.Common;

namespace Commercially.Common.Renderer
{
    public interface IItemRenderer
    {
        public MeshData CreateMesh(IShapeTesselator stall, ItemSlot slot, int index);

        public bool CanHandle(ItemStack stack);
        public int GetPriority();
        public EnumItemClass GetRendererClass();

        public bool ShouldCache(ItemStack stack);

    }
}
