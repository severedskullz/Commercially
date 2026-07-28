using Commercially.Common.Inventory.Slots;
using Commercially.Vinconomy.Interfaces;
using Commercially.Vinconomy.Inventory;
using Commercially.Vinconomy.Trading;
using System.Runtime.Intrinsics.X86;
using Vintagestory.API.Common;

namespace Vinconomy.Inventory.Slots
{
    public class StockItemSlot : FilteredItemSlot, IStallProductStockSlot
    {
        public int stallSlot { get; private set; } = 0;
        public int itemSlot { get; private set; } = 0;



        public StockItemSlot(InventoryBase inventory, int stallSlot, int itemSlot) : base(inventory)
        {
            this.stallSlot = stallSlot;
            this.itemSlot = itemSlot;
            this.StorageType = EnumItemStorageFlags.General
                | EnumItemStorageFlags.Metallurgy
                | EnumItemStorageFlags.Jewellery
                | EnumItemStorageFlags.Alchemy
                | EnumItemStorageFlags.Agriculture
                | EnumItemStorageFlags.Outfit
                | EnumItemStorageFlags.Backpack;
            //this.HexBackgroundColor = "#65d934";
        }

        public override bool CanHold(ItemSlot sourceSlot)
        {
            if (inventory is VinconBaseInventory vinconInventory)
            {
                ItemSlot productSlot = vinconInventory.GetStall(stallSlot).Product;

                if ( ShouldUpdateProductSlot(productSlot, sourceSlot) )
                {
                    UpdateProductSlot(productSlot, sourceSlot);
                }

                if (ItemMatchesProduct(productSlot.Itemstack, sourceSlot))
                {
                    //Console.WriteLine("Stall Slot " + stallSlot + ":First Non-Empty Slot satisfied, so we called Base");
                    return base.CanHold(sourceSlot);
                }
                else
                {
                    return false;
                }

            }

            //Console.WriteLine("Stall Slot " + stallSlot + ":First Non-Empty Slot was not satisfied, so we return false");
            return base.CanHold(sourceSlot);
        }

        public virtual bool ShouldUpdateProductSlot(ItemSlot productSlot, ItemSlot sourceSlot)
        {
            return productSlot?.Itemstack == null;
        }

        public virtual bool ItemMatchesProduct(ItemStack product, ItemSlot sourceSlot)
        {
            VinconBaseInventory inv = (VinconBaseInventory) inventory;
            return TradingUtil.IsMatchingItem(product, sourceSlot.Itemstack, inventory.Api.World);
        }

        /// <summary>
        /// Updates the product slot for the stall with the given source slot. Returns true if the update is successful.
        /// Consequently, this also means that the item stack can be contained in the stall.
        /// </summary>
        /// <param name="sourceSlot"></param>
        /// <returns></returns>
        public virtual void UpdateProductSlot(ItemSlot productSlot, ItemSlot sourceSlot)
        {

            bool canHold = base.CanHold(sourceSlot);

            if (canHold)
            {
                productSlot.Itemstack = sourceSlot.Itemstack?.Clone();
                productSlot.Itemstack.StackSize = 1;
                productSlot.MarkDirty();
                    
            }
        }

        public override bool CanTakeFrom(ItemSlot sourceSlot, EnumMergePriority priority = EnumMergePriority.AutoMerge)
        {
            //Console.WriteLine("Can Take From " + stallSlot + " called...");
            if (CanHold(sourceSlot))
            {
                return base.CanTakeFrom(sourceSlot, priority);
            }
            return false;
        }

        public void SetFilter(Func<ItemSlot, bool> filter)
        {
            Filter = filter;
        }

        public int GetStall()
        {
            return stallSlot;
        }

        public int GetProductSlot()
        {
            return itemSlot;
        }
    }

}
