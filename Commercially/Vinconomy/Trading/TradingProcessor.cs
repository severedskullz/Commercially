using Commercially.Common.Slots;
using Commercially.Common.Util;
using Commercially.Vinconomy.Interfaces;
using System;
using Vintagestory.API.Common;
using Vintagestory.GameContent;

namespace Commercially.Vinconomy.Trading
{
    public class TradingProcessor
    {

        public static bool CanFitPaymentIntoParent(TradeRequest request)
        {
            ICurrencySinkProvider currencySinkProvider = null;
            if (request.ParentEntity != null)
            {
                currencySinkProvider = request.ParentEntity.GetComponent<ICurrencySinkProvider>();
            }

            if (currencySinkProvider == null)
            {
                currencySinkProvider = request.SellingEntity.GetComponent<ICurrencySinkProvider>();
            }

            if (currencySinkProvider == null)
            {
                return false;
            }

            ItemSlot[] currencySlots = currencySinkProvider.CurrencySlots;
            int maxStackSize = request.CurrencyNeeded.Collectible.MaxStackSize;
            int qntyLeft = request.CurrencyNeeded.StackSize * request.NumPurchases;
            foreach (ItemSlot itemSlot in currencySlots)
            {
                if (itemSlot.Itemstack == null)
                {
                    qntyLeft -= maxStackSize;
                }
                else
                {
                    if (TradingUtil.IsMatchingItem(itemSlot.Itemstack, request.CurrencyNeeded, request.Api.World, false))
                    {
                        qntyLeft -= maxStackSize - itemSlot.StackSize;
                    }
                }

                if (qntyLeft <= 0)
                {
                    break;
                }
            }

            return (qntyLeft <= 0);
        }

        public static bool HasEnoughStock(TradeRequest request)
        {
            if (request.IsAdminShop) return true;
            return (request.ProductSourceSlots.TotalCount / request.GetFinalProductNeededPerPurchase()) > 0;
        }

        public static int GetNumTradesForStock(TradeRequest request)
        {
            if (request.IsAdminShop) return request.NumPurchases;
            return Math.Min(request.NumPurchases, request.ProductSourceSlots.TotalCount / request.GetFinalProductNeededPerPurchase());
        }

        public static bool CanPlayerAfford(TradeRequest request)
        {
            int currencyRequired = request.GetFinalCurrencyNeededPerPurchase();
            int totalCurrecny = request.CurrencySourceSlots.TotalCount;

            return totalCurrecny >= currencyRequired;

        }

        public static bool CanPlayerHold(TradeRequest request)
        {
            if (request.ToolSourceSlots == null)
            {
                return true;
            }
            else if (request.ToolSourceSlots is ServingCapacityAggregatedSlots servings)
            {
                return servings.TotalCapacity / request.GetFinalProductNeededPerPurchase() > 0;
            }
            else if (request.ToolSourceSlots is LiquidCapacityAggregatedSlots capacity)
            {
                float litersNeeded = ConvertStackToLiters(request.ProductNeeded, request.GetFinalProductNeededPerPurchase());
                return (int)(capacity.TotalCapacity / litersNeeded) > 0;
            }

            return true;
        }

        public static bool HasEnoughDurability(TradeRequest request)
        {
            if (request.ToolSourceSlots == null)
            {
                return true;
            }
            else if (request.ToolSourceSlots is DurabilityAggregatedSlots durability)
            {
                return durability.TotalDurability / request.GetFinalProductNeededPerPurchase() > 0;
            }
            /*
            else
            {
                return request.ToolSourceSlots.TotalCount / request.GetFinalProductNeededPerPurchase() > 0;
            }*/

            return true;
        }

        public static bool HasRequiredTradePass(TradeRequest req)
        {
            return !(req.TradePassNeeded != null && req.TradePassSourceSlots.TotalCount <= 0);
        }

        public static float ConvertStackToLiters(ItemStack stack, int amount)
        {
            WaterTightContainableProps contentProps = BlockLiquidContainerBase.GetContainableProps(stack);
            if (contentProps == null)
            {
                return 0;
            }
            return amount / contentProps.ItemsPerLitre;
        }
    }
}
