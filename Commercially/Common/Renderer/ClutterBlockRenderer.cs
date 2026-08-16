using Commercially.Common.Interfaces;
using Commercially.Common.Util;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent;

namespace Commercially.Common.Renderer
{
    public class ClutterBlockRenderer : IItemRenderer
    {
        public EnumItemClass GetRendererClass() => EnumItemClass.Block;
        public int GetPriority() => 1;
        public bool ShouldCache(ItemStack stack) => false;

        public bool CanHandle(ItemStack stack)
        {
            return stack.Block is BlockClutter;
        }

        public MeshData CreateMesh(IBlockEntityComponent stall, ItemSlot slot, int index)
        {
            //ICoreClientAPI coreClientAPI = (ICoreClientAPI)stall.Api;

            //Dictionary<string, MultiTextureMeshRef> clutterMeshRefs = ObjectCacheUtil.GetOrCreate(coreClientAPI, "viconClutterMeshesInventory", () => new Dictionary<string, MultiTextureMeshRef>());
            ItemStack stack = slot.Itemstack;
            string type = stack.Attributes.GetString("type", "");
            IShapeTypeProps cprops = (stack.Block as BlockShapeFromAttributes).GetTypeProps(type, stack, null);
            if (cprops == null)
            {
                return null;
            }
            float rotX = stack.Attributes.GetFloat("rotX", 0f);
            float rotY = stack.Attributes.GetFloat("rotY", 0f);
            float rotZ = stack.Attributes.GetFloat("rotZ", 0f);
            string otcode = stack.Attributes.GetString("overrideTextureCode", null);
            MeshData modeldata = (stack.Block as BlockShapeFromAttributes).GetOrCreateMesh(cprops, null, otcode);
            return modeldata.Clone().Rotate(new Vec3f(0.5f, 0.5f, 0.5f), rotX, rotY, rotZ);

        }
    }
}
