using Commercially.Common;
using Commercially.Common.Interfaces;
using Commercially.Common.Renderer;
using Commercially.Common.Slots;
using Commercially.Vinconomy.Interfaces;
using Commercially.Vinconomy.Inventory.Impl;
using Commercially.Vinconomy.Inventory.StallSlots;
using Commercially.Vinconomy.Trading;
using System;
using System.Collections.Generic;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.API.Util;
using Vintagestory.GameContent;

namespace Commercially.Vinconomy.BlockEntityBehaviors
{
    public class BEStallBehavior : BlockEntityBehavior, IStallComponent, IShapeTesselator
    {
        protected VinconomyModSystem VinconomyCore;
        protected CommerciallyModSystem CommerciallyCore;


        IStallInventoryProvider IStallComponent.InventoryProvider => _InventoryProvider;
        IStallInventoryProvider _InventoryProvider;

        IOwnableLeaf IStallComponent.Ownable => _Ownable;
        IOwnableLeaf _Ownable;

        protected bool RequiresParent;

        public int StallCount => _InventoryProvider?.StallCount ?? 0;

        

        public BEStallBehavior(BlockEntity blockentity) : base(blockentity)
        {
        }

        public override void Initialize(ICoreAPI api, JsonObject properties)
        {
            base.Initialize(api, properties);
            _InventoryProvider = this.GetComponent<IStallInventoryProvider>();
            _Ownable = this.GetComponent<IOwnableLeaf>();

        }

        public ItemStack GetCurrencyForStallSlot(int stallSlot)
        {
            return _InventoryProvider.GetStallSlot(stallSlot).Currency.Itemstack?.Clone();
        }

        public ItemStack GetProductForStallSlot(int stallSlot)
        {
            return _InventoryProvider.GetStallSlot(stallSlot).Product.Itemstack?.Clone();
        }

        public StallSlotBase GetStallSlot(int stallSlot)
        {
            return _InventoryProvider.GetStallSlot(stallSlot);
        }

        public T GetStallSlot<T>(int stallSlot) where T : StallSlotBase
        {
            return _InventoryProvider.GetStallSlot<T>(stallSlot);
        }

        public virtual bool CanPurchaseItem(IPlayer player, int stallSlot, int numPurchases)
        {
            if (numPurchases <= 0)
            {
                CommerciallyModSystem.PrintClientMessage(player, TradingConstants.PURCHASED_ZERO);
                return false;
            }

            ItemSlot currency = _InventoryProvider.GetStallSlot(stallSlot).Currency;

            if (currency.Itemstack == null)
            {
                CommerciallyModSystem.PrintClientMessage(player, TradingConstants.NO_PRICE);
                return false;
            }

            if (RequiresParent)
            {
                
                // Does the shop have a Parent ID set if it needs one?
                if (_Ownable == null || _Ownable.ParentID != -1)
                {
                    CommerciallyModSystem.PrintClientMessage(player, TradingConstants.NOT_REGISTERED);
                    return false;
                }


                // Is there a shop with the given Register ID?
                IShopInventoryProvider register = VinconomyCore.GetShop(_Ownable.OwnerUID, _Ownable.ParentID);
                if (register == null && !_Ownable.IsAdminOwned)
                {
                    CommerciallyModSystem.PrintClientMessage(player, TradingConstants.COULDNT_GET_REGISTER);
                    return false;
                }
            }



            ItemSlot[] stockSlots = _InventoryProvider.GetStallSlot(stallSlot).GetStallSlots(); ;
            ItemSlot purchaseSlot = null;

            //Find the first slot available that we can purchase from
            foreach (var stockSlot in stockSlots)
            {
                if (stockSlot.StackSize >= 0)
                {
                    purchaseSlot = stockSlot;
                    break;
                }
            }
            if (purchaseSlot == null)
            {
                CommerciallyModSystem.PrintClientMessage(player, TradingConstants.NO_PRODUCT);
                return false;
            }

            return false; //core.CanPurchaseItem(player, this, parent, stallSlot, numPurchases);
        }

        //public virtual ItemStack[] TakeProduct



        public virtual bool TryPurchaseItem(IPlayer player, int stallSlot, int numPurchases)
        {
            // Step 1:   Can they purchase the item through local means - enough stock, enough currency, etc.
            // Step 2:   Is there a Parent that needs to be loaded? If so, try to load it in the world
            // Step 2.5: Resume call if we needed to wait for the chunk to load
            // Step 3:   If there is a parent where the items need to be inserted, check if it can fit
            // Step 4





            if (CanPurchaseItem(player, stallSlot, numPurchases))
            {

            }

            IOwnable parent = this.Blockentity.GetBehavior<IOwnableLeaf>();



            //if (core.CanPurchaseItem(player, this, ))

            return false;
        }

        public virtual void PurchaseItem(IPlayer player, int stallSlot, int numPurchases, IShopComponent shopRegister)
        {
            TradeRequest request = new TradeRequest(Api, player);
            IOwnable ownable = this.GetComponent<IOwnable>();
            ItemStack currencyStack = GetCurrencyForStallSlot(stallSlot);
            ItemStack productStack = GetProductForStallSlot(stallSlot);
            request.WithShop(shopRegister, this, stallSlot, ownable.IsAdminOwned);
            request.WithPurchases(numPurchases);
            request.WithCurrency(currencyStack, TradingUtil.GetAllValidSlotsFor(player, currencyStack), currencyStack.StackSize);
            request.WithProduct(productStack, GetStallSlot(stallSlot).GetProducts(), productStack.StackSize);

            AggregatedSlots coupons = TradingUtil.GetCouponsSlotsFor(player, request.ProductNeeded, shopRegister);
            if (coupons.Slots.Count > 0)
            {
                request.WithCoupons(coupons.Slots[0]);
            }

            request.WithTools(GetRequiredTools(player, stallSlot), 1);


            if (shopRegister != null)
            {
                RegisterInventory inv = shopRegister.GetComponent<IInventoryProvider>()?.Inventory as RegisterInventory;
                if (inv != null)
                {
                    ItemStack tradePass = inv.GetTradePass();
                    if (tradePass != null)
                    {
                        request.WithTradePass(tradePass, TradingUtil.GetAllValidSlotsFor(player, tradePass));
                    }
                }
            }


            request.Build();

            TradeResult result = VinconomyCore.TryPurchaseItem(request);
            if (result.ErrorMsg != null)
            {
                CommerciallyModSystem.PrintClientMessage(player, result.ErrorMsg);
            }
            else
            {
                Blockentity.MarkDirty(true, null);
                //Blockentity.UpdateMeshes();
            }
        }

        private AggregatedSlots GetRequiredTools(IPlayer player, int stallSlot)
        {
            return null;
        }

        public int GetRemainingProductForStallSlot(int stallSlot)
        {
            return _InventoryProvider.GetStallSlot(stallSlot).GetProducts().TotalCount;
        }

        public override bool OnTesselation(ITerrainMeshPool mesher, ITesselatorAPI tessThreadTesselator)
        {
            TesselateDisplayedItems(mesher, tessThreadTesselator);
            TesselateDecoBlock(mesher, tessThreadTesselator);
            return base.OnTesselation(mesher, tessThreadTesselator);
        }

        protected void TesselateDisplayedItems(ITerrainMeshPool mesher, ITesselatorAPI tessThreadTesselator)
        {
            if (mesher == null)
                return;
            /*

            if (shouldRenderInventory)
            {
                MeshData mesh = null;
                ItemSlot slot = null;
                for (int i = 0; i < StallSlotCount; i++)
                {
                    try
                    {
                        slot = inventory.FindFirstNonEmptyStockSlot(i);
                        if (slot != null && !slot.Empty && tfMatrices != null)
                        {
                            mesh = getOrCreateMesh(slot, i);
                            if (mesh != null)
                            {
                                mesher.AddMeshData(mesh, tfMatrices[i]);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        modSystem.Mod.Logger.Error($"Had some trouble rendering mesh in a stall @ {Pos.X} {Pos.Y} {Pos.Z} for slot {i}. Exception was {e.Message}");
                    }

                }
            }
            */
        }

        protected virtual void TesselateDecoBlock(ITerrainMeshPool mesher, ITesselatorAPI tessThreadTesselator)
        {

            if (_InventoryProvider.GetDecorationStack() != null)
            {
                ItemSlot decoration = _InventoryProvider.GetDecorationSlot();

                MeshData mesh = CommerciallyCore.GetRenderer(decoration).CreateMesh(this, decoration, 0);
                mesh = mesh.Clone().Rotate(new Vec3f(0.5f, 0.5f, 0.5f), 0, (float)((Block.Shape.rotateY * Math.PI) / 180), 0);
                mesher.AddMeshData(mesh);
            }
        }

        protected virtual void UpdateMesh(int index)
        {
            if (Api != null && Api.Side != EnumAppSide.Server && !_InventoryProvider.Inventory[index].Empty)
            {
                GetOrCreateMesh(_InventoryProvider.Inventory[index], index);
            }
        }

        protected virtual string GetMeshCacheKey(ItemSlot slot)
        {
            ItemStack stack = slot.Itemstack;
            if (stack == null)
                return null;

            if (stack.Collectible is IContainedMeshSource containedMeshSource)
            {
                return containedMeshSource.GetMeshCacheKey(slot);
            }

            return stack.Collectible.Code.ToString();
        }

        protected MeshData GetMesh(ItemSlot stack)
        {
            string meshCacheKey = GetMeshCacheKey(stack);
            MeshCache.TryGetValue(meshCacheKey, out var value);
            return value;
        }

        protected MeshData GetOrCreateMesh(ItemSlot slot, int index)
        {
            MeshData modeldata = GetMesh(slot);
            if (modeldata != null)
            {
                return modeldata;
            }

            IItemRenderer renderer = CommerciallyCore.GetRenderer(slot);
            if (renderer != null)
            {
                ItemStack stack = slot.Itemstack;
                modeldata = renderer.CreateMesh(this, slot, index);
                if (modeldata == null)
                {
                    //Don't crash if we couldnt get the model for some reason
                    return null;
                }

                //Bypass the Display and Shelvable transforms for Armor Stands, where we want the model coordinates to match the character, not the zero'd positions.
                if (!bypassShelvableAttributes)
                {
                    ModelTransform modelTransform = null;
                    // pick our preselected Attribute Transform Code
                    if (stack.Collectible.Attributes?[AttributeTransformCode].Exists ?? false)
                    {
                        modelTransform = stack.Collectible.Attributes?[AttributeTransformCode].AsObject<ModelTransform>();
                    }
                    else if (stack.Block is IShelvable)
                    {
                        modelTransform = (stack.Block as IShelvable).GetOnShelfTransform(stack);
                    }
                    else if (stack.Collectible.Attributes?["onDisplayTransform"].Exists ?? false)
                    {
                        modelTransform = stack.Collectible.Attributes?["onDisplayTransform"].AsObject<ModelTransform>();

                    }
                    else if (stack.Collectible.Attributes?["groundStorageTransform"].Exists ?? false)
                    {
                        modelTransform = stack.Collectible.Attributes?["groundStorageTransform"].AsObject<ModelTransform>();

                    }

                    if (modelTransform != null)
                    {
                        modelTransform.EnsureDefaultValues();
                        modeldata.ModelTransform(modelTransform);
                    }
                    // Should be handled by IShelvable, but I still see it in the JSON
                    else if (stack.Collectible.Attributes?["shelvable"].Exists ?? false)
                    {
                        modeldata.Scale(new Vec3f(0.5f, 0.0f, 0.5f), 0.85f, 0.85f, 0.85f);
                    }
                    else
                    {
                        modeldata.Scale(new Vec3f(0.5f, 0.0f, 0.5f), 0.35f, 0.35f, 0.35f);
                    }

                }

                if (stack.Class == EnumItemClass.Item && (stack.Item.Shape == null || stack.Item.Shape.VoxelizeTexture))
                {
                    modeldata.Rotate(new Vec3f(0.5f, 0.5f, 0.5f), MathF.PI / 2f, 0f, 0f);
                    modeldata.Scale(new Vec3f(0.5f, 0.5f, 0.5f), 0.35f, 0.35f, 0.35f);
                    modeldata.Translate(0f, -15f / 32f, 0f);
                }


                if (renderer.ShouldCache(stack))
                {
                    string meshCacheKey = GetMeshCacheKey(slot);
                    MeshCache[meshCacheKey] = modeldata;
                }
            }



            return modeldata;
        }
        
        public virtual string ClassCode => _InventoryProvider.Inventory.ClassName;
        protected Dictionary<string, MeshData> MeshCache => ObjectCacheUtil.GetOrCreate(Api, "meshesDisplay-" + ClassCode, () => new Dictionary<string, MeshData>());

        public Size2i AtlasSize => throw new NotImplementedException();

        public TextureAtlasPosition this[string textureCode] => throw new NotImplementedException();

        protected float[][] GenTransformationMatrices()
        {
            int stallCount = _InventoryProvider.StallCount;
            float[][] tfMatrices = new float[stallCount][];
            for (int index = 0; index < stallCount; index++)
            {
                Cuboidf sb = Block.SelectionBoxes[index];
                float left = -.25f;
                float right = left + .5f;

                float x = (index % 2 == 0) ? left : right;
                float y = sb.YSize <= .45f ? sb.MaxY - 0.39f + (.45f - sb.YSize) : sb.MaxY - 0.39f;
                float z = (index / 2 == 0) ? left : right;
                Matrixf matrix = new Matrixf().Translate(0.5f, 0f, 0.5f).RotateYDeg(Block.Shape.rotateY).Translate(x, y, z).Translate(-0.5f, 0f, -0.5f);
                tfMatrices[index] = matrix.Values;
            }
            return tfMatrices;
        }


        CollectibleObject nowTesselatingObj = null;
        Shape nowTesselatingShape = null;
        private string AttributeTransformCode;
        private bool bypassShelvableAttributes;

        public void SetNowTesselatingObj(CollectibleObject collectible)
        {
            nowTesselatingObj = collectible;
            nowTesselatingShape = null;
        }

        public void SetNowTesselatingShape(Shape shape)
        {
            nowTesselatingShape = shape;
            nowTesselatingObj = null;
        }
    }
}
