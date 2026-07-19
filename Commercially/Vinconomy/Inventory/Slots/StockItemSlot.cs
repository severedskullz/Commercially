using Commercially.Common.Inventory.Slots;
using Commercially.Vinconomy.Interfaces;
using Commercially.Vinconomy.Inventory;
using Commercially.Vinconomy.Trading;
using Vintagestory.API.Common;

namespace Vinconomy.Inventory.Slots
{
    public class StockItemSlot : FilteredItemSlot, IStallProductSlot
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
            if ((Filter?.Invoke(sourceSlot) ?? true) == false)
            {
                return false;
            }

            if (inventory is VinconBaseInventory)
            {
                ItemSlot slot = ((VinconBaseInventory)inventory).GetStall(stallSlot).Product;
                if (slot?.Itemstack == null)
                {
                    bool canHold = base.CanHold(sourceSlot);

                    if (canHold)
                    {
                        slot.Itemstack = sourceSlot.Itemstack.Clone();
                        slot.Itemstack.StackSize = 1;
                        slot.MarkDirty();
                    }

                    return canHold;
                } else if (TradingUtil.IsMatchingItem(slot.Itemstack, sourceSlot.Itemstack, inventory.Api.World))
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
