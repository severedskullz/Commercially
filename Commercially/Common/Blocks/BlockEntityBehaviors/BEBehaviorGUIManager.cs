using Commercially.Common.Blocks.BlockEntities;
using Commercially.Common.GUI;
using Commercially.Common.Interfaces;
using Commercially.Common.Networking.Packets;
using System;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Datastructures;
using Vintagestory.API.Server;
using Vintagestory.API.Util;

namespace Commercially.Common.Blocks.BlockEntityBehaviors
{
    public class BEBehaviorGUIManager : BlockEntityBehavior, IGUIManager
    {
        string[] Tabs;
        string DialogueLangCode;
        string DialogueName;
        GUIModularBlockEntity Gui = null;

        public BEBehaviorGUIManager(BlockEntity blockentity) : base(blockentity)
        {
        }

        public override void Initialize(ICoreAPI api, JsonObject properties)
        {
            base.Initialize(api, properties);
            Tabs = properties["tabs"]?.AsArray<string>();
            DialogueLangCode = properties["langCode"]?.AsString();
            DialogueName = properties["name"]?.AsString();
        }

        public bool OpenGUI(BECommercialBase bECommercialBase, Caller caller, BlockSelection blockSel, string key, string defaultTab = null)
        {
            if (Api.Side == EnumAppSide.Server)
            {
                GUITabsPacket packet = new GUITabsPacket();
                ModularGUIModSystem modSystem = Api.ModLoader.GetModSystem<ModularGUIModSystem>();
               
                packet.SelectedIndex = blockSel?.SelectionBoxIndex ?? 0;
                packet.DefaultTab = defaultTab;
                packet.DialogueName = DialogueName;
                packet.DialogueLangCode = DialogueLangCode;

                foreach (var item in Tabs)
                {
                    Type type = modSystem.GetTabType(item);

                    if (type == null)
                    {
                        modSystem.Mod.Logger.Error($"No tab type found for {item} in GUI {Blockentity.Block.Code} at {Blockentity.Pos}. Skipping tab.");
                        continue;
                    }
                        

                    ModularTab instance = (ModularTab)Activator.CreateInstance(type);
                    byte[] data = instance.OnSendData(bECommercialBase, caller, blockSel, key);
                    if (data?.Length > 0) {
                        packet.AddTab(item, data);
                    }
                }
                InventoryBase inventory = Blockentity.GetBehavior<IInventoryProvider>()?.Inventory;
                if (inventory != null)
                {
                    caller.Player.InventoryManager.OpenInventory(inventory);
                }

                (Api as ICoreServerAPI).Network.SendBlockEntityPacket((IServerPlayer)caller.Player, Blockentity.Pos, CommerciallyConstants.OPEN_GUI, SerializerUtil.Serialize(packet));
            }
            return true;
        }

        public override void OnReceivedServerPacket(int packetid, byte[] data)
        {
            if (packetid == CommerciallyConstants.OPEN_GUI)
            {
                GUITabsPacket packet = null;
                if (data?.Length > 0)
                {
                    packet = SerializerUtil.Deserialize<GUITabsPacket>(data);
                }
                InventoryBase inventory = Blockentity.GetBehavior<IInventoryProvider>()?.Inventory;
                GUIModularBlockEntity gui = null;

                BECommercialBase commercialBase = this.Blockentity as BECommercialBase;
                string dialogueName;
                if (string.IsNullOrEmpty(packet?.DialogueLangCode))
                {
                    dialogueName = packet?.DialogueName ?? "Dialogue";
                }
                else
                {
                    dialogueName = Lang.Get(packet.DialogueLangCode);
                }

                if (inventory != null)
                {
                    gui = new GUIModularBlockEntity(dialogueName, commercialBase, inventory);
                } else { 
                    gui = new GUIModularBlockEntity(dialogueName, commercialBase);
                }
                gui.BlockSelectionIndex = packet?.SelectedIndex ?? 0;
                gui.LoadTabs(Tabs);
                gui.OnClosed += UnsetGUI;

                if (packet != null)
                {
                    if (packet.DefaultTab != null)
                    {
                        gui.SetActiveTab(packet.DefaultTab);
                    }

                    foreach (var tab in gui.Tabs)
                    {
                        tab.OnRecievedData(packet.GetData(tab.TabName));
                    }
                }
                gui.TryOpen();
            }
            else if (packetid == CommerciallyConstants.TAB_UPDATE)
            {
                if (Gui == null)
                {
                    Api.Logger.Error($"Received tab update packet for GUI at {Blockentity.Pos} but GUI is not open.");
                    return;
                }

                GUITabPacket tabPacket = SerializerUtil.Deserialize<GUITabPacket>(data);
                ModularTab tab = GetTab(tabPacket.TabCode);
                tab?.OnRecievedData(tabPacket.Data);
            }
            else if (packetid == CommerciallyConstants.GUI_UPDATE)
            {
                if (Gui == null)
                {
                    Api.Logger.Error($"Received GUI update packet for GUI at {Blockentity.Pos} but GUI is not open.");
                    return;
                }

                Gui.FullRecompose();
            }

        }

        private void UnsetGUI()
        {
            Gui = null;
        }

        private ModularTab GetTab(string tabCode)
        {
            foreach (var tab in Gui.Tabs)
            {
                if (tab.Code == tabCode)
                    return tab;
            }
            return null;
        }
    }
}
