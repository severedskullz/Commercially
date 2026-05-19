
using Commercially.Common.Interfaces;
using Commercially.Common.Util;
using System;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Util;

namespace Commercially.Common.Renderer
{
    public class CoinItemRenderer : IItemRenderer
    {
        public EnumItemClass GetRendererClass() => EnumItemClass.Item;
        public int GetPriority() => 10;
        public bool ShouldCache(ItemStack stack) => false;

        public bool CanHandle(ItemStack stack)
        {
            return stack.Class == EnumItemClass.Item && stack.Item.Code.Domain.Equals("coinage") && stack.Item.Code.PathStartsWith("coin-");
        }

        public MeshData CreateMesh(IShapeTesselator stall, ItemSlot slot, int index)
        {
            MeshData modeldata = null;
            ItemStack stack = slot.Itemstack;
            try
            {
                ICoreClientAPI coreClientAPI = stall.GetApi() as ICoreClientAPI;
                
                // For some reason this is now blowing up on 1.21 on the item renderer, so we will duplicate it and just skip this part and return the base model.
                /*
                IContainedMeshSource containedMeshSource = stack.Collectible as IContainedMeshSource;
                if (containedMeshSource != null)
                {
                    modeldata = containedMeshSource.GenMesh(stack, coreClientAPI.BlockTextureAtlas, stall.Pos);
                    if (modeldata != null)
                    {
                        return modeldata;
                    }

                }
                */


                stall.SetNowTesselatingObj(stack.Collectible);
                stall.SetNowTesselatingShape(null);

                if (stack?.Item.Shape?.Base != null)
                {
                    stall.SetNowTesselatingShape(coreClientAPI.TesselatorManager.GetCachedShape(stack.Item.Shape.Base));
                }

                coreClientAPI.Tesselator.TesselateItem(stack.Item, out modeldata, stall);
                modeldata.RenderPassesAndExtraBits.Fill((short)2);
                modeldata.Scale(new Vintagestory.API.MathTools.Vec3f(0.5f, 0, 0.5f), .4f, .4f, .4f);
            } catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            return modeldata;
        }


    }
}
