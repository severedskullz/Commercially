using Commercially.Common;
using Commercially.Common.GUI.Tabs;
using Commercially.Common.Interfaces;
using Commercially.Common.Registry.Packets;
using Commercially.Common.Slots;
using Commercially.Common.Util;
using Commercially.Vinconomy.BlockEntityBehaviors;
using Commercially.Vinconomy.BlockEntityBehaviors.InventoryProviders;
using Commercially.Vinconomy.GUI.Tabs;
using Commercially.Vinconomy.Interfaces;
using Commercially.Vinconomy.Inventory.StallSlots;
using Commercially.Vinconomy.Trading;
using System;
using System.Collections.Generic;
using Vinconomy.Delegates;
using Vinconomy.ItemTypes;
using Vinconomy.Util;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;

namespace Commercially.Vinconomy
{
    public class VinconomyModSystem : ModSystem
    {
        private ICoreServerAPI _CoreServerAPI;
        CommerciallyModSystem CommerciallySystem;

        private static Dictionary<string, Type> StallTypes;

        private readonly string CONFIG_NAME = "vinconomy-core.json";
        private VinconomyConfig Config;
        public VinconomyDatabase DB { get; private set; }

        public override double ExecuteOrder() => 1.1;

        public VinconomyConfig ResetModConfig()
        {
            VinconomyConfig config = new VinconomyConfig();

            return config;
        }

        public override void StartPre(ICoreAPI api)
        {
            StallTypes = new Dictionary<string, Type>();


            try
            {
                VinconomyConfig config = api.LoadModConfig<VinconomyConfig>(CONFIG_NAME);
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
                Mod.Logger.Error("Could not load Mod Config for Vinconomy. Loading defaults instead. Check your config and ensure there are no errors.");
                //api.StoreModConfig<ViconConfig>(new ViconConfig(), filename);
            }

            if (api.Side == EnumAppSide.Server)
                DB = new VinconomyDatabase((ICoreServerAPI)api);



            base.StartPre(api);
        }




        // Called on server and client
        // Useful for registering block/entity classes on both sides
        public override void Start(ICoreAPI api)
        {
            api.RegisterItemClass("VinconLedger", typeof(ItemLedger));
            api.RegisterItemClass("VinconCatalog", typeof(ItemCatalog));
            api.RegisterItemClass("VinconSculptureBundle", typeof(ItemSculptureBundle));
            api.RegisterItemClass("VinconGachaBall", typeof(ItemGachaBall));
            api.RegisterItemClass("VinconTenretni", typeof(ItemTenretniBook));
            api.RegisterItemClass("VinconCoupon", typeof(ItemCoupon));

            api.Network.RegisterChannel(VinConstants.VINCONOMY_CHANNEL);
                //.RegisterMessageType(typeof(RegistryUpdatePacket))
                //.RegisterMessageType(typeof(ShopUpdatePacket))
                //.RegisterMessageType(typeof(ShopCatalogRequestPacket))
                //.RegisterMessageType(typeof(ShopCatalogResponsePacket));

           // api.Event.OnTestBlockAccess += TestAccess;
            

            //api.RegisterBlockClass("Commercially.BlockCommercial", typeof(BlockCommercialBase));

            //api.RegisterBlockEntityClass("Commercially.BECommercialBase", typeof(BECommercialBase));

            api.RegisterBlockEntityBehaviorClass("Vinconomy.Stall", typeof(BEStallBehavior));
            api.RegisterBlockEntityBehaviorClass("Vinconomy.Register", typeof(BEShopBehavior));
            api.RegisterBlockEntityBehaviorClass("Vinconomy.RegisterInventory", typeof(RegisterInventoryProvider));
            api.RegisterBlockEntityBehaviorClass("Vinconomy.StallInventory", typeof(GenericStallInventoryProvider));
            api.RegisterBlockEntityBehaviorClass("Vinconomy.StallDisplay", typeof(BEDisplayContentsBehavior));
            api.RegisterBlockEntityBehaviorClass("Vinconomy.CouponCutter", typeof(BECouponCutterBehavior));

            api.RegisterItemClass("VinconLedger", typeof(ItemLedger));
            api.RegisterItemClass("VinconCatalog", typeof(ItemCatalog));
            api.RegisterItemClass("VinconSculptureBundle", typeof(ItemSculptureBundle));
            api.RegisterItemClass("VinconGachaBall", typeof(ItemGachaBall));
            api.RegisterItemClass("VinconTenretni", typeof(ItemTenretniBook));


            //api.RegisterBlockBehaviorClass("Commercially.TextureSwappable", typeof(BehaviorTextureSwappable));

            RegisterStallType("GenericStallSlot", typeof(GenericStallSlot));

            ModularGUIModSystem guiSystem = api.ModLoader.GetModSystem<ModularGUIModSystem>();
            guiSystem.RegisterTabType(GuiBlockEntityShopCustomerTab.CODE, typeof(GuiBlockEntityShopCustomerTab));
            guiSystem.RegisterTabType(GuiBlockEntityShopOwnerTab.CODE, typeof(GuiBlockEntityShopOwnerTab));
            guiSystem.RegisterTabType(GuiBlockEntityDisplayDebugTab.CODE, typeof(GuiBlockEntityDisplayDebugTab));
            guiSystem.RegisterTabType(GuiBlockEntityRegisterConfigTab.CODE, typeof(GuiBlockEntityRegisterConfigTab));
            guiSystem.RegisterTabType(GuiVinconCouponCutter.CODE, typeof(GuiVinconCouponCutter));




            CommerciallySystem = api.ModLoader.GetModSystem<CommerciallyModSystem>();
        }



        public override void StartServerSide(ICoreServerAPI api)
        {
            _CoreServerAPI = api;
            /*
            _serverChannel = api.Network.GetChannel(CommConstants.COMM_CHANNEL);
            _serverChannel.SetMessageHandler(new NetworkClientMessageHandler<ShopCatalogRequestPacket>(OnRecieveShopCatalogRequest));
            api.Event.SaveGameLoaded += OnSaveGameLoading;
            api.Event.PlayerNowPlaying += SendAllPublicShops;
            */

            DB.InitializeDB();


        }


        public override void StartClientSide(ICoreClientAPI api)
        {
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

        public static void RegisterStallType(string className, Type type) {
            StallTypes.Add(className, type);
        }

        public static Type GetStallType(string className)
        {
            return StallTypes[className];
        }

        public IShopComponent GetShop(string ownerUID, long? parentID)
        {
            return CommerciallySystem.GetOwnable(ownerUID, parentID)?.GetComponent<IShopComponent>(); ;
        }

        public bool CanPurchaseItem(IPlayer player, IStallComponent bEShopBehavior, IShopComponent register, int stallSlot, int numPurchases)
        {
            return true;
        }
        

        /// <summary>
        /// Runs before ProcessTrade to manipulate any parameters of the trade request. As an example, modify the price per product to include taxes in the calculation
        /// </summary>
        /// <param name="request"></param>
        /// <returns> should proceesing continue </returns>
        private bool PreProcessTrade(TradeRequest request) {
            EnumHandling handled = EnumHandling.PassThrough;
            foreach (var handlers in PreProcessTradeHandlers)
            {
                handlers.Value.Invoke(request, ref handled);

                if (handled == EnumHandling.PreventSubsequent) return true;
                if (handled == EnumHandling.PreventDefault) return false;
            }
            return true; // Handled / PassThrough
        }

        /// <summary>
        /// Processes the given Trade Request, converting it into a Trade Result after verification of general criteria such as the player having enough money for the trade, the shop having enough stock, etc.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="runProcessing"></param>
        /// <returns></returns>
        // TODO: this RunProcessing logic switch might not be very useful... I want to think of a way for PreProcessTrade to potentially do the logic instead, but that doesnt return a TradeResult.
        // On the flip-side, I dont want PreProcessTrade to be required to create a new TradeResult as it should be BEFORE the processing occurs. The seperation of concerns here overlap, which is bad.
        private static TradeResult ProcessTrade(TradeRequest request, bool runProcessing = true) {
            TradeResult result = new TradeResult(request);

            if (!runProcessing)
            {
                return result;
            }

            //There must be a ICurrencySinkProvider somewhere... Either we set it to the parent entity, or itself.
            if (!request.IsAdminShop && request.GetCurrencySink() == null)
                return SetErrorAndReturn(result, TradingConstants.NOT_REGISTERED);

            if (request.CurrencyNeeded == null)
                return SetErrorAndReturn(result, TradingConstants.NO_PRICE);

            if (request.ProductNeeded == null)
                return SetErrorAndReturn(result, TradingConstants.NO_PRODUCT);

            if (!TradingProcessor.HasEnoughStock(request))
                return SetErrorAndReturn(result, TradingConstants.NOT_ENOUGH_STOCK);

            if (!TradingProcessor.CanPlayerAfford(request))
                return SetErrorAndReturn(result, TradingConstants.NOT_ENOUGH_MONEY);

            if (request.NumPurchases <= 0)
                return SetErrorAndReturn(result, TradingConstants.PURCHASED_ZERO);

            if (!TradingProcessor.HasEnoughDurability(request))
                return SetErrorAndReturn(result, TradingConstants.NO_TOOL);

            if (!TradingProcessor.HasRequiredTradePass(request))
                return SetErrorAndReturn(result, TradingConstants.NO_PASS);

            if (!TradingProcessor.CanFitPaymentIntoParent(request))
                return SetErrorAndReturn(result, TradingConstants.NO_REGISTER_SPACE);

            return result;
        }
        private void PostProcessTrade(TradeResult result)
        {
            EnumHandling handled = EnumHandling.PassThrough;
            foreach (var handlers in PostProcessTradeHandlers)
            {
                handlers.Value.Invoke(result, ref handled);

                if (handled == EnumHandling.PreventSubsequent) return;
                if (handled == EnumHandling.PreventDefault) return;
            }
            return; // Handled / PassThrough
        }

        private void PreFinalizeTrade(TradeResult result) {
            EnumHandling handled = EnumHandling.PassThrough;
            foreach (var handlers in PreFinalizeTradeHandlers)
            {
                handlers.Value.Invoke(result, ref handled);

                if (handled == EnumHandling.PreventSubsequent) return;
                if (handled == EnumHandling.PreventDefault) return;
            }
            return; // Handled / PassThrough
        }

        private void FinalizeTrade(TradeResult result) {
            TransferCurrencyToParent(result);
            TransferCouponsToParent(result);
            TransferProductToPlayer(result);
            result.Request.SellingEntity?.GetBlockEntity().MarkDirty();
        }

        private void TransferProductToPlayer(TradeResult result)
        {
            if (result.ProductStacks.TotalCount == 0) return;

            IPlayer player = result.Request.Customer;
            AssetLocation sound = null;
            while(result.ProductStacks.CanRemoveStack())
            {
                ItemStack stack = result.ProductStacks.RemoveStack();

                if (stack != null)
                {
                    this.Mod.Logger.Debug($"Adding {stack.StackSize}x {stack} product to Parent");
                    if (stack.Block?.Sounds?.Place.Location != null)
                    {
                        sound = stack.Block?.Sounds?.Place.Location;
                    }

                    player.InventoryManager.TryGiveItemstack(stack, true);
                    if (stack.StackSize > 0)
                    {
                        result.Request.Api.World.SpawnItemEntity(stack, player.Entity.Pos.XYZ.Add(0.5), null);
                    }
                }
            }

            result.Request.Api.World.PlaySoundAt(sound ?? new AssetLocation("sounds/player/build"), result.Request.Customer.Entity, result.Request.Customer, true, 16f, 1f);
        }

        private void TransferCurrencyToParent(TradeResult result)
        {
            if (result.CurrencyStacks.TotalCount == 0) return;

            ICurrencySinkProvider provider = result.Request.GetCurrencySink();
            if (provider != null)
            {
                ItemSlot[] slots = provider.CurrencySlots;
                while (result.CurrencyStacks.CanRemoveStack())
                {
                    ItemStack nextStack = result.CurrencyStacks.RemoveStack();
                    this.Mod.Logger.Debug($"Adding {nextStack.StackSize}x {nextStack} currency to Parent");
                    AddItemToSlots(result.Request.Api, nextStack, slots);
                }
                provider.GetBlockEntity().MarkDirty();
            }
        }

        private void TransferCouponsToParent(TradeResult result)
        {
            if (result.CouponStacks.TotalCount == 0) return;

            ICurrencySinkProvider provider = result.Request.GetCurrencySink();
            if (provider != null)
            {
                ItemSlot[] slots = provider.CouponSlots;
                while (result.CouponStacks.CanRemoveStack())
                {
                    ItemStack nextStack = result.CouponStacks.RemoveStack();
                    this.Mod.Logger.Debug($"Adding {nextStack.StackSize}x {nextStack} coupon to Parent");
                    AddItemToSlots(result.Request.Api, nextStack, slots);

                }
                provider.GetBlockEntity().MarkDirty();
            }
        }

        private static bool AddItemToSlots(ICoreAPI api, ItemStack stack, ItemSlot[] slots)
        {
            if (stack == null || stack.StackSize == 0) return false;

            ItemSlot dslot = new ItemSlot(null);
            dslot.Itemstack = stack;

            int amountLeft = stack.StackSize;

            foreach (ItemSlot slot in slots)
            {
                if (slot.CanHold(dslot))
                {
                    amountLeft -= dslot.TryPutInto(api.World, slot, amountLeft);
                    slot.MarkDirty();
                }

                if (amountLeft <= 0)
                {
                    return true;
                }
            }
            return false;
        }

        private void PostFinalizeTrade(TradeResult result) {
            EnumHandling handled = EnumHandling.PassThrough;
            foreach (var handlers in PostFinalizeTradeHandlers)
            {
                handlers.Value.Invoke(result, ref handled);

                if (handled == EnumHandling.PreventSubsequent) return;
                if (handled == EnumHandling.PreventDefault) return;
            }
            return; // Handled / PassThrough
        }

        public static TradeResult SetErrorAndReturn(TradeResult result, string error)
        {
            result.ErrorMsg = error;
            result.Request.NumPurchases = 0;
            return result;
        }

        public TradeResult TryPurchaseItem(TradeRequest request)
        {
            // Dev Note: I tried to make this as flexible as possible. If there are not enough "hook" spots, at the very least you can Harmony Patch the individual methods
            // Im hoping it won't come to that, and this should be enough for just about every use case I can think if, but if you need more then feel free to let me know!

            // Anything related to permissions should be in the Pre/Post Process step - some things I can think of are villages/cities where you need to be a member of that
            // village or city to be able to buy things from there, modifying the cost per purchase number to include taxes which will be sent to a city "vault" or something
            // in the PreFinalizeTrade hook before we send the rest to the Register, or if you wanted to have some sort of general whitelist/blacklist.

            // Anything related to recording sales like the Ledger system, modifying where payment/product goes, sending off ingame messages to the seller, or interacting with
            // an external API like the cross-server-trading server should go in the Pre/Post Finalize steps. At this point the trade is valid, the items have been removed from
            // the source slots and its just a matter of sending things where they should go

            if (request == null) return new TradeResult(null) { ErrorMsg = TradingConstants.PURCHASED_ZERO }; 

            // Step 1: Validate the trade by checking if we have enough currency, enough stock, permissions to trade, etc.
            bool runProcessing = PreProcessTrade(request);
            TradeResult result = ProcessTrade(request, runProcessing); //TODO: Pointless runProcessing variable passing?
            if (result.ErrorMsg != null) return result;
            PostProcessTrade(result);
            if (result.ErrorMsg != null) return result;

            // Step 2: At this point the trade is "Valid" and we can commit to the trade.
            // Extract all the items from the Source Slots into the TradeResult's aggregated item stacks
            ExtractItems(result);

            //Step 3: Log the sale to the ledger before the items are removed from the aggregates or processed by other mods
            LogPurchase(result);

            //Step 4: Now that we have taken the currency from the player, product from the shop, etc. we need to put the items in their proper places
            PreFinalizeTrade(result);
            FinalizeTrade(result);
            PostFinalizeTrade(result);

            return result;
        }

        private void LogPurchase(TradeResult result)
        {
            DB.SavePurchase(result);
        }

        /// <summary>
        /// Extracts the Product, Currency, and Coupons out of the shop and players inventory and places the raw stacks into the TradeResult to be distributed by
        /// the FinalizeTrade step.
        /// </summary>
        /// <param name="result"></param>
        private void ExtractItems(TradeResult result)
        {
            AggregatedSlots products = result.Request.ProductSourceSlots;
            int totalProductToMove = result.Request.GetFinalProductNeededPerPurchase() * result.Request.NumPurchases;
            AggregatedStacks productStacks = result.ProductStacks;

            foreach (ItemSlot slot in products)
            {
                ItemStack takenStack = slot.TakeOut(totalProductToMove);
                if (takenStack != null)
                {
                    this.Mod.Logger.Debug($"Took out {takenStack.StackSize}x {takenStack} product from Product Stacks");
                    totalProductToMove -= takenStack.StackSize;
                    productStacks.Add(takenStack);
                    slot.MarkDirty();
                }

                if (totalProductToMove <= 0)
                {
                    if (totalProductToMove < 0)
                    {
                        this.Mod.Logger.Error($"Somehow removed {Math.Abs(totalProductToMove)} extra items from Product");
                    }
                    break;
                }

            }

            AggregatedSlots currency = result.Request.CurrencySourceSlots;
            int totalCurrencyToMove = result.Request.GetFinalCurrencyNeededPerPurchase() * result.Request.NumPurchases;
            AggregatedStacks currencyStacks = result.CurrencyStacks;
            foreach (ItemSlot slot in currency)
            {
                ItemStack takenStack = slot.TakeOut(totalCurrencyToMove);
                if (takenStack != null)
                {
                    this.Mod.Logger.Debug($"Took out {takenStack.StackSize}x {takenStack} product from Currency Stacks");
                    totalCurrencyToMove -= takenStack.StackSize;
                    currencyStacks.Add(takenStack);
                    slot.MarkDirty();
                }

                if (totalCurrencyToMove <= 0)
                {
                    if (totalCurrencyToMove < 0)
                    {
                        this.Mod.Logger.Error($"Somehow removed {Math.Abs(totalCurrencyToMove)} extra items from Currency");
                    }
                    break;
                }

            }

            AggregatedSlots coupons = result.Request.CouponSourceSlots;
            if (coupons != null)
            {
                int totalCouponsToMove = result.Request.NumPurchases;
                AggregatedStacks couponStacks = result.CouponStacks;
                foreach (ItemSlot slot in coupons)
                {
                    ItemStack takenStack = slot.TakeOut(totalCouponsToMove);
                    if (takenStack != null)
                    {
                        this.Mod.Logger.Debug($"Took out {takenStack.StackSize}x {takenStack} product from Coupon Stacks");
                        totalCouponsToMove -= takenStack.StackSize;
                        couponStacks.Add(takenStack);
                        slot.MarkDirty();
                    }

                    if (totalCouponsToMove <= 0)
                    {
                        if (totalCouponsToMove < 0)
                        {
                            this.Mod.Logger.Error($"Somehow removed {Math.Abs(totalCouponsToMove)} extra items from Coupons");
                        }
                        break;
                    }
                }
            }
        }

        public SortedList<int, PreProcessTrade> PreProcessTradeHandlers = new SortedList<int, PreProcessTrade>();
        public void RegisterPreProcessTradeHandler(int priority, PreProcessTrade hook)
        {
            PreProcessTradeHandlers.Add(priority, hook);
        }

        public SortedList<int, PostProcessTrade> PostProcessTradeHandlers = new SortedList<int, PostProcessTrade>();
        public void RegisterPostProcessTradeHandlers(int priority, PostProcessTrade hook)
        {
            PostProcessTradeHandlers.Add(priority, hook);
        }

        public SortedList<int, PreFinalizeTrade> PreFinalizeTradeHandlers = new SortedList<int, PreFinalizeTrade>();
        public void RegisterPreFinalizeTradeHandler(int priority, PreFinalizeTrade hook)
        {
            PreFinalizeTradeHandlers.Add(priority, hook);
        }

        public SortedList<int, PostFinalizeTrade> PostFinalizeTradeHandlers = new SortedList<int, PostFinalizeTrade>();


        public void RegisterPostFinalizeTradeHandler(int priority, PostFinalizeTrade hook)
        {
            PostFinalizeTradeHandlers.Add(priority, hook);
        }

        internal OwnableShopInformation GetShopInformation(int shopId)
        {
            throw new NotImplementedException();
        }
    }

    
}

