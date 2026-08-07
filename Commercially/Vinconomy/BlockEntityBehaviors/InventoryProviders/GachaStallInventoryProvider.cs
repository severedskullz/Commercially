using Commercially.Common.Blocks.BlockEntityBehaviors;
using Commercially.Vinconomy.Interfaces;
using Commercially.Vinconomy.Inventory.Impl;
using Commercially.Vinconomy.Inventory.StallSlots;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;

namespace Commercially.Vinconomy.BlockEntityBehaviors.InventoryProviders
{
    public class GachaStallInventoryProvider : BEBehaviorAbstractContainer, IStallInventoryProvider
    {
        private GachaShopInventory _Inventory;
        public override InventoryBase Inventory => _Inventory;

        public int StallCount => _Inventory.StallSlots?.Length ?? 0;

        public GachaStallInventoryProvider(BlockEntity blockentity) : base(blockentity)
        {
            _Inventory = new GachaShopInventory(blockentity, blockentity.Api);
        }

        public override void Initialize(ICoreAPI api, JsonObject properties)
        {
            base.Initialize(api, properties);
            _Inventory.InitializeFromProperties(properties, "GachaShopInventory", this.Pos.ToString(), api);
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
            return null;
        }

        public ItemSlot GetDecorationSlot()
        {
            return null;
        }


    }
}
