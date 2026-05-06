using Commercially.Common.Slots;
using Commercially.Vinconomy.Trading;
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
        public ItemSlot Currency { get; protected set; }

        /// <summary>
        /// The Product given to the customer. The StackSize should be representitive of how much of an item is given to the customer. For example, if the shop were to be selling a stack of 64 Dirt, then 
        /// the stack size would be 64.
        /// </summary>
        public ItemSlot Product { get; protected set; }

        /// <summary>
        /// How many slots are considered "Product" that the player can fill in to sell items from.
        /// </summary>
        public virtual int StallSlotCount => GetStallSlots().Length;

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
        public int ProductPerPurchase { get => Product.StackSize; set => Product.Itemstack?.StackSize = value; }
        public int CurrencyPerPurchase { get => Currency.StackSize; set => Currency.Itemstack?.StackSize = value; }

        public bool IsFuzzyMatching {  get; set; }

        public abstract bool IsInitialized { get; }

        public int StallSlot { get; protected set; }

        /// <summary>
        /// Accessor for the slots provided by the given stall. For the sake of consistency in implementations: Internal slots should be first, followed by Product slots.
        /// </summary>
        /// <param name="slotId"></param>
        /// <returns></returns>
        public abstract ItemSlot this[int slotId] { get; set; }

        public StallSlotBase() { }

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
            ItemStack currencyStack = tree.GetItemstack(CURRENCY);
            Currency = new VinconCloningSlot(this.Inventory)
            {
                Itemstack = currencyStack
            };


            ItemStack productStack = tree.GetItemstack(PRODUCT);
            Product = new VinconCloningSlot(this.Inventory)
            {
                Itemstack = productStack
            };

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

            ItemSlot[] items = GetStallSlots();
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
            ItemSlot[] slots = GetStallSlots();
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
        public abstract ItemSlot GetStallSlot(int itemSlot);
        public abstract ItemSlot[] GetStallSlots();

        public abstract AggregatedSlots GetProducts();
    }
}
