using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;

namespace Commercially.Common.Inventory
{
    /// <summary>
    /// A Mechanism primarily meant for allowing an Inventory to be initialized with the base constructor InventoryBase("-", null) and still be ready to use after an Initialization call
    /// </summary>
    public interface ILateInitInventory
    {
        /// <summary>
        /// Returns whether or not the Slots for the given inventory are initialized. This should be used in conjunction with the Initialize(int, string, string, ICoreApi) method in order
        /// to correctly reconfigure the inventory even after it is initialized for the first time or reloading through its BlockEntity
        /// </summary>
        public bool IsSlotsInitialized { get; }

        /// <summary>
        /// Completely ensures that the Inventory is initialized and ready for use. Should ensure that Slots is not null, contains at least the number of elements in the array as numSlots, adding
        /// new ones if initizlied with fewer either from the constructor or through FromTreeAttributes after the JSON properties have been changed, sets the Inventory Network Util
        /// on the Inventory if it is still null, and finally calling AfterBlocksLoaded() to resolve the block/item IDs
        /// </summary>
        /// <param name="properties"></param>
        /// <param name="className"></param>
        /// <param name="instanceID"></param>
        /// <param name="api"></param>
        public void Initialize(JsonObject properties, string className, string instanceID, ICoreAPI api);
    }
}