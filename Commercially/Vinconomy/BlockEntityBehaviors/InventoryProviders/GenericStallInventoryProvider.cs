using Commercially.Common.BlockEntityBehaviors;
using Commercially.Vinconomy.Interfaces;
using Commercially.Vinconomy.Inventory.Impl;
using Commercially.Vinconomy.Inventory.StallSlots;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;

namespace Commercially.Vinconomy.BlockEntityBehaviors.InventoryProviders
{
    public class GenericStallInventoryProvider : BEBehaviorAbstractContainer, IStallInventoryProvider
    {
        private GenericShopInventory _Inventory;
        public override InventoryBase Inventory => _Inventory;

        public int StallCount => _Inventory.StallSlots?.Length ?? 0;

        public GenericStallInventoryProvider(BlockEntity blockentity) : base(blockentity)
        {
            _Inventory = new GenericShopInventory(blockentity.Api);
        }

        public override void Initialize(ICoreAPI api, JsonObject properties)
        {
            base.Initialize(api, properties);
            _Inventory.Initialize(properties, "GenericShopInventory", this.Pos.ToString(), api);
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

        public ItemStack GetDecorationStack()
        {
            return _Inventory[0].Itemstack;
        }

        public ItemSlot GetDecorationSlot()
        {
            return _Inventory[0];
        }


    }
}
