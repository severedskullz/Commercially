using Commercially.Common.Blocks.BlockEntities;
using Commercially.Common.Interfaces;
using System;
using System.Collections.Generic;
using Vintagestory.API.Client;
using Vintagestory.API.Common;

namespace Commercially.Common.GUI
{
    public class GUIModularBlockEntity : GuiDialogBlockEntity, IModularGui
    {
        public const string CODE = "commercially:ModularBlockEntity";

        public List<ModularTab> Tabs = new List<ModularTab>();

        //TODO: This does nothing to prevent you from viewing hidden tabs. Need to store a map betwen the GuiTab[] and ModularTab list instead for this to work properly
        ModularTab ActiveTab {
            get {
                //Try and select the TabIndex'd tab first
                if (TabIndex >= 0 && TabIndex < Tabs.Count)
                    return Tabs[TabIndex];

                // Then fall back on the first tab if we still dont have a hit
                if (Tabs.Count > 0)
                    return Tabs[0];

                // Otherwise return nothing...
                return null;
            } 
        }
        public int TabIndex = 0;
        public int BlockSelectionIndex = 0;

        BECommercialBase BlockEntity;
        private ModularGUIModSystem GuiSystem;
        private readonly double TAB_WIDTH=200;
        private readonly double TAB_HEIGHT=300;

        public IOwnable Ownable => this.BlockEntity.Ownable;
        public IInteractionManager InteractionManager => this.BlockEntity.InteractionManager;
        public GuiComposer Composer => this.SingleComposer;


        public GUIModularBlockEntity(string dialogTitle, BECommercialBase blockEntity) : base(dialogTitle, blockEntity.Pos, (ICoreClientAPI)blockEntity.Api)
        {
            BlockEntity = blockEntity;
            GuiSystem = capi.ModLoader.GetModSystem<ModularGUIModSystem>();
        }

        public GUIModularBlockEntity(string dialogTitle, BECommercialBase blockEntity, InventoryBase inventory) : base(dialogTitle, inventory, blockEntity.Pos, (ICoreClientAPI)blockEntity.Api)
        {
            BlockEntity = blockEntity;
            GuiSystem = capi.ModLoader.GetModSystem<ModularGUIModSystem>();
        }

        public void LoadTabs(string[] tabs)
        {

            TabIndex = 0;
            List<ModularTab> newTabs = new List<ModularTab>(tabs.Length);
            foreach (var item in tabs)
            {
                Type type = GuiSystem.GetTabType(item);
                if (type == null) { 
                   capi.Logger.Error("Could not find tab with code {0} for block entity at {1}", item, BlockEntityPosition);
                    continue;
                }

                ModularTab instance = (ModularTab)Activator.CreateInstance(type);
                instance.Initialize(this, BlockEntity);
                newTabs.Add(instance);
            }
            Tabs = newTabs;
        }


        public override void OnGuiOpened()
        {
            base.OnGuiOpened();
            Compose();
        }

        public override void OnGuiClosed()
        {
            base.OnGuiClosed();
        }

        private void Compose()
        {
            try
            {
                GuiTab[] newTabs = GetGUITabs();
                double tabHeight = GuiElement.scaled(25) * newTabs.Length;

                ElementBounds dialogBounds = ElementStdBounds.AutosizedMainDialog.WithAlignment(EnumDialogArea.CenterMiddle);
                ElementBounds tabBounds = ElementBounds.FixedSize(TAB_WIDTH, System.Math.Min(TAB_HEIGHT, tabHeight)).FixedLeftOf(dialogBounds).WithFixedOffset(0, GuiStyle.TitleBarHeight);
                ElementBounds bgBounds = ElementBounds.Fill.WithFixedPadding(GuiStyle.ElementToDialogPadding);
                bgBounds.BothSizing = ElementSizing.FitToChildren;
           
                SingleComposer = capi.Gui.CreateCompo("GUI-" + BlockEntityPosition, dialogBounds)
                    .AddDialogTitleBar(DialogTitle, OnTitleBarCloseClicked)
                    .AddVerticalTabs(newTabs, tabBounds, OnTabChanged, "tabs")
                    .AddShadedDialogBG(bgBounds);

                if (ActiveTab != null)
                {
                    ActiveTab.Compose(SingleComposer, bgBounds);
                }

                // Fix the first tab getting set to Active by default
                if (newTabs.Length > 0)
                    newTabs[0].Active = TabIndex == 0;

                SingleComposer.Compose();
            } catch (Exception e)
            {
                capi.Logger.Error("Error composing GUI for block entity at {0}. {1}", BlockEntityPosition, e);
                capi.ShowChatMessage("Error composing GUI for block entity - Nothing will be shown to prevent game crash. See log for further details");
            }

        }

        public void FullRecompose()
        {
            SingleComposer.Clear(new ElementBounds());
            SingleComposer?.Dispose();
            Compose();
        }

        private void OnTitleBarCloseClicked()
        {
            TryClose();
        }

        private void OnTabChanged(int arg1, GuiTab tab)
        {
            this.TabIndex = tab.DataInt;
            FullRecompose();
        }

        public GuiTab[] GetGUITabs()
        {
            List<GuiTab> guiTabs = new List<GuiTab>(Tabs.Count);
            int i = 0;
            foreach (var tab in Tabs)
            {
                if (tab.IsVisible(this))
                {
                    guiTabs.Add(new GuiTab() { Name = tab.TabName, Active = tab == ActiveTab, DataInt = i });
                }
                
                i++;
            }
            return guiTabs.ToArray();
        }

        /// <summary>
        /// Sends an arbitrary packet. Only really initended to be used for Inventory packets, since Tyron is yet again inconsistent with his methods.
        /// </summary>
        /// <param name="p"></param>
        public void SendPacket(object p)
        {
            capi.Network.SendBlockEntityPacket(BlockEntityPosition.X, BlockEntityPosition.Y, BlockEntityPosition.Z, p);
        }

        public void SendPacket(int packetId, byte[] p)
        {
            capi.Network.SendBlockEntityPacket(BlockEntityPosition, packetId, p);
        }

        public void SendPacket(int packetId, object p)
        {
            capi.Network.SendBlockEntityPacket(BlockEntityPosition, packetId, p);
        }

        public void SetActiveTab(string activeTab)
        {
            if (string.IsNullOrEmpty(activeTab)) return;

            int i = 0;
            foreach (var tab in Tabs)
            {
                if (tab.Code == activeTab)
                {
                    TabIndex = i;
                    return;
                }
                i++;
            }
            capi.Logger.Error("Couldn't find tab {0} while trying to set Active Tab for block entity at {1}. Defaulting to first tab.", activeTab, BlockEntityPosition);
            TabIndex = 0;
        }
    }
}
