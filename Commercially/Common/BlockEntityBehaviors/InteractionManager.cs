#nullable enable
using Commercially.Common.Interfaces;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;

namespace Commercially.Common.BlockEntityBehaviors
{
    public class InteractionManager : BlockEntityBehavior, IInteractionManager
    {
        //string ProviderType { get; }

        public InteractionManager(BlockEntity blockentity) : base(blockentity)
        {

        }

        public override void Initialize(ICoreAPI api, JsonObject properties)
        {
            base.Initialize(api, properties);
        }

        public IInteraction? GetInteraction(string key, Caller caller, BlockSelection blockSel) {

            if (blockSel == null)
            {
            }
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

    public class InteractionConfig
    {

    }
}
