using Commercially.Common.Util;
using Vintagestory.API.Common;

namespace Commercially.Common.Interfaces
{
    public interface IInventoryProvider : IBlockEntityComponent
    {
        public InventoryBase Inventory { get; }
    }
}
