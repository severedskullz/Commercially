using Vinconomy.Inventory.Slots;
using Vintagestory.API.Common;

namespace Commercially.Vinconomy.Inventory.Impl
{
    public class GenericShopInventory : VinconBaseInventory
    {
        public GenericShopInventory(BlockEntity entity, ICoreAPI api) : base(entity, api)
        {
        }

        public override void InitializeInternalSlots()
        {
            if (!IsInternalSlotsInitialized)
            {
                InternalSlots = new ItemSlot[1];
                InternalSlots[0] = new VinconDecoBlockSlot(this, 0);

                /*
                for (int i = 1; i < InternalSlots.Length; i++)
                {
                    InternalSlots[i] = new ItemSlot(this);
                }
                */
            }
        }

        public ItemSlot GetDecorationBlock()
        {
            return InternalSlots[0];
        }
    }
}
