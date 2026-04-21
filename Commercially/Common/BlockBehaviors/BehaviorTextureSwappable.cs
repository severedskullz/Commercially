using System.Collections.Generic;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.API.Util;

namespace Commercially.Common.BlockBehaviors
{
    public class BehaviorTextureSwappable : BlockBehavior, ITexPositionSource
    {
        private const string GUI_MESHES = "commercialGuiMeshRefs";

        private ITexPositionSource tmpTextureSource;
        private ICoreAPI Api;

        public string PrimaryMaterial { get; set; }
        public string SecondaryMaterial { get; set; }
        public string DecoMaterial { get; set; }
        public Size2i AtlasSize { get; private set; }

        public string SharedInstanceTest { get; set; } = "InitialValue";

        public BehaviorTextureSwappable(Block block) : base(block)
        {
        }

        public override void OnLoaded(ICoreAPI api)
        {
            this.Api = api;
            base.OnLoaded(api);
        }

        public override void Initialize(JsonObject properties)
        {
            base.Initialize(properties);
            // TODO: Generate creative inventory stacks based on properites
            // Shamelessly reference https://github.com/maltiez2/vsmod_backpacks/blob/master/source/GenerateCreativeStacks.cs
        }

        public override void OnBeforeRender(ICoreClientAPI capi, ItemStack itemstack, EnumItemRenderTarget target, ref ItemRenderInfo renderinfo)
        {
            JsonObject obj = itemstack.Collectible.Attributes;

            Dictionary<string, MultiTextureMeshRef> meshrefs = ObjectCacheUtil.GetOrCreate(capi, GUI_MESHES, () => new Dictionary<string, MultiTextureMeshRef>());
            string primary = itemstack.Attributes.GetString("PrimaryMaterial", "default");
            string secondary = itemstack.Attributes.GetString("SecondaryMaterial", "default");
            string deco = itemstack.Attributes.GetString("DecoMaterial", "default");
            string key = $"{itemstack.Collectible.Code.Path}-{primary}-{secondary}-{deco}";
            MultiTextureMeshRef meshref;
            if (!meshrefs.TryGetValue(key, out meshref))
            {
                AssetLocation shapeloc = this.block.Shape.Base.CopyWithPathPrefixAndAppendixOnce("shapes/", ".json");
                Shape shape = Vintagestory.API.Common.Shape.TryGet(capi, shapeloc);
                MeshData mesh = GenMesh(capi, primary, secondary, deco, shape, null);

                if (mesh != null)
                {
                    meshrefs[key] = capi.Render.UploadMultiTextureMesh(mesh);
                    meshref = meshrefs[key];
                }

            }
            renderinfo.ModelRef = meshref;
            renderinfo.CullFaces = false;
        }

        public MeshData GenMesh(ICoreClientAPI capi, string primaryMaterial, string secondaryMaterial, string decoMaterial, Shape shape = null, ITesselatorAPI tesselator = null)
        {
            if (tesselator == null)
            {
                tesselator = capi.Tesselator;
            }
            this.tmpTextureSource = tesselator.GetTextureSource(this.block, 0, false);
            if (shape == null)
            {
                AssetLocation shapeloc = this.block.Shape.Base.CopyWithPathPrefixAndAppendixOnce("shapes/", ".json");
                shape = Vintagestory.API.Common.Shape.TryGet(capi, shapeloc);
            }
            if (shape == null)
            {
                return null;
            }
            this.AtlasSize = capi.BlockTextureAtlas.Size;
            UpdateMaterials(primaryMaterial, secondaryMaterial, decoMaterial);
            MeshData mesh;
            tesselator.TesselateShape("textureSwappable", shape, out mesh, this, new Vec3f(this.block.Shape.rotateX, this.block.Shape.rotateY, this.block.Shape.rotateZ), 0, 0, 0, null, null);
            return mesh;
        }

        public void UpdateMaterials(string primary, string secondary, string deco)
        {
            this.PrimaryMaterial = primary;
            this.SecondaryMaterial = secondary;
            this.DecoMaterial = deco;
        }

        public virtual TextureAtlasPosition this[string textureCode]
        {
            get
            {
                if (this.tmpTextureSource == null)
                {
                    return null;
                }

                if (textureCode == "primary")
                {
                    return tmpTextureSource[PrimaryMaterial];
                }
                if (textureCode == "secondary")
                {
                    return tmpTextureSource[SecondaryMaterial];
                }
                if (textureCode == "deco")
                {
                    return tmpTextureSource[DecoMaterial];
                }

                if (tmpTextureSource[textureCode] != null)
                {
                    return tmpTextureSource[textureCode];
                }
                else
                {
                    return tmpTextureSource["default"];
                }
            }
        }
    }
}
