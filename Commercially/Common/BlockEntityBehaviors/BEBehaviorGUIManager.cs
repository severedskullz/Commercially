using Commercially.Common.BlockEntities;
using Commercially.Common.GUI;
using Commercially.Common.Interfaces;
using Commercially.Common.Networking.Packets;
using System;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;
using Vintagestory.API.Server;
using Vintagestory.API.Util;

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
            if (Api.Side == EnumAppSide.Server)
            {
                GUITabsPacket packet = new GUITabsPacket();
                ModularGUIModSystem modSystem = Api.ModLoader.GetModSystem<ModularGUIModSystem>();
               

                foreach (var item in Tabs)
                {
                    Type type = modSystem.GetTabType(item);

                    if (type == null)
                    {
                        modSystem.Mod.Logger.Error($"No tab type found for {item} in GUI {Blockentity.Block.Code} at {Blockentity.Pos}. Skipping tab.");
                        continue;
                    }
                        

                    ModularTab instance = (ModularTab)Activator.CreateInstance(type);
                    byte[] data = instance.OnSendData(bECommercialBase);
                    if (data?.Length > 0) {
                        packet.AddTab(item, data);
                    }
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

                if (inventory != null)
                {
                    gui = new GUIModularBlockEntity("Dialogue", this.Blockentity as BECommercialBase, inventory);
                } else { 
                    gui = new GUIModularBlockEntity("Dialogue", this.Blockentity as BECommercialBase);
                }
                gui.LoadTabs(Tabs);
                if (packet != null)
                {
                    foreach (var tab in gui.Tabs)
                    {
                        tab.OnRecievedData(packet.GetData(tab.TabName));
                    }
                }
                gui.TryOpen();
            }
        }


    }
}
