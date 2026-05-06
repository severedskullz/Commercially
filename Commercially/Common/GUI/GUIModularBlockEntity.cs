using Commercially.Common.BlockEntities;
using Commercially.Common.BlockEntityBehaviors;
using Commercially.Common.Interfaces;
using System;
using System.Collections.Generic;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Server;

namespace Commercially.Common.GUI
{
    public class GUIModularBlockEntity : GuiDialogBlockEntity, IModularGui
    {
        public const string CODE = "commercially:ModularBlockEntity";

        List<ModularTab> Tabs = new List<ModularTab>();

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

        BECommercialBase BlockEntity;
        private readonly double TAB_WIDTH=200;
        private readonly double TAB_HEIGHT=300;

        public IOwnable Ownable => this.BlockEntity.Ownable;
        public InteractionManager InteractionManager => this.BlockEntity.InteractionManager;
        public GuiComposer Composer => this.SingleComposer;


        public GUIModularBlockEntity(string dialogTitle, BECommercialBase blockEntity) : base(dialogTitle, blockEntity.Pos, (ICoreClientAPI)blockEntity.Api)
        {
            BlockEntity = blockEntity;
        }

        public GUIModularBlockEntity(string dialogTitle, BECommercialBase blockEntity, InventoryBase inventory) : base(dialogTitle, inventory, blockEntity.Pos, (ICoreClientAPI)blockEntity.Api)
        {
        }

        public void LoadTabs(List<ModularTab> tabs)
        {
            Tabs = tabs;
            TabIndex = 0;

            foreach (var item in tabs)
            {
                item.Initialize(this, BlockEntity);
            }
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
                GuiTab[] newTabs = GetTabs();
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

        public GuiTab[] GetTabs()
        {
            List<GuiTab> guiTabs = new List<GuiTab>();
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


    }
}
