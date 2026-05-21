using Commercially.Common.GUI;
using Commercially.Common.GUI.Tabs;
using System;
using System.Collections.Generic;
using Vintagestory.API.Common;

namespace Commercially.Common
{
    public class ModularGUIModSystem : ModSystem
    {
        public const string AttributeKey = "GUIConfig";

        Dictionary<string, Type> tabTypes = new Dictionary<string, Type>();
        Dictionary<string, Type> guiTypes = new Dictionary<string, Type>();

        private ICoreAPI Api;

        public override void Start(ICoreAPI api)
        {
            Api = api;

            RegisterGUIType(GUIModularBlockEntity.CODE, typeof(GUIModularBlockEntity));
            RegisterGUIType(GUIModularDialog.CODE, typeof(GUIModularDialog));

            RegisterTabType(GuiBlockEntityContainerTab.CODE, typeof(GuiBlockEntityContainerTab));
            RegisterTabType(GuiBlockEntityOwnershipTab.CODE, typeof(GuiBlockEntityOwnershipTab));
            RegisterTabType(GuiBlockEntityWaypointTab.CODE, typeof(GuiBlockEntityWaypointTab));
            RegisterTabType(GuiBlockEntityDebugTab.CODE, typeof(GuiBlockEntityDebugTab));
        }

        public void RegisterGUIType(string guiType, Type tab)
        {
            if (guiTypes.ContainsKey(guiType))
            {
                Api.Logger.Warning("GUI type {0} is already registered. Overwriting with new tab.", guiType);
            }
            guiTypes[guiType] = tab;
        }

        public Type GetGUIType(string guiType)
        {
            if (guiTypes.TryGetValue(guiType, out var tab))
            {
                return tab;
            }
            Api.Logger.Warning("GUI type {0} not found. Returning null.", guiType);
            return null;
        }

        public void RegisterTabType(string guiType, Type tab)
        {
            if (tabTypes.ContainsKey(guiType))
            {
                Api.Logger.Warning("GUI Tab Type {0} is already registered. Overwriting with new tab.", guiType);
            }
            tabTypes[guiType] = tab;
        }

        public Type GetTabType(string guiType)
        {
            if (tabTypes.TryGetValue(guiType, out var tab))
            {
                return tab;
            }
            Api.Logger.Warning("GUI Tab Type {0} not found. Returning null.", guiType);
            return null;
        }
    }
}
