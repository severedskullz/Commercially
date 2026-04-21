using Cairo;
using Commercially.Common.BlockBehaviors;
using Commercially.Common.BlockEntities;
using Commercially.Common.BlockEntityBehaviors;
using Commercially.Common.BlockTypes;
using Commercially.Common.Renderer;
using System;
using System.Collections.Generic;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;

namespace Commercially.Common
{
    public class CommerciallyModSystem : ModSystem
    {
        private readonly string CONFIG_NAME = "commercially-core.json";
        private CommercialConfig Config;
        private ICoreServerAPI _coreServerApi;
        private ICoreClientAPI _coreClientApi;
        private Dictionary<EnumItemClass, List<IItemRenderer>> Renderers;
        private bool isRegisteringRenderers;

        public bool IsServer => _coreServerApi != null;
        public bool IsClient => _coreClientApi != null;

        public CommercialDatabase DB { get; private set; }

        public override double ExecuteOrder() => 1.01;

        public CommercialConfig ResetModConfig()
        {
            CommercialConfig config = new CommercialConfig();

            return config;
        }

        public override void StartPre(ICoreAPI api)
        {

            try
            {
                CommercialConfig config = api.LoadModConfig<CommercialConfig>(CONFIG_NAME);
                if (config == null)
                {
                    config = ResetModConfig();
                    api.StoreModConfig(config, CONFIG_NAME);
                }

                Config = config;
            }
            catch
            {
                Config = ResetModConfig();
                Mod.Logger.Error("Could not load Mod Config for Commercially. Loading defaults instead. Check your config and ensure there are no errors.");
                //api.StoreModConfig<ViconConfig>(new ViconConfig(), filename);
            }

            if (api.Side == EnumAppSide.Server)
                DB = new CommercialDatabase((ICoreServerAPI) api);

            base.StartPre(api);
        }


        // Called on server and client
        // Useful for registering block/entity classes on both sides
        public override void Start(ICoreAPI api)
        {
            /*
            api.Network.RegisterChannel(CommConstants.COMM_CHANNEL)
                .RegisterMessageType(typeof(RegistryUpdatePacket))
                .RegisterMessageType(typeof(ShopUpdatePacket))
                .RegisterMessageType(typeof(ShopCatalogRequestPacket))
                .RegisterMessageType(typeof(ShopCatalogResponsePacket));

            api.Event.OnTestBlockAccess += TestAccess;
            */

            api.RegisterBlockClass("Commercially.BlockCommercial", typeof(BlockCommercialBase));

            api.RegisterBlockEntityClass("Commercially.BECommercialBase", typeof(BECommercialBase));

            api.RegisterBlockEntityBehaviorClass("Commercially.InteractionManager", typeof(InteractionManager));
            api.RegisterBlockEntityBehaviorClass("Commercially.Ownable", typeof(BEBehaviorOwnable));
            api.RegisterBlockEntityBehaviorClass("Commercially.OwnableReferenced", typeof(BEBehaviorOwnableReferenced));
            api.RegisterBlockEntityBehaviorClass("Commercially.OwnableLeaf", typeof(BEBehaviorOwnableLeaf));
            api.RegisterBlockEntityBehaviorClass("Commercially.OwnableNode", typeof(BEBehaviorOwnableNode));
            api.RegisterBlockEntityBehaviorClass("Commercially.OwnableRoot", typeof(BEBehaviorOwnableRoot));
            api.RegisterBlockEntityBehaviorClass("Commercially.TextureSwappable", typeof(BEBehaviorTextureSwappable));
            api.RegisterBlockEntityBehaviorClass("Commercially.GenericContainer", typeof(BEBehaviorGenericContainer));
            

            api.RegisterBlockBehaviorClass("Commercially.TextureSwappable", typeof(BehaviorTextureSwappable));
            api.RegisterBlockBehaviorClass("Commercially.CommercialEvents", typeof(BehaviorCommercialEvents));
        }



        public override void StartServerSide(ICoreServerAPI api)
        {
            /*
            _serverChannel = api.Network.GetChannel(CommConstants.COMM_CHANNEL);
            _serverChannel.SetMessageHandler(new NetworkClientMessageHandler<ShopCatalogRequestPacket>(OnRecieveShopCatalogRequest));
            api.Event.SaveGameLoaded += OnSaveGameLoading;
            api.Event.PlayerNowPlaying += SendAllPublicShops;
            */
            _coreServerApi = api;
            DB.InitializeDB();


        }


        public override void StartClientSide(ICoreClientAPI api)
        {
            _coreClientApi = api;

            /*
            _clientChannel = api.Network.GetChannel(CommConstants.COMM_CHANNEL);
            _clientChannel.SetMessageHandler(new NetworkServerMessageHandler<RegistryUpdatePacket>(OnRecieveRegistry));
            _clientChannel.SetMessageHandler(new NetworkServerMessageHandler<ShopUpdatePacket>(OnRecieveRegistryUpdate));
            _clientChannel.SetMessageHandler(new NetworkServerMessageHandler<ShopCatalogResponsePacket>(OnRecieveShopCatalogResponse));

            api.RegisterLinkProtocol("viewmap", OnMapLinkClicked);
            api.ModLoader.GetModSystem<WorldMapManager>().RegisterMapLayer<ShopMapLayer>("vinconomyShop", 20);
            */

        }

        public bool OnBlockBroken(AssetLocation code, IWorldAccessor world, BlockPos pos, IPlayer byPlayer, float dropQuantityMultiplier)
        {
            return true;
        }

        public void OnBlockPlaced(AssetLocation code, IWorldAccessor world, BlockPos blockPos, ItemStack byItemStack)
        {

        }

        public bool TryPlaceBlock(IWorldAccessor world, IPlayer byPlayer, ItemStack itemstack, BlockSelection blockSel)
        {
            return true;
        }


        public void AddOwnable(IOwnableReference entity)
        {
            if (IsServer)
            {
                DB.AddOwnable(entity);
            }
        }

        public void RemoveOwnable(IOwnableReference entity)
        {
            if (IsServer)
            {
                DB.ClearPositionForOwnableById(entity.ID);
            }
        }

        public void UpdateOwnable(IOwnableReference entity)
        {
            if (IsServer)
            {
                DB.UpdateOwnableEntry(entity);
            }
        }

        public static void PrintClientMessage(IPlayer player, string message, object[] args = null)
        {
            if (message == null)
                return;

            if (player == null)
                return;

            if (args == null)
            {
                args = [];
            }
            if (player is IServerPlayer)
            {
                (player as IServerPlayer).SendMessage(0, Lang.GetL((player as IServerPlayer)?.LanguageCode ?? "en", message, args), EnumChatType.OwnMessage, null);
            }
            else
            {
                ((IClientPlayer)player).ShowChatNotification(Lang.Get(message, args));
            }
        }

        public static void LoadChunk(ICoreServerAPI api, int x, int y, int z, Action onLoaded)
        {
            int cx = x / GlobalConstants.ChunkSize;
            int cy = y / GlobalConstants.ChunkSize;
            int cz = z / GlobalConstants.ChunkSize;
            IServerChunk chunk = api.WorldManager.GetChunk(cx, cy, cz);

            //BlockPos pos = new BlockPos(x, y, z);
            //ChunkPos.ToChunkIndex(x, y, z);
            //IServerChunk chunk = api.WorldManager.GetChunk(pos);

            //https://discord.com/channels/302152934249070593/351624415039193098/1185988966479052850
            //var chunkCoord = blockPos / 32;
            //var chunkIndex = MapUtil.Index3dL(chunkCoord, chunkMapSizeX, chunkMapSizeZ);
            // ChunkPosFromChunkIndex3D 

            if (chunk != null)
            {
                onLoaded.Invoke();
            }
            else
            {
                ChunkLoadOptions options = new ChunkLoadOptions();
                options.OnLoaded += onLoaded;

                api.WorldManager.LoadChunkColumnPriority(cx, cz, options);
            }


        }

        public IItemRenderer GetRenderer(ItemSlot slot)
        {
            return GetRenderer(slot.Itemstack);
        }

        public IItemRenderer GetRenderer(ItemStack stack)
        {
            List<IItemRenderer> typeRenderers = Renderers[stack.Class];
            foreach (IItemRenderer renderer in typeRenderers)
            {
                if (renderer.CanHandle(stack))
                    return renderer;
            }

            //This should have returned already, but in case someone fucked up
            this.Mod.Logger.Error("Did not get a renderer for item " + stack.Collectible.Code.Path);
            if (stack.Class == EnumItemClass.Block)
                return new BlockRenderer();
            else
                return new ItemRenderer();
        }

        public void BeginRendererRegistration()
        {
            isRegisteringRenderers = true;
        }

        public void RegisterRenderer(IItemRenderer blockRenderer)
        {
            Renderers[blockRenderer.GetRendererClass()].Add(blockRenderer);
            if (!isRegisteringRenderers)
            {
                Renderers[blockRenderer.GetRendererClass()].Sort((i1, i2) => { return i2.GetPriority().CompareTo(i1.GetPriority()); });
            }
        }

        public void EndRendererRegistration()
        {
            isRegisteringRenderers = false;
            foreach (EnumItemClass i in Enum.GetValues(typeof(EnumItemClass)))
            {
                Renderers[i].Sort((i1, i2) => { return i2.GetPriority().CompareTo(i1.GetPriority()); });
            }

        }

        public void RegisterCustomIcon(string key)
        {
            _coreClientApi.Gui.Icons.CustomIcons["vicon-" + key] = delegate (Context ctx, int x, int y, float w, float h, double[] rgba)
            {
                AssetLocation loc = new AssetLocation("commercially:textures/icons/slot-" + key + ".svg");
                IAsset asset = _coreClientApi.Assets.TryGet(loc, true);
                int color = ColorUtil.ColorFromRgba(175, 200, 175, 125);
                _coreClientApi.Gui.DrawSvg(asset, ctx.GetTarget() as ImageSurface, x, y, (int)w, (int)h, color);
            };
        }
    }
}
