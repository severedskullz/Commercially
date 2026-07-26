using Commercially.Common.Inventory.Slots;
using Commercially.Vinconomy.Interfaces;
using Commercially.Vinconomy.Trading;
using Vintagestory.API.Common;

namespace Commercially.Vinconomy.Inventory.Slots
{
    public class ToggledStockItemSlot : ToggledSlot, IStallProductSlot
    {
        public int stallSlot { get; private set; } = 0;
        public int itemSlot { get; private set; } = 0;

        public ToggledStockItemSlot(InventoryBase inventory, int stallSlot, int itemSlot) : base(inventory)
        {
        }

        public override bool CanHold(ItemSlot sourceSlot)
        {
            if ((Filter?.Invoke(sourceSlot) ?? true) == false)
            {
                return false;
            }

            if (!Enabled)
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
                }
                else if (TradingUtil.IsMatchingItem(slot.Itemstack, sourceSlot.Itemstack, inventory.Api.World))
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

        public override void ActivateSlot(ItemSlot sourceSlot, ref ItemStackMoveOperation op)
        {
            if (Itemstack == null && sourceSlot.Itemstack == null && op.MouseButton == EnumMouseButton.Left)
            {
                this.Enabled = !this.Enabled;
                //this.HexBackgroundColor =  this.Enabled ? null : "#FF0000";
            } else
                base.ActivateSlot(sourceSlot, ref op);
        }


    }
}
