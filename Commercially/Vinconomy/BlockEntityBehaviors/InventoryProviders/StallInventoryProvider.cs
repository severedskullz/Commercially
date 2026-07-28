using Commercially.Common.Blocks.BlockEntityBehaviors;
using Commercially.Vinconomy.Interfaces;
using Commercially.Vinconomy.Inventory;
using Commercially.Vinconomy.Inventory.StallSlots;
using Vintagestory.API.Common;

namespace Commercially.Vinconomy.BlockEntityBehaviors.InventoryProviders
{
    public class StallInventoryProvider : BEBehaviorAbstractContainer, IStallInventoryProvider
    {
        protected VinconBaseInventory _Inventory;
        public override InventoryBase Inventory => _Inventory;

        public int StallCount => _Inventory.StallSlots?.Length ?? 0;

        public StallInventoryProvider(BlockEntity blockentity) : base(blockentity)
        {

        }

        public ItemStack GetCurrencyForStallSlot(int stallSlot)
        {
            return _Inventory.GetStall(stallSlot).Currency?.Itemstack?.Clone();
        }

        public ItemStack GetProductForStallSlot(int stallSlot)
        {
            return _Inventory.GetStall(stallSlot).Product?.Itemstack?.Clone();
        }

        public StallSlotBase GetStallSlot(int stallSlot)
        {
            return _Inventory.GetStall(stallSlot);
        }

        public T GetStallSlot<T>(int stallSlot) where T : StallSlotBase
        {
            return _Inventory.GetStall<T>(stallSlot);
        }

        public ItemStack GetDecorationBlock()
        {
            return _Inventory[0].Itemstack;
        }

        public ItemSlot GetDecorationSlot()
        {
            return _Inventory[0];
        }


    }
}
