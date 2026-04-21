using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;

namespace Commercially.Common.GUI
{
    public abstract class ModularTab
    {
        public ICoreClientAPI API { get; set; }
        public abstract string Code { get; }
        public abstract string TabName { get; }
        public BlockEntity BlockEntity { get; set; }
        public GuiDialog Gui { get; set; }

        /// <summary>
        /// Initializes the tab with the provided GUI and block entity. The block entity may be null if the tab is not associated with a block entity.
        /// Perform any initialization logic here
        /// </summary>
        /// <param name="gui"></param>
        /// <param name="entity"></param>
        public virtual void Initialize(GuiDialog gui, BlockEntity entity = null)
        {
                Gui = gui;
                BlockEntity = entity;
                API = (entity?.Api as ICoreClientAPI);
        }

        /// <summary>
        /// Compose the tab's GUI elements. There must be atleast one rootBounds.WithChild() / rootBounds.WithChildren() call to add atleast one element for auto-resizing to occur. 
        /// Protip: Do NOT use *.WithParent(rootBounds), as it does not actually add it as a child element of rootBounds
        /// <param name="rootBounds">The main bounds within which the tab's elements should be composed. This should be the eventual parent of all child nodes created within this method.</param>
        /// </summary>
        public abstract void Compose(GuiComposer composer, ElementBounds rootBounds);
        public abstract void OnGuiClosed();
        public abstract void OnGuiOpened();
        public abstract bool IsVisible(GuiDialog gui);

        public virtual JsonObject GetConfiguration(string baseKey = ModularGUIModSystem.AttributeKey) 
        {
            return BlockEntity?.Block?.Attributes?[baseKey]?[Code];
        }

    }

}