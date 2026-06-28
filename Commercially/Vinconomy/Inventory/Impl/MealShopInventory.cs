using Vintagestory.API.Common;

namespace Commercially.Vinconomy.Inventory.Impl
{
    public class MealShopInventory : FillableShopInventory
    {
        public MealShopInventory(BlockEntity entity, ICoreAPI api) : base(entity, api)
        {
        }

        public override bool TransferToStall(int stallSlot, ItemSlot sourceSlot, int amount)
        {
            return true;
        }
        public override bool TransferFromStall(int stallSlot, ItemSlot destSlot, int amount)
        {
            return true;
        }
    }
}
