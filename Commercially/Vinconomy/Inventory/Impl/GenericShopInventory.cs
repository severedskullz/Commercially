using Vinconomy.Inventory.Slots;
using Vintagestory.API.Common;

namespace Commercially.Vinconomy.Inventory.Impl
{
    public class GenericShopInventory : VinconBaseInventory
    {
        public GenericShopInventory(ICoreAPI api) : base(api)
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

        /*
        public override void FromTreeAttributes(ITreeAttribute tree)
        {
            base.FromTreeAttributes(tree);
            InternalSlots[0].Itemstack = tree.GetItemstack("decoration");
        }


        public override void ToTreeAttributes(ITreeAttribute tree)
        {
            base.ToTreeAttributes(tree);
            tree.SetItemstack("decoration", InternalSlots[0].Itemstack);
        }
        */
    }
}
