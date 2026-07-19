using Commercially.Common.Blocks.BlockBehaviors;
using Commercially.Common.Interfaces;
using System.Collections.Generic;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;
using Vintagestory.API.Util;

namespace Commercially.Common.Blocks.BlockEntityBehaviors
{
    public class BEBehaviorTextureSwappable : BlockEntityBehavior, IPersistableStackAttributes
    {
        private BehaviorTextureSwappable behavior;

        public string PrimaryMaterial { get; set; }
        public string SecondaryMaterial { get; set; }
        public string DecoMaterial { get; set; }

        public BEBehaviorTextureSwappable(BlockEntity blockentity) : base(blockentity)
        {
            behavior = this.Blockentity.Block.GetBehavior<BehaviorTextureSwappable>();
            if (behavior == null)
            {
                throw new System.Exception("BEBehaviorTextureSwappable requires the block to have a BehaviorTextureSwappable");
            }
        }

        public override void OnBlockPlaced(ItemStack byItemStack = null)
        {
            PrimaryMaterial = byItemStack.Attributes.GetString("PrimaryMaterial", "default");
            SecondaryMaterial = byItemStack.Attributes.GetString("SecondaryMaterial", "default");
            DecoMaterial = byItemStack.Attributes.GetString("DecoMaterial", "default");
        }

        public override bool OnTesselation(ITerrainMeshPool mesher, ITesselatorAPI tesselator)
        {
            if (Api != null)
            {
                MeshData mesh = GetMesh(tesselator);
                if (mesh == null)
                {
                    return false;
                }
                mesher.AddMeshData(mesh, 1);
            }
            return true;
        }

        protected MeshData GetMesh(ITesselatorAPI tesselator)
        {
            Dictionary<string, MeshData> swappableMeshes = ObjectCacheUtil.GetOrCreate(Api, "textureSwappableMeshes", () => new Dictionary<string, MeshData>());
            MeshData mesh = null;
            Block block = this.Blockentity.Block;
            string key = $"{block.Code.Path}-{PrimaryMaterial}-{SecondaryMaterial}-{DecoMaterial}";
            if (swappableMeshes.TryGetValue(key, out mesh))
            {
                return mesh;
            }

            AssetLocation shapeloc = this.Blockentity.Block.Shape.Base.CopyWithPathPrefixAndAppendixOnce("shapes/", ".json");
            Shape shape = Vintagestory.API.Common.Shape.TryGet(Api, shapeloc);

            mesh = this.Blockentity.Block.GetBehavior<BehaviorTextureSwappable>().GenMesh(Api as ICoreClientAPI, PrimaryMaterial, SecondaryMaterial, DecoMaterial, shape, tesselator);
            swappableMeshes[key] = mesh;
            return mesh;
        }

        public override void FromTreeAttributes(ITreeAttribute tree, IWorldAccessor world)
        {
            PrimaryMaterial = tree.GetString("PrimaryMaterial", "default");
            SecondaryMaterial = tree.GetString("SecondaryMaterial", "default");
            DecoMaterial = tree.GetString("DecoMaterial", "default");
        }

        public override void ToTreeAttributes(ITreeAttribute tree)
        {
            tree.SetString("PrimaryMaterial",   PrimaryMaterial ?? "default");
            tree.SetString("SecondaryMaterial", SecondaryMaterial ?? "default");
            tree.SetString("DecoMaterial",      DecoMaterial ?? "default");
        }

        public void UpdateMaterials(string primary, string secondary, string deco)
        {
            this.PrimaryMaterial = primary;
            this.SecondaryMaterial = secondary;
            this.DecoMaterial = deco;
        }

        public void AddAttributes(ItemStack stack)
        {
            ToTreeAttributes(stack.Attributes);
        }
    }
}
