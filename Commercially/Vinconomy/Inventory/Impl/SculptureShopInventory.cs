using Commercially.Vinconomy.Interfaces;
using Commercially.Vinconomy.Inventory.StallSlots;
using System;
using Vintagestory.API.Common;

namespace Commercially.Vinconomy.Inventory.Impl
{
    internal class SculptureShopInventory : VinconBaseInventory
    {
        public SculptureShopInventory(BlockEntity entity, ICoreAPI api) : base(entity, api)
        {
        }

        public override void OnStockModified(ItemSlot slot)
        {
            if (Api.Side == EnumAppSide.Client) return;

            if (slot is IStallProductStockSlot stallProductSlot)
            {
                int stallSlot = stallProductSlot.GetStall();
                StallSlotBase stall = this.GetStall(stallSlot);
                ItemStack product = stall.Product?.Itemstack?.Clone();
                ItemStack currency = stall.Currency?.Itemstack?.Clone();
                int stockCount = stall.GetProducts().TotalCount;

                UpdateStockForSlot(StallComponent, stallSlot, product, stockCount, currency);
            }
        }
    }
}
