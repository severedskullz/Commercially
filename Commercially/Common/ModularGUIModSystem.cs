using Commercially.Common.GUI;
using Commercially.Common.GUI.Tabs;
using System;
using System.Collections.Generic;
using Vintagestory.API.Client;
using Vintagestory.API.Common;

namespace Commercially.Common
{
    public class ModularGUIModSystem : ModSystem
    {
        public const string AttributeKey = "GUIConfig";

        Dictionary<string, Type> tabTypes = new Dictionary<string, Type>();

        //TODO: Not sure if I can even use this tbh. They dont have no-arg constructors so id have to make some sort of "init" method for all possible dialog types
        // Dont know enough about Reflection in C# to find the constructor from the assembly like we can do in Java
        Dictionary<string, Type> guiTypes = new Dictionary<string, Type>();

        private ICoreClientAPI Api;

        public override bool ShouldLoad(EnumAppSide forSide)
        {
            return forSide == EnumAppSide.Client;
        }

        public override void StartClientSide(ICoreClientAPI api)
        {
            base.StartClientSide(api);
            Api = api;

            RegisterGUIType(GUIModularBlockEntity.CODE, typeof(GUIModularBlockEntity));
            RegisterGUIType(GUIModularDialog.CODE, typeof(GUIModularDialog));

            RegisterTabType(GuiBlockEntityContainerTab.CODE, typeof(GuiBlockEntityContainerTab));
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
