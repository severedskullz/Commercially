using Vintagestory.API.Common;

namespace Vinconomy.Inventory.Slots
{
    public class ItemLockedSlot : ItemSlot
    {
        

        public ItemLockedSlot(InventoryBase inventory) : base(inventory)
        {

            //this.HexBackgroundColor = "#12526B";
        }

        public override bool CanHold(ItemSlot sourceSlot)
        {
            return false;
        }

        public override bool CanTake()
        {
            return false;
        }

        public override bool CanTakeFrom(ItemSlot sourceSlot, EnumMergePriority priority = EnumMergePriority.AutoMerge)
        {
            return false;
        }

    }
    
}
