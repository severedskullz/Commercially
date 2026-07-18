using Commercially.Vinconomy.Inventory.StallSlots;
using System;
using System.Collections.Generic;
using System.Text;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;

namespace Commercially.Vinconomy.Inventory.Impl
{
    public class TellerInventory : VinconBaseInventory
    {
        public TellerInventory(BlockEntity entity, ICoreAPI api) : base(entity, api)
        {
        }

        public TellerInventory(string inventoryName, Type stallType, int numStalls, int slotsPerStall, ICoreAPI coreAPI) : base(inventoryName, stallType, numStalls, slotsPerStall, coreAPI)
        {
        }
    }
}
