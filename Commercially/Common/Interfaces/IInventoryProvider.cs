using Commercially.Common.Util;
using Vintagestory.API.Common;

namespace Commercially.Common.Interfaces
{
    public interface IInventoryProvider : IComponent
    {
        public InventoryBase Inventory { get; }
    }
}
