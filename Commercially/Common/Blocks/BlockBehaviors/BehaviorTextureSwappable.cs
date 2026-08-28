using HarmonyLib;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.API.Util;

namespace Commercially.Common.Blocks.BlockBehaviors
{

    public class GenerateCreativeStacksConfig
    {
        public bool Enabled { get; set; } = true;
        public string BaseBlock { get; set; }
        public List<string> PrimaryMaterials { get; set; } = [];
        public List<string> SecondaryMaterials { get; set; } = [];
        public List<string> DecoMaterials { get; set; } = [];
        public string[] CreativeTabs { get; set; } = [];
    }

    public class BehaviorTextureSwappable : BlockBehavior, ITexPositionSource
    {
        private const string GUI_MESHES = "commercialGuiMeshRefs";

        private ITexPositionSource tmpTextureSource;
        private ICoreAPI Api;
        private GenerateCreativeStacksConfig _config;

        public string PrimaryMaterial { get; set; }
        public string SecondaryMaterial { get; set; }
        public string DecoMaterial { get; set; }
        public Size2i AtlasSize { get; private set; }

        public string SharedInstanceTest { get; set; } = "InitialValue";

        public BehaviorTextureSwappable(Block block) : base(block)
        {
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


        public override void OnLoaded(ICoreAPI api)
        {
            this.Api = api;
            base.OnLoaded(api);

            // Learned the hard way, Vintage Story sends CreativeTabAndStackList to clients when they connect, so calling this on both results in duplicated creative stacks.
            // Let the server generate them, then send them to the client.
            if (api.Side == EnumAppSide.Server) { 
                if (_config != null)
                {
                    AddAllTypesToCreativeInventory(api, _config);
                    _config = null;
                }
                else
                {
                    Api.ModLoader.GetModSystem<CommerciallyModSystem>().Mod.Logger.Error($"Failed to generate creative stacks for '{collObj?.Code}': missing config");
                }
            }

            bool found = false;
            foreach (var item in (collObj as Block).BlockEntityBehaviors)
            {
                if (item.Name == "Commercially.TextureSwappable")
                {
                    found = true;
                    break;
                }
            }

            if (!found)
            {
                Api.ModLoader.GetModSystem<CommerciallyModSystem>().Mod.Logger.Error($"Failed to find BEBehaviorTextureSwappable for '{collObj?.Code}' - Adding one automatically. Please add \"Commercially.BehaviorTextureSwappable\" manually in the block's definition JSON.");
                (collObj as Block).BlockEntityBehaviors.AddItem(new BlockEntityBehaviorType() { Name = "Commercially.BehaviorTextureSwappable" });
            }

        }

        public override void Initialize(JsonObject properties)
        {
            base.Initialize(properties);

            _config = properties.AsObject<GenerateCreativeStacksConfig>();
        }

        private void AddAllTypesToCreativeInventory(ICoreAPI api, GenerateCreativeStacksConfig config)
        {
            if (!config.Enabled || config.BaseBlock == null || collObj.Code.Path != config.BaseBlock)
            {
                return;
            }

            LinkedList<JsonItemStack> stacks = [];

            List<string> attributesCombinations = [];
            foreach (string primary in config.PrimaryMaterials)
            {
                JToken token = new JObject();
                token["Code"] = config.BaseBlock;
                token["PrimaryMaterial"] = primary;
                if (config.SecondaryMaterials?.Count > 0)
                {
                    foreach (string secondary in config.SecondaryMaterials)
                    {
                        JToken withSecondary = token.DeepClone();
                        withSecondary["SecondaryMaterial"] = secondary;
                        if (config.DecoMaterials?.Count > 0)
                        {

                            foreach (string decoration in config.DecoMaterials)
                            {
                                JToken withDecoration = withSecondary.DeepClone();
                                withDecoration["DecoMaterial"] = decoration;
                                stacks.AddLast(GenStackJson(api, withDecoration.ToString()));
                            }
                        }
                        else
                        {
                            stacks.AddLast(GenStackJson(api, withSecondary.ToString()));
                        }
                    }
                }
                else
                {
                    stacks.AddLast(GenStackJson(api, token.ToString()));
                }
            }


            if (collObj.CreativeInventoryStacks == null)
            {
                collObj.CreativeInventoryStacks = [new() { Stacks = [.. stacks], Tabs = config.CreativeTabs }];
                collObj.CreativeInventoryTabs = null;
            }
            else
            {
                collObj.CreativeInventoryStacks = collObj.CreativeInventoryStacks.Append(new CreativeTabAndStackList() { Stacks = stacks.ToArray(), Tabs = config.CreativeTabs });
            }
        }
        private JsonItemStack GenStackJson(ICoreAPI api, string json)
        {
            JsonItemStack stackJson = new()
            {
                Code = collObj.Code,
                Type = collObj.ItemClass,
                Attributes = new JsonObject(JToken.Parse(json))
            };

            stackJson.Resolve(api.World, "GenerateCreativeStacks");

            return stackJson;
        }
    }
}
