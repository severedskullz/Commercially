using Commercially.Common.Inventory;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;

namespace Commercially.Common.Blocks.BlockEntityBehaviors
{
    public class BEBehaviorGenericContainer : BEBehaviorAbstractContainer
    {

        public override InventoryBase Inventory => _Inventory;
        public ConfigurableInventory _Inventory;

        public BEBehaviorGenericContainer(BlockEntity blockentity) : base(blockentity)
        {
            _Inventory = new ConfigurableInventory(null);
        }

        public override void Initialize(ICoreAPI api, JsonObject properties)
        {
            base.Initialize(api, properties);
            _Inventory.Initialize(properties, "BEBehaviorGenericContainer", this.Pos.ToString(), api);
        }
    }
}
