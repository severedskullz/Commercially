using Commercially.Common.Interfaces;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;

namespace Commercially.Common.Blocks.BlockEntities
{
    public class BECommercialBase : BlockEntity, IInteractableBlockEntity
    {
        public IOwnable Ownable { get; private set; }

        public IInteractionManager InteractionManager { get; private set; }

        public IGUIManager GUIManager { get; private set; }


        public override void Initialize(ICoreAPI api)
        {
            base.Initialize(api);

            // Loop through behaviors to find the correct behaviors. The system supports adding/removing different types, but we should only have 1 of each type. 
            for (int i = 0; i < Behaviors.Count; i++)
            {
                var behavior = Behaviors[i];
                if (behavior is IOwnable ownable)
                {
                    if (Ownable != null)
                    {
                        api.Logger.Warning("Multiple IOwnable behaviors found on block entity at {0} for block {1}. This may cause unexpected behavior.", Pos, this.Block);
                    }
                    Ownable = ownable;
                    continue;
                }

                if (behavior is IInteractionManager interactionManager)
                {
                    if (InteractionManager != null)
                    {
                        api.Logger.Warning("Multiple InteractionManager behaviors found on block entity at {0} for block {1}. This may cause unexpected behavior.", Pos, this.Block);
                    }
                    InteractionManager = interactionManager;
                    continue;
                }

                if (behavior is IGUIManager guiManager)
                {
                    if (GUIManager != null)
                    {
                        api.Logger.Warning("Multiple GuiManager behaviors found on block entity at {0} for block {1}. This may cause unexpected behavior.", Pos, this.Block);
                    }
                    GUIManager = guiManager;
                    continue;
                }

            }

        }

        public bool OnInteract(IWorldAccessor world, Caller caller, BlockSelection blockSel, string key = "default")
        {
            if (InteractionManager != null)
            {
                IInteraction interaction = InteractionManager.GetInteraction(key, caller, blockSel);
                if (interaction != null)
                {
                    return interaction.Interact(world, caller, this, blockSel);
                }
            }

            if (caller.Player != null && GUIManager != null)
            {
                return GUIManager.OpenGUI(this, caller, blockSel, key);
            }

            return false; // Interaction was not handled
        }

        public override void ToTreeAttributes(ITreeAttribute tree)
        {
            base.ToTreeAttributes(tree);
        }

        public override void FromTreeAttributes(ITreeAttribute tree, IWorldAccessor world)
        {
            base.FromTreeAttributes(tree, world);
        }

        public bool IsParent()
        {
            return Ownable is IOwnableRoot;
        }

        public bool IsChild()
        {
            return Ownable is IOwnableChild;
        }

        public bool IsChildBranch()
        {
            return Ownable is IOwnableNode;
        }

        public string GetOwnerName()
        {
            return Ownable != null ? Ownable?.OwnerName : null;
        }

        public string GetOwnableType()
        {
            return Ownable != null ? Ownable.OwnableType : null;
        }

        public bool IsAdminOwned()
        {
            return Ownable != null && Ownable.IsAdminOwned;
        }

        public string GetOwnerUID()
        {
            return Ownable != null ? Ownable.OwnerUID : null;
        }

        public override void OnBlockRemoved()
        {
            base.OnBlockRemoved();
        }

    }
}
