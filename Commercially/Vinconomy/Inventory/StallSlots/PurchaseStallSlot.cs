using Commercially.Common.Interfaces;
using Commercially.Common.Inventory.Slots;
using Commercially.Common.Util;
using Commercially.Vinconomy.Interfaces;
using Commercially.Vinconomy.Trading;
using System;
using Vinconomy.Inventory.Slots;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;
using Vintagestory.API.Util;

namespace Commercially.Vinconomy.Inventory.StallSlots
{
    public class PurchaseStallSlot : StallSlotBase, ICurrencySinkProvider
    {
        public override int StallSlotCount => (PurchasedProduct?.Length ?? 0) + (ProvidedCurrency?.Length ?? 0);

        public override bool IsInitialized => PurchasedProduct != null && ProvidedCurrency != null;

        public bool RegisterFallback { get; set; }

        public ItemSlot[] CurrencySlots => PurchasedProduct;

        InventoryBase IInventoryProvider.Inventory => Inventory;

        public StockItemSlot[] ProvidedCurrency;
        public FilteredItemSlot[] PurchasedProduct;
        public bool IsLimited;
        public int NumPurchasesRemaining;

        public PurchaseStallSlot(VinconBaseInventory inventory, int stallSlot) : base(inventory, stallSlot)
        {
        }

        public PurchaseStallSlot(VinconBaseInventory inventory, int stallSlot, int currencySlotsPerStall, int purchasedStockPerStall) : base(inventory, stallSlot)
        {
            Product.BackgroundIcon = "commercially-payment";
            ProvidedCurrency = new StockItemSlot[currencySlotsPerStall];
            for (int i = 0; i < currencySlotsPerStall; i++)
            {
                ProvidedCurrency[i] = new StockItemSlot(inventory, StallSlot, i)
                {
                    BackgroundIcon = "commercially-payment"
                };

            }
            Currency.BackgroundIcon = "commercially-general";
            PurchasedProduct = new FilteredItemSlot[purchasedStockPerStall];
            for (int i = 0; i < purchasedStockPerStall; i++)
            {
                PurchasedProduct[i] = new FilteredItemSlot(inventory)
                {
                    BackgroundIcon = "commercially-general"
                };
            }
        }

        public override ItemSlot this[int slotId] {
            get 
            {
                if (slotId == 0) return Currency;
                else if (slotId == 1) return Product;
                else if (slotId - 2 < ProvidedCurrency.Length) return ProvidedCurrency[slotId - 2];
                else return PurchasedProduct[slotId - ProvidedCurrency.Length - 2];
            }
            set {
                if (slotId == 0) Currency = (CurrencySlot) value;
                else if (slotId == 1) Product = (ProductSlot) value;
                else if (slotId - 2 < ProvidedCurrency.Length) ProvidedCurrency[slotId - 2] = (StockItemSlot)value;
                else PurchasedProduct[slotId - ProvidedCurrency.Length - 2] = (FilteredItemSlot)value;
            } 
        }

        public override ItemSlot[] GetProductSlots()
        {
            return ProvidedCurrency;
        }

        public ItemSlot[] GetProductAndParentSlots()
        {
            ItemSlot[] currency = GetProductSlots();
            if (this.RegisterFallback && this.Inventory.StallComponent.Ownable.HasParent())
            {
                ItemSlot[] parentCurrency = GetParentCurrencySlots();
                return currency.Append(parentCurrency);
            }
            return currency;
        }

        public ItemSlot[] GetParentCurrencySlots()
        {
            ICurrencySinkProvider provider = this.Inventory.StallComponent?.Ownable?.GetParent().GetComponent<ICurrencySinkProvider>();
            if (provider != null)
            {
                return provider.CurrencySlots;
            }
            return [];
        }

        public ItemSlot[] GetPurchasedProducts()
        {
            return PurchasedProduct;
        }

        public override void ToTreeAttributes(ITreeAttribute tree)
        {
            base.ToTreeAttributes(tree);
            tree.SetInt("numSlots", ProvidedCurrency.Length);
            for (int i = 0; i < ProvidedCurrency.Length; i++)
            {
                if (ProvidedCurrency[i].Itemstack != null)
                {
                    tree.SetItemstack("slot" + i, ProvidedCurrency[i].Itemstack);
                }
            }

            tree.SetInt("numProductSlots", PurchasedProduct.Length);
            for (int i = 0; i < PurchasedProduct.Length; i++)
            {
                if (PurchasedProduct[i].Itemstack != null)
                {
                    tree.SetItemstack("productSlot" + i, PurchasedProduct[i].Itemstack);
                }
            }

            tree.SetBool("isLimited", IsLimited);
            tree.SetBool("registerFallback", RegisterFallback);
            tree.SetInt("numPurchasesRemaining", NumPurchasesRemaining);

        }

        public override void FromTreeAttributes(ITreeAttribute tree)
        {
            base.FromTreeAttributes(tree);

            int numCurrencySlots = tree.GetInt("numSlots",20);
            int numProductSlots = tree.GetInt("numProductSlots", 30);

            if (!IsInitialized)
            {
                ProvidedCurrency = new StockItemSlot[numCurrencySlots];
                for (int i = 0; i < numCurrencySlots; i++)
                {
                    StockItemSlot slot = new StockItemSlot(Inventory, StallSlot, i);
                    ItemStack itemStack = tree.GetItemstack("slot" + i);
                    slot.Itemstack = itemStack;
                    slot.BackgroundIcon = "commercially-payment";
                    ProvidedCurrency[i] = slot;
                    
                    
                    if (Inventory.Api?.World != null)
                    {
                        itemStack?.ResolveBlockOrItem(Inventory.Api.World);
                    }
                }

                PurchasedProduct = new FilteredItemSlot[numProductSlots];
                for (int i = 0; i < numProductSlots; i++)
                {
                    FilteredItemSlot slot = new FilteredItemSlot(Inventory);
                    ItemStack itemStack = tree.GetItemstack("productSlot" + i);
                    slot.Itemstack = itemStack;
                    slot.BackgroundIcon = "commercially-general";
                    PurchasedProduct[i] = slot;
                    if (Inventory.Api?.World != null)
                    {
                        itemStack?.ResolveBlockOrItem(Inventory.Api.World);
                    }
                }
            } else
            {
                for (int i = 0; i < numCurrencySlots; i++)
                {
                    ItemStack itemStack = tree.GetItemstack("slot" + i);
                    ProvidedCurrency[i].Itemstack = itemStack;
                    if (Inventory.Api?.World != null)
                    {
                        itemStack?.ResolveBlockOrItem(Inventory.Api.World);
                    }
                }

                for (int i = 0; i < numProductSlots; i++)
                {
                    ItemStack itemStack = tree.GetItemstack("productSlot" + i);
                    PurchasedProduct[i].Itemstack = itemStack;
                    if (Inventory.Api?.World != null)
                    {
                        itemStack?.ResolveBlockOrItem(Inventory.Api.World);
                    }
                }
            }

            IsLimited = tree.GetBool("isLimited", false);
            RegisterFallback = tree.GetBool("registerFallback", false);
            NumPurchasesRemaining = tree.GetInt("numPurchasesRemaining", 0);
        }

        public override void Initialize(VinconBaseInventory inventory, int stallSlot, int numSlotsPerStall)
        {
            base.Initialize(inventory, stallSlot, numSlotsPerStall);

            if (!IsInitialized)
            {
                ProvidedCurrency = new StockItemSlot[numSlotsPerStall];
                for (int i = 0; i < numSlotsPerStall; i++)
                {
                    ProvidedCurrency[i] = new StockItemSlot(inventory, StallSlot, i);
                }

                //TODO: This count will be wrong. How can we add another "numSlotsPerStall" for the purchased product slots?
                PurchasedProduct = new StockItemSlot[numSlotsPerStall];
                for (int i = 0; i < numSlotsPerStall; i++)
                {
                    PurchasedProduct[i] = new StockItemSlot(inventory, StallSlot, i);
                }
            }
        }

        public override void TransferProdutToPlayer(TradeResult result)
        {
            if (result.ProductStacks.TotalCount == 0) return;

            IPlayer player = result.Request.Customer;
            AssetLocation sound = null;
            while (result.ProductStacks.CanRemoveStack())
            {
                ItemStack stack = result.ProductStacks.RemoveStack();

                if (stack != null)
                {
                    this.Inventory.modSystem.Mod.Logger.Debug($"Adding {stack.StackSize}x {stack} product to Parent");
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

        public override void ExtractProductFromStall(TradeResult result)
        {
            if (IsLimited)
            {
                NumPurchasesRemaining -= result.FinalPurchases;
                if (NumPurchasesRemaining < 0)
                {
                    this.Inventory.modSystem.Mod.Logger.Error($"Somehow removed {Math.Abs(NumPurchasesRemaining)} extra purchases from Purchase Stall");
                    NumPurchasesRemaining = 0;
                }
            }

            int totalProductToMove = result.TotalProductAmount;
            AggregatedStacks productStacks = result.ProductStacks;
           
            if (result.Request.IsAdminShop)
            {
                int maxStackSize = result.Request.ProductNeeded.Collectible.MaxStackSize;
                while (totalProductToMove > 0)
                {
                    ItemStack transferStack = result.Request.ProductNeeded.Clone();
                    int stackSize = Math.Min(totalProductToMove, maxStackSize);
                    transferStack.StackSize = stackSize;
                    productStacks.Add(transferStack);
                    totalProductToMove -= stackSize;
                }
            } else {
                AggregatedSlots products = result.Request.ProductSourceSlots;
                foreach (ItemSlot slot in products)
                {
                    ItemStack takenStack = slot.TakeOut(totalProductToMove);
                    if (takenStack != null)
                    {
                        this.Inventory.modSystem.Mod.Logger.Debug($"Took out {takenStack.StackSize}x {takenStack} product from Product Stacks");
                        totalProductToMove -= takenStack.StackSize;
                        productStacks.Add(takenStack);
                        slot.MarkDirty();
                    }

                    if (totalProductToMove <= 0)
                    {
                        if (totalProductToMove < 0)
                        {
                            this.Inventory.modSystem.Mod.Logger.Error($"Somehow removed {Math.Abs(totalProductToMove)} extra items from Product");
                        }
                        break;
                    }
                }
            }

            if (totalProductToMove > 0)
            {
                this.Inventory.modSystem.Mod.Logger.Error($"Somehow missing {totalProductToMove}  items from Product");
            }
        }

        /// <summary>
        /// Reminder that in the case of Purchase Stalls, Currency here is the purchased item.
        /// </summary>
        /// <param name="result"></param>
        public override void TransferCurrencyToOwnable(TradeResult result)
        {
            if (result.CurrencyStacks.TotalCount == 0) return;

            ILogger logger = this.Inventory.modSystem.Mod.Logger;
            while (result.CurrencyStacks.CanRemoveStack())
            {
                ItemStack nextStack = result.CurrencyStacks.RemoveStack();
                logger.Debug($"Adding {nextStack.StackSize}x {nextStack} currency to Parent");
                AddItemToSlots(result.Request.Api, nextStack, PurchasedProduct);
            }
            this.Inventory.BlockEntity.MarkDirty();
        }

        public override int GetNumPurchasesRemaining()
        {
            int numPurchasesRemaining = GetProductQuantity() / ProductPerPurchase;
            if (!IsLimited) return numPurchasesRemaining;
            return Math.Min(NumPurchasesRemaining, numPurchasesRemaining);
        }
    }
}
