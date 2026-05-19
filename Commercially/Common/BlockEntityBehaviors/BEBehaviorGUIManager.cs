using Commercially.Common.BlockEntities;
using Commercially.Common.GUI;
using Commercially.Common.Interfaces;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;

namespace Commercially.Common.BlockEntityBehaviors
{
    public class BEBehaviorGUIManager : BlockEntityBehavior, IGUIManager
    {
        string[] Tabs;

        public BEBehaviorGUIManager(BlockEntity blockentity) : base(blockentity)
        {
        }

        public override void Initialize(ICoreAPI api, JsonObject properties)
        {
            base.Initialize(api, properties);
            Tabs = properties["tabs"]?.AsArray<string>();
        }

        public bool OpenGUI(BECommercialBase bECommercialBase, Caller caller, BlockSelection blockSel, string key)
        {
            if (Api.Side == EnumAppSide.Client)
            {
                GUIModularBlockEntity gui = new GUIModularBlockEntity("Dialogue", bECommercialBase);
                gui.LoadTabs(Tabs);
                gui.TryOpen();
            }
            return true;
        }

    }
}
