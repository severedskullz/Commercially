using System.Collections.Generic;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;

namespace Commercially.Common.BlockEntityBehaviors
{
    public class BEBehaviorOwnableLeaf : BEBehaviorOwnableReferenced, IOwnableLeaf
    {
        public BEBehaviorOwnableLeaf(BlockEntity blockentity) : base(blockentity)
        {
        }

        public long? ParentID { get; set; }

        public string ParentType {  get; set; }


        public override void Initialize(ICoreAPI api, JsonObject properties)
        {
            base.Initialize(api, properties);

        }

        public override void ToTreeAttributes(ITreeAttribute tree)
        {
            base.ToTreeAttributes(tree);
            if (ParentID.HasValue)
            {
                tree.SetLong("ParentID", ParentID.Value);
            }
            if (ParentType != null)
            {
                tree.SetString("ParentType", ParentType);
            }
        }

        public override void FromTreeAttributes(ITreeAttribute tree, IWorldAccessor world)
        {
            base.FromTreeAttributes(tree, world);
            ParentID = tree.TryGetLong("ParentID");
            ParentType = tree.GetString("ParentType");
        }

        public List<IOwnable> GetChildren()
        {
            throw new System.NotImplementedException();
        }

        public IOwnable GetParent()
        {
            throw new System.NotImplementedException();
        }

        public void SetOwnableRoot(IOwnableRoot root)
        {
            throw new System.NotImplementedException();
        }
    }
}
