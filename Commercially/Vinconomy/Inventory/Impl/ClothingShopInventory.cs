using System;
using System.Collections.Generic;
using System.Text;
using Vintagestory.API.Common;

namespace Commercially.Vinconomy.Inventory.Impl
{
    public class ClothingShopInventory : VinconBaseInventory
    {
        public ClothingShopInventory(BlockEntity entity, ICoreAPI api) : base(entity, api)
        {
        }

        public ClothingShopInventory(BlockEntity entity, string inventoryName, Type stallType, int numStalls, int slotsPerStall, ICoreAPI coreAPI) : base(entity, inventoryName, stallType, numStalls, slotsPerStall, coreAPI)
        {
        }
    }
}
