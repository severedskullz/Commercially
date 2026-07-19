using Vintagestory.API.Common;

namespace Commercially.Common.Inventory.Slots
{
    public class FilteredItemSlot : ItemSlot
    {
        public FilteredItemSlot(InventoryBase inventory) : base(inventory)
        {
        }

        public Func<ItemSlot, bool> Filter { get; set; }

        public override bool CanHold(ItemSlot sourceSlot)
        {
            return Filter?.Invoke(sourceSlot) ?? true &&  base.CanHold(sourceSlot);
        }
    }
}
