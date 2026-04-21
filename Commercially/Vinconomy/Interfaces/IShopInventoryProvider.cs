using Vintagestory.API.Common;

namespace Commercially.Vinconomy.Interfaces
{
    public interface IShopInventoryProvider : ICurrencySinkProvider
    {
        public ItemSlot TradePass { get; } 
    }
}