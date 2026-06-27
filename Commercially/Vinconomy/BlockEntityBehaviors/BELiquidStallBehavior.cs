using Commercially.Common;
using Commercially.Common.Interfaces;
using Commercially.Common.Slots;
using Commercially.Common.Util;
using Commercially.Vinconomy.Interfaces;
using Commercially.Vinconomy.Inventory.Impl;
using Commercially.Vinconomy.Inventory.StallSlots;
using Commercially.Vinconomy.Trading;
using Commercially.Vinconomy.Trading.Processor;
using Vintagestory.API.Common;
using Vintagestory.API.Config;

namespace Commercially.Vinconomy.BlockEntityBehaviors
{
    public class BELiquidStallBehavior : BEStallBehavior
    {
        public BELiquidStallBehavior(BlockEntity blockentity) : base(blockentity)
        {
        }


        public override TradeResult PurchaseItem(IPlayer player, int stallSlot, int numPurchases, IShopComponent shopRegister)
        {
            TradeRequest request = new TradeRequest(Api, player);
            IOwnable ownable = this.GetComponent<IOwnable>();
            ItemStack currencyStack = GetCurrencyForStallSlot(stallSlot);
            ItemStack productStack = GetProductForStallSlot(stallSlot);
            request.WithShop(shopRegister, this, stallSlot, ownable?.IsAdminOwned ?? false);
            request.WithPurchases(numPurchases);
            request.WithCurrency(currencyStack, TradingUtil.GetAllValidSlotsFor(player, currencyStack), currencyStack.StackSize);
            request.WithProduct(productStack, GetStallSlot(stallSlot).GetProducts(), productStack.StackSize);

            AggregatedSlots coupons = TradingUtil.GetCouponsSlotsFor(player, request.ProductNeeded, shopRegister);
            if (coupons.Slots.Count > 0)
            {
                request.WithCoupons(coupons.Slots[0]);
            }

            request.WithContainers(GetRequiredTools(player, stallSlot));


            if (shopRegister != null)
            {
                RegisterInventory inv = shopRegister.GetComponent<IInventoryProvider>()?.Inventory as RegisterInventory;
                if (inv != null)
                {
                    ItemStack tradePass = inv.GetTradePass();
                    if (tradePass != null)
                    {
                        request.WithTradePass(tradePass, TradingUtil.GetAllValidSlotsFor(player, tradePass));
                    }
                }
            }


            request.Build();

            TradeResult result = VinconomyCore.TryPurchaseItem(request);
            if (result.ErrorMsg != null)
            {
                CommerciallyModSystem.PrintClientMessage(player, result.ErrorMsg);
            }
            else
            {
                Blockentity.MarkDirty(true, null);
                //Blockentity.UpdateMeshes();
            }

            return result;
        }

        public LiquidCapacityAggregatedSlots GetRequiredTools(IPlayer player, int stallSlot)
        {
            ItemStack desiredStack = GetStallSlot<LiquidStallSlot>(stallSlot).FindFirstNonEmptyStockSlot()?.Itemstack;
            LiquidCapacityAggregatedSlots aggregatedSlots = new LiquidCapacityAggregatedSlots(Api);

            ItemSlot handItem = player.InventoryManager.ActiveHotbarSlot;
            if (LiquidTradingProcessor.CanHoldLiquid(Api.World, handItem.Itemstack, desiredStack))
            {
                aggregatedSlots.Add(handItem);
            }

            IInventory hotbarInv = player.InventoryManager.GetHotbarInventory();
            foreach (ItemSlot itemSlot in hotbarInv)
            {
                if (handItem == itemSlot || itemSlot.Itemstack == null) { continue; }
                if (LiquidTradingProcessor.CanHoldLiquid(Api.World, itemSlot.Itemstack, desiredStack))
                {
                    aggregatedSlots.Add(itemSlot);
                }
            }

            IInventory characterInv = player.InventoryManager.GetOwnInventory(GlobalConstants.backpackInvClassName);
            foreach (ItemSlot itemSlot in characterInv)
            {
                if (handItem == itemSlot) { continue; }
                if (LiquidTradingProcessor.CanHoldLiquid(Api.World, itemSlot.Itemstack, desiredStack))
                {
                    aggregatedSlots.Add(itemSlot);
                }
            }
            return aggregatedSlots;
        }

    }
}
