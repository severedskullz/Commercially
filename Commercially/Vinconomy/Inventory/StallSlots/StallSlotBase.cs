using Commercially.Common.Interfaces;
using Commercially.Common.Inventory.Slots;
using Commercially.Common.Util;
using Commercially.Vinconomy.Interfaces;
using Commercially.Vinconomy.Inventory.Impl;
using Commercially.Vinconomy.Trading;
using System;
using Vinconomy.Inventory.Slots;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;

namespace Commercially.Vinconomy.Inventory.StallSlots
{
    public abstract class StallSlotBase
    {
        public const string PRODUCT = "product";
        public const string CURRENCY = "currency";
        public const string FUZZY_MATCHING = "fuzzyMatching";


        public InventoryBase Inventory { get; protected set; }

        /// <summary>
        /// The Currency used for the purchase. The StackSize should be representitive of how much an item costs. For example, if something were to cost 6 Rusty Gears, its stack size would be 6. 
        /// </summary>
        public VinconCloningSlot Currency { get; protected set; }

        /// <summary>
        /// The Product given to the customer. The StackSize should be representitive of how much of an item is given to the customer. For example, if the shop were to be selling a stack of 64 Dirt, then 
        /// the stack size would be 64.
        /// </summary>
        public FilteredItemSlot Product { get; protected set; }

        /// <summary>
        /// How many slots are considered "Product" that the player can fill in to sell items from.
        /// </summary>
        public virtual int StallSlotCount => GetProductSlots().Length;

        /// <summary>
        /// How many slots are considered "Internal" and not a part of the Product slots. At the very least each stall should have 2 slots: Currency and Product.
        /// If your implementation needs more slots, simply increase this number to account for all the slots added. This field is used when enumerating over the entire inventory
        /// as well as accessing individual item slots for the GUI components
        /// </summary>
        public virtual int InternalSlotCount => 2; // Currency + Product slot

        /// <summary>
        /// The total number of slots that this stall should take up - inclusive of both Internal (Currency, Product, etc.) and Product (Items for sale) slots
        /// </summary>
        public virtual int TotalSlotCount => InternalSlotCount + StallSlotCount;
        public int ProductPerPurchase { get => Math.Max(1,Product.StackSize); set => Product.Itemstack?.StackSize = value; }
        public int CurrencyPerPurchase { get => Math.Max(1, Currency.StackSize); set => Currency.Itemstack?.StackSize = value; }

        public bool IsFuzzyMatching {  get; set; }

        public abstract bool IsInitialized { get; }

        public int StallSlot { get; protected set; }

        /// <summary>
        /// Accessor for the slots provided by the given stall. For the sake of consistency in implementations: Internal slots should be first, followed by Product slots.
        /// </summary>
        /// <param name="slotId"></param>
        /// <returns></returns>
        public abstract ItemSlot this[int slotId] { get; set; }

        public StallSlotBase(InventoryBase inventory, int stallSlot) {
            this.Inventory = inventory;
            this.StallSlot = stallSlot;
            Currency = new VinconCloningSlot(inventory);
            Product = new VinconCloningSlot(inventory);
        }

        public virtual ItemSlot GetInternalSlot(int slotId)
        {
            if (slotId == 0) return Currency;
            else return Product;
        }

        public virtual void ToTreeAttributes(ITreeAttribute tree)
        {
            tree.SetItemstack(CURRENCY, Currency.Itemstack);
            tree.SetItemstack(PRODUCT, Product.Itemstack);
            tree.SetBool(FUZZY_MATCHING, IsFuzzyMatching);

        }
        public virtual void FromTreeAttributes(ITreeAttribute tree)
        {

            //This technically gets called BEFORE Initialize() does, so Currency and Product might be null
            ItemStack currencyStack = tree.GetItemstack(CURRENCY);
            
            Currency.Itemstack = currencyStack;


            ItemStack productStack = tree.GetItemstack(PRODUCT);
            
            Product.Itemstack = productStack;

            if (Inventory.Api?.World != null)
            {
                currencyStack?.ResolveBlockOrItem(Inventory.Api.World);
                productStack?.ResolveBlockOrItem(Inventory.Api.World);
            }

            IsFuzzyMatching = tree.GetBool(FUZZY_MATCHING);
        }

        public virtual int GetNumPurchasesRemaining()
        {
            return GetProductQuantity() / ProductPerPurchase;
        }

        public virtual int GetProductQuantity()
        {
            if (Product?.Itemstack == null) return 0;

            ItemSlot[] items = GetProductSlots();
            int amount = 0;
            foreach (ItemSlot item in items)
            {
                if (item?.Itemstack != null && item?.Itemstack.Collectible.Code == Product?.Itemstack?.Collectible.Code)
                {
                    amount += item.Itemstack.StackSize;
                }
            }

            return amount;
        }


        public virtual ItemSlot FindFirstNonEmptyStockSlot()
        {
            ItemSlot[] slots = GetProductSlots();
            foreach (var item in slots)
            {
                if (TradingUtil.IsMatchingItem(Product?.Itemstack, item?.Itemstack, this.Inventory.Api.World, IsFuzzyMatching))
                {
                    return item;
                }
            }
            return null;
        }

        /// <summary>
        /// Lazily Initialize the given Stall with the parent inventory as the provided stall slot. Does not instantiate the stall slots to prepare the stall for FromTreeAttributes or manual resolution
        /// </summary>
        /// <param name="inventory"></param>
        /// <param name="stallSlot"></param>
        public abstract void PreInitialize(VinconBaseInventory inventory, int stallSlot);
        public abstract void Initialize(VinconBaseInventory vinconBaseInventory, int stallSlot, int numSlotsPerStall);
        public abstract ItemSlot GetProductSlot(int itemSlot);
        public abstract ItemSlot[] GetProductSlots();

        public abstract AggregatedSlots GetProducts();

        public virtual bool MatchesProduct(ItemStack itemStack)
        {
            return TradingUtil.IsMatchingItem(Product?.Itemstack, itemStack, this.Inventory.Api.World, IsFuzzyMatching);
        }

        /// <summary>
        /// Takes the product from the stall slot and places it into the destination slot if it is provided.
        /// If the destination slot is not provided or it could not fit the product, returnedItems will be populated instead.
        /// <br/>
        /// This gives us the flexibility to either place the product directly into a slot (such as when the product is a meal, then outputSlot should have a bowl, crock, or pot and then we should be incrementing the serving counts)
        /// or return it to the caller for further processing.
        /// <br/>
        /// </summary>
        /// <param name="amount"></param>
        /// <param name="returnedItems"></param>
        /// <param name="outputSlot"></param>
        /// <param name="allowExcess">Whether or not to continue taking items if the destination slot is full. This value is ignored if outputSlot is null</param>
        /// <returns>True if any product was successfully taken from the stall slot, false otherwise</returns>        
        public virtual int TakeProductFromSlot(int amount, out AggregatedStacks returnedItems, ItemSlot? outputSlot, bool allowExcess = false)
        {
            returnedItems = null;
            ItemSlot[] slots = GetProductSlots();

            int amountItem = amount;
            int movedItems = 0;

            foreach (var slot in slots)
            {
                int moved = 0;
                if (outputSlot != null)
                {
                    moved = slot.TryPutInto(Inventory.Api.World, outputSlot, amountItem);
                    if (moved == 0 && allowExcess)
                    {
                        ItemStack takenStack = slot.TakeOut(amountItem);
                        moved = takenStack.StackSize;
                        returnedItems ??= new AggregatedStacks(); 
                        returnedItems.Add(takenStack);
                    }
                }

                movedItems += moved;
                amountItem -= moved;
                
                if (amountItem <= 0) break;
            }


            return movedItems;
        }

        public virtual int AddProductToSlot(IPlayer byPlayer, ItemSlot sourceSlot, bool bulk)
        { 
            return AddProductToSlot(byPlayer, sourceSlot, bulk ? sourceSlot.StackSize : 1);
        }


        /// <summary>
        /// Adds the product from the specified stall slot and places it into the destination slot if it is provided.
        /// </summary>
        /// <param name="amount"></param>
        /// <param name="sourceSlot"></param>
        /// <returns>True if any product was successfully added to the stall slot, false otherwise</returns>        
        public virtual int AddProductToSlot(IPlayer byPlayer, ItemSlot sourceSlot, int amount)
        {
            if (!MatchesProduct(sourceSlot.Itemstack)) return 0;

            ItemSlot[] slots = GetProductSlots();

            int amountItem = amount;
            int movedItems = 0;

            foreach (var slot in slots)
            {
                if (sourceSlot.Itemstack != null)
                {
                    int moved = sourceSlot.TryPutInto(Inventory.Api.World, slot, amountItem);
                    amountItem -= moved;
                    if (moved > 0)
                    {
                        movedItems += moved;
                        sourceSlot.MarkDirty();
                        slot.MarkDirty();
                    }

                    if (amountItem <= 0)
                    {
                        break;
                    }
                }
            }
            return movedItems;
        }

        public virtual TradeRequest CreateTradeRequest(IPlayer player, int numPurchases, IShopComponent shop, IStallComponent stall)
        {
            TradeRequest request = new TradeRequest(Inventory.Api, player);
            IOwnable ownable = stall.Ownable;
            ItemStack currencyStack = Currency.Itemstack;
            ItemStack productStack = Product.Itemstack;
            request.WithShop(shop, stall, StallSlot, ownable?.IsAdminOwned ?? false);
            request.WithPurchases(numPurchases);
            request.WithCurrency(currencyStack, TradingUtil.GetAllValidSlotsFor(player, currencyStack), currencyStack.StackSize);
            request.WithProduct(productStack, GetProducts(), productStack.StackSize);

            AggregatedSlots coupons = TradingUtil.GetCouponsSlotsFor(player, request.ProductNeeded, shop);
            if (coupons.Slots.Count > 0)
            {
                request.WithCoupons(coupons.Slots[0]);
            }

            request.WithContainers(GetRequiredContainers(player));

            if (shop != null)
            {
                RegisterInventory inv = shop.GetComponent<IInventoryProvider>()?.Inventory as RegisterInventory;
                if (inv != null)
                {
                    ItemStack tradePass = inv.GetTradePass();
                    if (tradePass != null)
                    {
                        request.WithTradePass(tradePass, TradingUtil.GetAllValidSlotsFor(player, tradePass));
                    }
                }
            }           
            return request.Build();
        }

        /// <summary>
        /// Transfers the product extracted from the stall into the result to the player. How this is done is up to the Stall - for example, a Meal StallSlot will not give items directly to the player, but instead iterate over the Container slots and add the respective servings.
        /// </summary>
        /// <param name="result"></param>
        public abstract void TransferProdutToPlayer(TradeResult result);

        /// <summary>
        /// Extracts the Product from the stall. Should do any conversion neccesary during extraction - for example, converting Meal ingredients into a Bowl with the appropriate servings as the ItemStack's stacksize, or bundling items into a single item like a Gacha Ball, 
        /// </summary>
        /// <param name="result"></param>
        public abstract void ExtractProductFromStall(TradeResult result);

        public virtual CapacityAggregatedSlots GetRequiredContainers(IPlayer player)
        {
            return null;
        }

        public virtual void SetStallFilter(Vintagestory.API.Common.Func<ItemSlot, bool> stallFilter)
        {
            Product.Filter = stallFilter;

            ItemSlot[] slots = GetProductSlots();
            foreach (var item in slots)
            {
                if (item is FilteredItemSlot)
                {
                    ((FilteredItemSlot)item).Filter = stallFilter;
                }
            }
        }
    }
}
