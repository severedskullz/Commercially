using Commercially.Common.Interfaces;
using Commercially.Common.Registry;
using System;
using System.IO;
using System.Linq;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;

namespace Commercially.Common.GUI.Tabs
{
    internal class GuiBlockEntityWaypointTab : ModularTab
    {
        CommerciallyModSystem modSystem;
        OwnableRegistration registration;
        string[] icons;
        int[] colors;
        private int currentColor;
        private string currentIcon;
        private bool isVisible;

        public const string CODE = "Commercially.Waypoint";
        public override string Code => CODE;
        public override string TabName => Lang.Get("commercially:tabname-waypoint");

        public override void Initialize(IModularGui gui, BlockEntity entity = null)
        {
            base.Initialize(gui, entity);
            modSystem = ClientApi.ModLoader.GetModSystem<CommerciallyModSystem>();
            icons = modSystem.OwnableMapLayer.WaypointIcons.Keys.ToArray();
            colors = modSystem.OwnableMapLayer.WaypointColors.ToArray();

            long id = entity.GetBehavior<IOwnableReference>()?.ID ?? -1;
            registration = modSystem.OwnableRegistry.GetOwnable(id);
            if (registration == null)
            {
                //throw new Exception("BlockEntity does not have an OwnableRegistration");
            }
        }

        public override void Compose(GuiComposer composer, ElementBounds rootBounds)
        {
            ElementBounds waypointLabel = ElementBounds.Fixed(0.0, 28.0, 300.0, 25.0);
            ElementBounds waypointVisible = waypointLabel.RightCopy(0, -5);

            ElementBounds colorLabelBounds = ElementBounds.FixedSize(500, 25.0).FixedUnder(waypointLabel).WithFixedOffset(0, 10);
            ElementBounds colorRow = ElementBounds.FixedSize(25, 25).FixedUnder(colorLabelBounds);

            ElementBounds iconLabelBounds = ElementBounds.Fixed(0.0, 220.0, 500, 25.0);
            ElementBounds iconRow = ElementBounds.FixedSize(25, 25.0).FixedUnder(iconLabelBounds);

            rootBounds.WithChildren(waypointLabel,waypointVisible,colorRow,iconRow,colorLabelBounds,iconLabelBounds);

            composer.AddStaticText(Lang.Get("commercially:gui-waypoint-visible"), CairoFont.WhiteSmallText(), waypointLabel)
                .AddHoverText(Lang.Get("commercially:tooltip-waypoint-visible"), CairoFont.WhiteDetailText(), 500, waypointLabel)
                .AddSwitch(new Action<bool>(OnToggleWaypointVisible), waypointVisible, "isvisible")
                .AddStaticText(Lang.Get("commercially:gui-color"), CairoFont.WhiteSmallText(), colorLabelBounds)
                .AddColorListPicker(colors, OnToggleColor, colorRow, 500, "colorpicker")
                .AddStaticText(Lang.Get("commercially:gui-icon"), CairoFont.WhiteSmallText(), iconLabelBounds)
                .AddIconListPicker(icons, OnToggleIcon, iconRow, 500, "iconpicker");

            if (registration?.BroadcastWaypoint ?? false)
            {
                //GuiComposerHelpers.GetButton(base.SingleComposer, "saveButton").Enabled = false;
                GuiComposerHelpers.ColorListPickerSetValue(composer, "colorpicker", colors.IndexOf(registration.WaypointColor));
                this.currentColor = registration.WaypointColor;
                GuiComposerHelpers.IconListPickerSetValue(composer, "iconpicker", icons.IndexOf(registration.WaypointIcon));
                this.currentIcon = registration.WaypointIcon;
                composer.GetSwitch("isvisible").On = true;
                this.isVisible = true;
            }
            else
            {
                GuiComposerHelpers.ColorListPickerSetValue(composer, "colorpicker", 0);
                this.currentColor = colors[0];
                GuiComposerHelpers.IconListPickerSetValue(composer, "iconpicker", 0);
                this.currentIcon = icons[0];
            }
        }

        private void OnToggleWaypointVisible(bool visible)
        {
            if (!Gui.Composer.Composed) return;

            this.isVisible = visible;
            SendWaypointUpdate();
        }

        private void OnToggleIcon(int index)
        {
            if (!Gui.Composer.Composed) return;

            this.currentIcon = icons[index];
            SendWaypointUpdate();
        }

        private void OnToggleColor(int index)
        {
            if (!Gui.Composer.Composed) return;

            this.currentColor = colors[index];
            SendWaypointUpdate();
        }

        private void SendWaypointUpdate()
        {
            byte[] data;
            using (MemoryStream ms = new MemoryStream())
            {
                BinaryWriter writer = new BinaryWriter(ms);
                writer.Write(isVisible);
                writer.Write(currentIcon);
                writer.Write(currentColor);
                data = ms.ToArray();
            }

            ClientApi.Network.SendBlockEntityPacket(this.BlockEntityPosition, CommerciallyConstants.SET_WAYPOINT, data);
        }

        public override bool IsVisible()
        {
            return true;
        }

        public override void OnGuiClosed()
        {

        }

        public override void OnGuiOpened()
        {

        }

        public override void OnRecievedData(byte[] data)
        {

        }

        public override byte[] OnSendData(BlockEntity entity, Caller caller, BlockSelection blockSel, string key)
        {
            return null;
        }
    }
}
