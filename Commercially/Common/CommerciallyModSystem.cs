using Cairo;
using Commercially.Common.BlockBehaviors;
using Commercially.Common.BlockEntities;
using Commercially.Common.BlockEntityBehaviors;
using Commercially.Common.BlockTypes;
using Commercially.Common.Database;
using Commercially.Common.Interactions;
using Commercially.Common.Interfaces;
using Commercially.Common.Registry;
using Commercially.Common.Registry.Packets;
using Commercially.Common.Renderer;
using System;
using System.Collections.Generic;
using Vinconomy.Delegates;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;
using Vintagestory.GameContent;

namespace Commercially.Common
{
    public class CommerciallyModSystem : ModSystem
    {
        private readonly string CONFIG_NAME = "commercially-core.json";
        private CommercialConfig Config;
        private ICoreServerAPI _CoreServerApi;
        private ICoreClientAPI _CoreClientApi;
        public IOwnableRegistry OwnableRegistry;
        private Dictionary<EnumItemClass, List<IItemRenderer>> Renderers = new Dictionary<EnumItemClass, List<IItemRenderer>>();
        private Dictionary<string, IInteraction> Interactions = new Dictionary<string, IInteraction>();
        private bool isRegisteringRenderers;
        public OwnableMapLayer OwnableMapLayer;

        public bool IsServer => _CoreServerApi != null;
        private IServerNetworkChannel _serverChannel;

        public bool IsClient => _CoreClientApi != null;
        private IClientNetworkChannel _clientChannel;

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
                DB = new CommercialDatabase((ICoreServerAPI)api);

            base.StartPre(api);
        }


        // Called on server and client
        // Useful for registering block/entity classes on both sides
        public override void Start(ICoreAPI api)
        {
            
            api.Network.RegisterChannel(CommerciallyConstants.COMM_CHANNEL)
                .RegisterMessageType(typeof(RegistryUpdatePacket))
                .RegisterMessageType(typeof(OwnableUpdatePacket))
                //.RegisterMessageType(typeof(ShopCatalogRequestPacket))
                //.RegisterMessageType(typeof(ShopCatalogResponsePacket))
                ;

            api.Event.OnTestBlockAccess += TestAccess;
            

            api.RegisterBlockClass("Commercially.BlockCommercial", typeof(BlockCommercialBase));

            api.RegisterBlockEntityClass("Commercially.BECommercialBase", typeof(BECommercialBase));

            api.RegisterBlockEntityBehaviorClass("Commercially.GuiManager", typeof(BEBehaviorGUIManager));
            api.RegisterBlockEntityBehaviorClass("Commercially.InteractionManager", typeof(InteractionManager));
            api.RegisterBlockEntityBehaviorClass("Commercially.Ownable", typeof(BEBehaviorOwnable));
            api.RegisterBlockEntityBehaviorClass("Commercially.OwnableReferenced", typeof(BEBehaviorOwnableReferenced));
            api.RegisterBlockEntityBehaviorClass("Commercially.OwnableChild", typeof(BEBehaviorOwnableChild));
            api.RegisterBlockEntityBehaviorClass("Commercially.OwnableNode", typeof(BEBehaviorOwnableNode));
            api.RegisterBlockEntityBehaviorClass("Commercially.OwnableRoot", typeof(BEBehaviorOwnableRoot));
            api.RegisterBlockEntityBehaviorClass("Commercially.TextureSwappable", typeof(BEBehaviorTextureSwappable));
            api.RegisterBlockEntityBehaviorClass("Commercially.GenericContainer", typeof(BEBehaviorGenericContainer));


            api.RegisterBlockBehaviorClass("Commercially.TextureSwappable", typeof(BehaviorTextureSwappable));
            api.RegisterBlockBehaviorClass("Commercially.CommercialEvents", typeof(BehaviorCommercialEvents));

            RegisterInteraction(OpenGuiInteraction.Key, new OpenGuiInteraction());
        }

        public EnumWorldAccessResponse TestAccess(IPlayer player, BlockSelection blockSelection, EnumBlockAccessFlags accessType, ref string claimant, EnumWorldAccessResponse response)
        {
            if (OnTestAccess != null)
            {
                EnumWorldAccessResponse multicastResult = response;
                Delegate[] delegates = OnTestAccess.GetInvocationList();
                foreach (Delegate delegator in delegates)
                {
                    try
                    {
                        multicastResult = ((OnTestAccessDelegate)delegator).Invoke(player, blockSelection, accessType, claimant, multicastResult);
                    }
                    catch (Exception e)
                    {
                        this.Mod.Logger.Error(e);
                    }
                }
                return multicastResult;
            }
            else return response;
        }
        public event OnTestAccessDelegate OnTestAccess;


        public override void StartServerSide(ICoreServerAPI api)
        {
            /*
            _serverChannel = api.Network.GetChannel(CommConstants.COMM_CHANNEL);
            _serverChannel.SetMessageHandler(new NetworkClientMessageHandler<ShopCatalogRequestPacket>(OnRecieveShopCatalogRequest));
            api.Event.SaveGameLoaded += OnSaveGameLoading;
            
            */
            _CoreServerApi = api;
            _serverChannel = api.Network.GetChannel(CommerciallyConstants.COMM_CHANNEL);


            OwnableRegistry = new OwnableServerRegistry(this, DB);
            DB.InitializeDB();

            api.Event.SaveGameLoaded += OnSaveGameLoading;
            //api.Event.PlayerJoin += SendAllPublicOwnables;
            api.Event.PlayerNowPlaying += SendAllPublicOwnables;
        }

        private void OnSaveGameLoading()
        {
            OwnableRegistry.Initialize();
        }

        private void SendAllPublicOwnables(IServerPlayer byPlayer)
        {
            List<OwnableRegistration> shops = OwnableRegistry.GetAllOwnables();
            List<OwnableUpdatePacket> updates = new List<OwnableUpdatePacket>();
            if (shops != null)
            {
                foreach (OwnableRegistration shop in shops)
                {
                    if (shop.BroadcastWaypoint || shop.CanAccess(byPlayer))
                    {
                        updates.Add(new OwnableUpdatePacket(shop, shop.OwnerUID == byPlayer.PlayerUID));
                    }
                }
            }
            _serverChannel.SendPacket(new RegistryUpdatePacket(updates), byPlayer);
        }

        public override void StartClientSide(ICoreClientAPI api)
        {
            _CoreClientApi = api;
            OwnableRegistry = new OwnableClientRegistry(this);

            _clientChannel = api.Network.GetChannel(CommerciallyConstants.COMM_CHANNEL);
            _clientChannel.SetMessageHandler(new NetworkServerMessageHandler<RegistryUpdatePacket>(this.OnRecieveRegistry));
            _clientChannel.SetMessageHandler(new NetworkServerMessageHandler<OwnableUpdatePacket>(this.OnRecieveRegistryUpdate));
            //_clientChannel.SetMessageHandler(new NetworkServerMessageHandler<ShopCatalogResponsePacket>(OnRecieveShopCatalogResponse));

            api.RegisterLinkProtocol("viewmap", OnMapLinkClicked);
            api.ModLoader.GetModSystem<WorldMapManager>().RegisterMapLayer<OwnableMapLayer>("CommerciallyOwnables", 20);

            Renderers = new Dictionary<EnumItemClass, List<IItemRenderer>>();
            foreach (EnumItemClass i in Enum.GetValues(typeof(EnumItemClass)))
            {
                Renderers[i] = new List<IItemRenderer>();
            }
            BeginRendererRegistration();
            RegisterRenderer(new BlockRenderer());
            RegisterRenderer(new ItemRenderer());
            RegisterRenderer(new ClutterBlockRenderer());
            RegisterRenderer(new CoinItemRenderer());
            RegisterRenderer(new MicroBlockRenderer());
            EndRendererRegistration();
        }

        private void OnMapLinkClicked(LinkTextComponent component)
        {

            string[] array = component.Href.Substring("viewmap://".Length).Split('=');
            int x = int.Parse(array[0]);
            int y = int.Parse(array[1]);
            int z = int.Parse(array[2]);
            WorldMapManager mapMan = _CoreClientApi.ModLoader.GetModSystem<WorldMapManager>();
            if (!mapMan.worldMapDlg.IsOpened() || mapMan.worldMapDlg.DialogType != EnumDialogType.Dialog)
            {

                mapMan.ToggleMap(EnumDialogType.Dialog);
                //mapMan.worldMapDlg.TryOpen();
            }
            (mapMan.worldMapDlg.SingleComposer.GetElement("mapElem") as GuiElementMap).CenterMapTo(new BlockPos(x, y, z, 1));
        }

        //Client Only
        private void OnRecieveRegistry(RegistryUpdatePacket packet)
        {
            OwnableRegistry = new OwnableClientRegistry(this);
            if (packet.registry != null)
            {
                foreach (OwnableUpdatePacket item in packet.registry)
                {
                    OwnableRegistry.AddOwnable(new OwnableRegistration(item));
                }
            }
            if (OwnableMapLayer != null)
                this.OwnableMapLayer.RebuildMapComponents();
        }

        //Client Only
        private void OnRecieveRegistryUpdate(OwnableUpdatePacket packet)
        {
            this.Mod.Logger.Debug("Got an update packet for shop " + packet.ID + ". It has coords " + packet.X + "/" + packet.Y + "/" + packet.Z + " and broadcasting is " + packet.BroadcastWaypoint);
            if (packet.IsRemoval)
            {
                OwnableRegistry.ClearOwnable(packet.ID);
            }
            else
            {
                ((OwnableClientRegistry)OwnableRegistry).UpdateOwnableFromServer(packet);
            }

            if (OwnableMapLayer != null)
                this.OwnableMapLayer.RebuildMapComponents();
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
            OwnableRegistration reg = OwnableRegistry.AddOwnable(entity);
            BroadcastOwnableUpdate(reg);

        }

        public void RemoveOwnable(IOwnableReference entity)
        {
            OwnableRegistration reg = OwnableRegistry.GetOwnable(entity.ID);
            OwnableRegistry.ClearOwnable(entity.ID);
            BroadcastOwnableUpdate(reg, true);

        }

        public void UpdateOwnable(IOwnableReference entity)
        {
            OwnableRegistration reg = OwnableRegistry.GetOwnable(entity.ID);
            reg.Name = entity.Name;

            reg.OwnerUID = entity.OwnerUID;
            reg.OwnerName = entity.OwnerName;

            if (entity is IOwnableChild)
            {
                reg.ParentId = ((IOwnableChild)entity).ParentID;
            }

            OwnableRegistry.UpdateOwnable(reg);
            BroadcastOwnableUpdate(reg);
        }

        public void UpdateOwnableWaypoint(IOwnableReference entity, bool broadcast,string icon = null, int? color = null)
        {
            OwnableRegistration reg = OwnableRegistry.GetOwnable(entity.ID);

            if (broadcast)
            {
                reg.BroadcastWaypoint = true;
                reg.WaypointIcon = icon ?? CommerciallyConstants.DEFAULT_WAYPOINT_ICON;
                reg.WaypointColor = color ?? CommerciallyConstants.DEFAULT_WAYPOINT_COLOR;
            }
            else
            {
                reg.BroadcastWaypoint = false;
                reg.WaypointIcon = null;
                reg.WaypointColor = 0;
            }

            OwnableRegistry.UpdateOwnable(reg);
            BroadcastOwnableUpdate(reg);
        }

        public void BroadcastOwnableUpdate(OwnableRegistration ownable, bool isRemoval = false)
        {
            if (_CoreServerApi == null) return; // Serverside only!

            if (ownable != null)
            {

                IServerPlayer owner = (IServerPlayer)_CoreServerApi.World.PlayerByUid(ownable.OwnerUID);

                if (owner.ConnectionState == EnumClientState.Playing)
                {
                    OwnableUpdatePacket update;
                    if (isRemoval)
                    {
                        update = new OwnableUpdatePacket(ownable.ID);
                    }
                    else
                    {
                        update = new OwnableUpdatePacket(ownable, true);
                    }

                    _serverChannel.SendPacket(update, owner);
                }

                //broadcast owner packet to all those with permission to the shop
                /*
                foreach (ShopAccess access in ownable.Permissions.Values)
                {
                    IServerPlayer granted = (IServerPlayer)_CoreServerApi.World.PlayerByUid(access.PlayerUID);
                    SendOwnerUpdate(ownable, granted);
                    excludeList.Add(granted); // Dont send them the waypoint update if its broadcasted, too...
                }
                */

                if (ownable.BroadcastWaypoint)
                {
                    OwnableUpdatePacket update;
                    if (isRemoval)
                    {
                        update = new OwnableUpdatePacket(ownable.ID);
                    }
                    else 
                    { 
                        update = new OwnableUpdatePacket(ownable, false); 
                    }

                    _serverChannel.BroadcastPacket(update, [owner]);
                }
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
            
            Renderers.TryGetValue(stack.Class, out typeRenderers);
            if (typeRenderers != null) {
                foreach (IItemRenderer renderer in typeRenderers)
                {
                    if (renderer.CanHandle(stack))
                        return renderer;
                }
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
            EnumItemClass rendererClass = blockRenderer.GetRendererClass();

            if (!Renderers.ContainsKey(rendererClass))
            {
                Renderers.Add(rendererClass, []);
            }

            Renderers[rendererClass].Add(blockRenderer);
            if (!isRegisteringRenderers)
            {
                // Only sort the list on Priority when we are finished registration instead of each time we add them
                // However, as a failsafe if we forget to call Begin/EndRendererRegistration, then this will still fire
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
            _CoreClientApi.Gui.Icons.CustomIcons["vicon-" + key] = delegate (Context ctx, int x, int y, float w, float h, double[] rgba)
            {
                AssetLocation loc = new AssetLocation("commercially:textures/icons/slot-" + key + ".svg");
                IAsset asset = _CoreClientApi.Assets.TryGet(loc, true);
                int color = ColorUtil.ColorFromRgba(175, 200, 175, 125);
                _CoreClientApi.Gui.DrawSvg(asset, ctx.GetTarget() as ImageSurface, x, y, (int)w, (int)h, color);
            };
        }

        public IOwnableReference GetOwnable(string ownerUID, long? ownableId)
        {
            if (ownerUID == null || ownableId == null)
            {
                return null;
            }

            OwnableRegistration ownableReg = DB.GetOwnable(ownerUID, ownableId);

            if (ownableReg == null || ownableReg.Position == null) { return null; }

            //Make sure they still own the shop
            if (ownableReg.OwnerUID != ownerUID)
            {
                return null;
            }

            BlockEntity entity = _CoreServerApi.World.BlockAccessor.GetBlockEntity(ownableReg.Position);
            return entity?.GetBehavior<IOwnableReference>();
        }

        public void RegisterInteraction(string key, IInteraction interaction)
        {
            if (Interactions.ContainsKey(key))
            {
                this.Mod.Logger.Warning("Interaction {0} is already registered. Overwriting with new interaction.", key);
            }
            Interactions[key] = interaction;
        }

        public IInteraction? GetInteraction(string key)
        {
            if (Interactions.TryGetValue(key, out var interaction))
            {
                return interaction;
            }
            this.Mod.Logger.Warning("Interaction {0} not found. Returning null.", key);
            return null;
        }
    }
}
