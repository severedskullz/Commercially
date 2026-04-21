using Commercially.Common.Interfaces;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;

namespace Commercially.Common.BlockEntityBehaviors
{
    public class BEBehaviorOwnableReferenced : BEBehaviorOwnable, IOwnableReference, IPersistableStackAttributes
    {
        public long ID { get; private set; }

        public BlockPos Position => this.Pos;

        public BEBehaviorOwnableReferenced(BlockEntity blockentity) : base(blockentity)
        {
        }

        public override void Initialize(ICoreAPI api, JsonObject properties)
        {
            base.Initialize(api, properties);

        }

        public override void ToTreeAttributes(ITreeAttribute tree)
        {
            base.ToTreeAttributes(tree);
            tree.SetLong("ID", ID);
        }

        public override void FromTreeAttributes(ITreeAttribute tree, IWorldAccessor world)
        {
            base.FromTreeAttributes(tree, world);
            ID = tree.GetLong("ID");
        }

        
        public override void OnBlockPlaced(ItemStack byItemStack)
        {
            base.OnBlockPlaced(byItemStack);
            CommerciallyModSystem modSystem = Api.ModLoader.GetModSystem<CommerciallyModSystem>();

            // These attributes were set on the itemstack in BehaviorCommercialEvents:DoPlaceBlock
            OwnerName = byItemStack.Attributes.GetString("OwnerName");
            OwnerUID = byItemStack.Attributes.GetString("OwnerUID");

            if (byItemStack.Attributes.HasAttribute("ID"))
            {
                ID = byItemStack.Attributes.GetLong("ID");
                modSystem.UpdateOwnable(this);

            } else
            {
                modSystem.AddOwnable(this);
            }
        }
        

        public override void OnBlockRemoved()
        {
            CommerciallyModSystem modSystem = Api.ModLoader.GetModSystem<CommerciallyModSystem>();
            modSystem.RemoveOwnable(this);
            base.OnBlockRemoved();
        }

        public void SetIDInternal(long newId)
        {
            ID = newId;
        }


        public virtual void AddAttributes(ItemStack stack)
        {
            stack.Attributes.SetLong("ID", ID);
            stack.Attributes.SetString("OwnerUID", OwnerUID);
            stack.Attributes.SetString("OwnerName", OwnerName);
        }
    }
}
