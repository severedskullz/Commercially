using Commercially.Common.BlockEntityBehaviors;
using Commercially.Common.GUI;
using Commercially.Common.GUI.Tabs;
using Commercially.Common.Interfaces;
using Commercially.Vinconomy.GUI.Tabs;
using System.Collections.Generic;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;
using Vintagestory.GameContent;

namespace Commercially.Common.BlockEntities
{
    public class BECommercialBase : BlockEntity
    {

        CommerciallyModSystem modSystem => Api.ModLoader.GetModSystem<CommerciallyModSystem>();

        public virtual string BlockEntityType => "commercialbase";

        public IOwnable Ownable { get; private set; }

        public InteractionManager InteractionManager { get; private set; }


        public override void Initialize(ICoreAPI api)
        {
            base.Initialize(api);

            // Loop through behaviors to find the correct behaviors. The system supports adding/removing different types, but we should only have 1 of each type. 
            for (int i = 0; i < Behaviors.Count; i++)
            {
                var behavior = Behaviors[i];
                if (behavior is IOwnable)
                {
                    if (Ownable != null)
                    {
                        api.Logger.Warning("Multiple IOwnable behaviors found on block entity at {0} for block {1}. This may cause unexpected behavior.", Pos, this.Block);
                    }
                    Ownable = (IOwnable)behavior;
                    continue;
                }

                //TODO: Should we have an interface for interaction managers?
                if (behavior is InteractionManager)
                {
                    if (InteractionManager != null)
                    {
                        api.Logger.Warning("Multiple InteractionManager behaviors found on block entity at {0} for block {1}. This may cause unexpected behavior.", Pos, this.Block);
                    }
                    InteractionManager = (InteractionManager)behavior;
                    continue;
                }

            }

        }

        public bool OnInteract(IWorldAccessor world, Caller caller, BlockSelection blockSel, string key = "default")
        {
            if (InteractionManager != null)
            {
                IInteraction interaction = InteractionManager.GetInteraction(key);
                if (interaction != null)
                {
                    return interaction.Interact(world, caller, blockSel);
                }
            }

            if (caller.Player != null)
            {
                BEBehaviorContainer container = this.GetBehavior<BEBehaviorContainer>();
                InventoryBase inv = container?.Inventory;
                if (inv != null)
                {
                    caller.Player.InventoryManager.OpenInventory(inv);
                }

                if (Api.Side == EnumAppSide.Client)
                {
                    GUIModularBlockEntity gui = new GUIModularBlockEntity("Dialogue", this);
                    gui.LoadTabs(new List<ModularTab>() { 
                        new GuiBlockEntityOwnershipTab(),
                        new GuiBlockEntityDebugTab(),
                        new GuiBlockEntityContainerTab(),
                        new GuiBlockEntityShopOwnerTab(),
                        new GuiBlockEntityShopCustomerTab()
                    });
                    gui.TryOpen();
                }


            }


            return true;
            //return false; // Interaction was not handled
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

        public virtual void UpdateOwnableEntry()
        {
            //modSystem.DB.UpdateOwnable(this.GetBehavior<IOwnableReference>());
        }

        public override void OnBlockRemoved()
        {


            base.OnBlockRemoved();
        }
    }
}
