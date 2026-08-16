using Commercially.Common.Inventory.Slots;
using Vintagestory.API.Common;

namespace Commercially.Vinconomy.Interfaces
{
    public interface IContainedStallSlot : IStallSlot
    {
        public AggregatedStacks ExtractProduct(int totalProductNeeded, CapacityAggregatedSlots containerSourceSlots, bool isAdminShop);
        public CapacityAggregatedSlots GetRequiredContainers(IPlayer player);
    }
}
