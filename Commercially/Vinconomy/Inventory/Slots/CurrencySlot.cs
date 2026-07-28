using Commercially.Common.Inventory.Slots;
using Vintagestory.API.Common;

namespace Vinconomy.Inventory.Slots
{
    public class CurrencySlot : VinconCloningSlot
    {
        
        public CurrencySlot(InventoryBase inventory) : base(inventory)
        {
            //this.HexBackgroundColor = "#B62521";
            this.BackgroundIcon = "commercially-payment";
        }
    }

}
