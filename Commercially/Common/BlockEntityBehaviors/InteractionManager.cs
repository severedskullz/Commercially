#nullable enable
using Commercially.Common.Interfaces;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;

namespace Commercially.Common.BlockEntityBehaviors
{
    public class InteractionManager : BlockEntityBehavior
    {
        //string ProviderType { get; }

        public InteractionManager(BlockEntity blockentity) : base(blockentity)
        {

        }

        public IInteraction? GetInteraction(string key) {
            return null;
        }

        public virtual bool Interact(IWorldAccessor world, Caller caller, BlockSelection blockSel, string key = "default", ITreeAttribute? activationArgs = null) {

            return true;
        }


        public override void FromTreeAttributes(ITreeAttribute tree, IWorldAccessor world)
        {
            base.FromTreeAttributes(tree, world);
            var OwnerUID = tree.GetString("OwnerUID");
            var OwnerName = tree.GetString("OwnerName");

        }

    }
}
