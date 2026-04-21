using Commercially.Common;
using System;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.GameContent;

namespace Commercially.Common.Renderer
{
    public class BlockRenderer : IItemRenderer
    {
        public EnumItemClass GetRendererClass() => EnumItemClass.Block;
        public int GetPriority() => 0;
        public bool ShouldCache(ItemStack stack) => true;

        public bool CanHandle(ItemStack stack)
        {
            return stack.Class == EnumItemClass.Block;
        }

        public MeshData CreateMesh(IShapeTesselator stall, ItemSlot slot, int index)
        {
            ItemStack stack = slot.Itemstack;
            ICoreClientAPI coreClientAPI = stall.GetApi() as ICoreClientAPI;
            try
            {
                
                IContainedMeshSource containedMeshSource = stack.Collectible as IContainedMeshSource;
                if (containedMeshSource != null)
                {
                    MeshData modeldata = containedMeshSource.GenMesh(slot, coreClientAPI.BlockTextureAtlas, stall.GetBlockEntity().Pos);
                    if (modeldata != null)
                    {
                        return modeldata;
                    }

                }

                if (stack.Block is BlockGenericTypedContainer)
                {
                    BlockGenericTypedContainer container =  stack.Block as BlockGenericTypedContainer;
                    string type = stack.Attributes.GetAsString("type");
                    MeshData mesh =  container.GenMesh(coreClientAPI, type, stack.ItemAttributes["shape"][type].AsString());
                    return mesh;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }

            return coreClientAPI.TesselatorManager.GetDefaultBlockMesh(stack.Block).Clone();
        }

    }
}
