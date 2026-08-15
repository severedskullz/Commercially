using Commercially.Common.Interfaces;
using Commercially.Common.Util;
using System;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Util;
using Vintagestory.GameContent;

namespace Commercially.Common.Renderer
{
    public class ItemRenderer : IItemRenderer
    {
        public EnumItemClass GetRendererClass() => EnumItemClass.Item;
        public int GetPriority() => 0;
        public bool ShouldCache(ItemStack stack) => true;

        public bool CanHandle(ItemStack stack)
        {
            return stack.Class == EnumItemClass.Item;
        }

        public MeshData CreateMesh(IComponent stall, ItemSlot slot, int index)
        {
            ItemStack stack = slot.Itemstack;
            MeshData modeldata = null;
            try
            {
                ICoreClientAPI coreClientAPI = stall.GetApi() as ICoreClientAPI;
                IContainedMeshSource containedMeshSource = stack.Collectible as IContainedMeshSource;
                if (containedMeshSource != null)
                {
                    modeldata = containedMeshSource.GenMesh(slot, coreClientAPI.BlockTextureAtlas, stall.GetBlockEntity().Pos);
                    if (modeldata != null)
                    {
                        return modeldata;
                    }

                }

               // stall.SetNowTesselatingObj(stack.Collectible);

                if (stack.Item.Shape?.Base != null)
                {
                    //stall.SetNowTesselatingShape(coreClientAPI.TesselatorManager.GetCachedShape(stack.Item.Shape.Base));
                }

                //coreClientAPI.Tesselator.TesselateItem(stack.Item, out modeldata, stall);
                modeldata.RenderPassesAndExtraBits.Fill((short)2);
            } catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            return modeldata;
        }


    }
}
