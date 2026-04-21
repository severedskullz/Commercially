using Commercially.Common;
using Commercially.Vinconomy.Inventory.StallSlots;
using Vintagestory.API.Common;

namespace Commercially.Vinconomy.Interfaces
{
    public interface IStallComponent : IComponent
    {
        public IOwnableLeaf Ownable { get; }
        public IStallInventoryProvider InventoryProvider { get; }

        public int StallCount { get; }
        public ItemStack GetCurrencyForStallSlot(int stallSlot);
        public ItemStack GetProductForStallSlot(int stallSlot);
        public int GetRemainingProductForStallSlot(int stallSlot);
        public StallSlotBase GetStallSlot(int stallSlot);
        public T GetStallSlot<T>(int stallSlot) where T : StallSlotBase;
        public bool TryPurchaseItem(IPlayer player, int stallSlot, int numPurchases);
    }
}