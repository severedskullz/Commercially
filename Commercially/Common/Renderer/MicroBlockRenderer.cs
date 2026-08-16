using Commercially.Common.Interfaces;
using Commercially.Common.Util;
using System.Collections.Generic;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;
using Vintagestory.GameContent;

namespace Commercially.Common.Renderer
{
    public class MicroBlockRenderer : IItemRenderer
    {
        public EnumItemClass GetRendererClass() => EnumItemClass.Block;
        public int GetPriority() => 1;
        public bool ShouldCache(ItemStack stack) => false;

        public bool CanHandle(ItemStack stack)
        {
            return stack.Block is BlockMicroBlock;
        }

        public MeshData CreateMesh(IBlockEntityComponent stall, ItemSlot slot, int index)
        {
            ICoreClientAPI coreClientAPI = stall.GetApi() as ICoreClientAPI;

            ITreeAttribute treeAttribute = slot.Itemstack?.Attributes;
            if (treeAttribute == null)
            {
                treeAttribute = new TreeAttribute();
            }
            int[] materials = BlockEntityMicroBlock.MaterialIdsFromAttributes(treeAttribute, coreClientAPI.World);
            IntArrayAttribute cuboidAttribute = treeAttribute["cuboids"] as IntArrayAttribute;
            uint[] array = (cuboidAttribute != null) ? cuboidAttribute.AsUint : null;
            if (array == null)
            {
                LongArrayAttribute longArrayAttribute = treeAttribute["cuboids"] as LongArrayAttribute;
                array = ((longArrayAttribute != null) ? longArrayAttribute.AsUint : null);
            }
            List<uint> voxelCuboids = (array == null) ? new List<uint>() : new List<uint>(array);

            IntArrayAttribute decorAttribute = treeAttribute["decorIds"] as IntArrayAttribute;
            int[] decorIds = (int[])((decorAttribute != null) ? decorAttribute.value.Clone() : null);
            return BlockEntityMicroBlock.CreateMesh(coreClientAPI, voxelCuboids, materials, decorIds);
            
        }

        

    }
}
