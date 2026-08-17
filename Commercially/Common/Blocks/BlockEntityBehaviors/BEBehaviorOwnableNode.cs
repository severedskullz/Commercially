using Commercially.Common.Interfaces;
using Commercially.Common.Registry;
using System.Collections.Generic;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;

namespace Commercially.Common.Blocks.BlockEntityBehaviors
{
    public class BEBehaviorOwnableNode : BEBehaviorOwnableReferenced, IOwnableNode
    {
        public BEBehaviorOwnableNode(BlockEntity blockentity) : base(blockentity)
        {
        }

        public long? ParentID { get; set; }

        public string ParentType {  get; set; }

        public string[] AllowedTypes { get; private set; }

        public override void Initialize(ICoreAPI api, JsonObject properties)
        {
            base.Initialize(api, properties);
            AllowedTypes = properties["allowedTypes"].AsArray<string>(["Generic"]);
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
            if (ParentID.HasValue)
            {
                OwnableRegistration reg = Api.ModLoader.GetModSystem<CommerciallyModSystem>().OwnableRegistry.GetOwnable(ParentID.Value);
                return Api.World.BlockAccessor.GetBlockEntity(reg.Position)?.GetBehavior<IOwnable>();
            }

            return null;
        }

        public void SetOwnableRoot(IOwnableRoot root)
        {
            throw new System.NotImplementedException();
        }

        public void SetParent(IOwnableRoot root)
        {
            this.ParentID = root.ID;
        }

        public void SetParent(long parentId)
        {
            this.ParentID = parentId;
        }

        public string[] GetAllowedParentTypes()
        {
            return AllowedTypes;
        }
    }
}
