using Vintagestory.API.Common;

namespace Commercially.Common.Inventory.Slots
{
    public class ToggledSlot : FilteredItemSlot
    {
        public ToggledSlot(InventoryBase inventory) : base(inventory)
        {
        }

        public override bool DrawUnavailable => !Enabled;

        private bool _Enabled = true;
        public bool Enabled { 
            get => _Enabled; 
            set { 
                _Enabled = value; 
                this.HexBackgroundColor = _Enabled ? null : "#FF0000";
            }
        
        }

    }
}
