using Commercially.Common.Interfaces;
using System;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Datastructures;

namespace Commercially.Common.GUI.Tabs
{
    public class GuiBlockEntityOwnershipTab : ModularTab
    {
        public const string CODE = "commercially.Ownership";
        public override string Code => CODE;
        public override string TabName => Lang.Get("commercially:tabname-ownership");

        IOwnable Ownable;
        private string[] ParentOwnableTypes;
        private string ParentOwnableTypesText;
        private string NameText;

        private string[] ParentIDs;
        private string[] ParentNames;
        private int SelectedIndex;

        public override void Compose(GuiComposer composer, ElementBounds rootBounds)
        {
            // Name
            ElementBounds nameLabelBounds = ElementBounds.FixedSize(300, 25).WithFixedOffset(0, GuiStyle.TitleBarHeight);
            ElementBounds nameInputBounds = ElementBounds.FixedSize(300, 25).FixedUnder(nameLabelBounds);
            rootBounds.WithChildren(nameLabelBounds,nameInputBounds);

            // Owner
            ElementBounds parentSelectionLabel = ElementBounds.FixedSize(300, 25).FixedUnder(nameInputBounds,10);
            ElementBounds parentSelectBounds = parentSelectionLabel.BelowCopy().WithFixedWidth(250);
            rootBounds.WithChildren(parentSelectionLabel, parentSelectBounds);
            // Permissions

            // Admin Ownership Toggle
            ElementBounds adminShopLabel = ElementBounds.FixedSize(250, 25).FixedUnder(parentSelectBounds).WithFixedOffset(0, 15);
            ElementBounds adminShopBounds = ElementBounds.FixedSize(40, 40).FixedUnder(parentSelectBounds).FixedRightOf(adminShopLabel).WithFixedOffset(20, 10);
            rootBounds.WithChildren(adminShopLabel, adminShopBounds);

            // Save
            ElementBounds saveButtonBounds = ElementBounds.FixedSize(60, 20).FixedUnder(adminShopBounds, 10).WithAlignment(EnumDialogArea.RightTop);

            CairoFont hoverText = CairoFont.WhiteDetailText();
            CairoFont smallText = CairoFont.WhiteSmallText();
            CairoFont inputText = CairoFont.TextInput();


            composer.AddStaticText(Lang.Get(NameText), smallText, nameLabelBounds);
            composer.AddTextInput(nameInputBounds, null, inputText, "shopName");

            composer.AddStaticText(Lang.Get(ParentOwnableTypesText), smallText, parentSelectionLabel);
            composer.AddHoverText(Lang.Get("commercially:tooltip-parent"), hoverText, 500, parentSelectionLabel);
            composer.AddDropDown(ParentIDs, ParentNames, SelectedIndex, new SelectionChangedDelegate(this.OnSelectionChanged), parentSelectBounds, "parentSelection");

            composer.AddIf((Ownable != null && Ownable.IsAdminOwned) || IOwnable.IsCreativePlayer(API.World.Player))
                       .AddStaticText(Lang.Get("commercially:label-admin-owned"), smallText, adminShopLabel)
                       .AddHoverText(Lang.Get("commercially:tooltip-admin-owned"), hoverText, 500, adminShopLabel)
                       .AddSwitch(new Action<bool>(this.OnToggleAdminShop), adminShopBounds, "adminOwned")
                   .EndIf();

        }

        private void OnToggleAdminShop(bool obj)
        {

        }

        private void OnSelectionChanged(string code, bool selected)
        {

        }

        public override void Initialize(GuiDialog gui, BlockEntity entity = null)
        {
            base.Initialize(gui, entity);
            Ownable = entity?.GetBehavior<IOwnable>();
            JsonObject config = GetConfiguration("OwnableConfig");
            if (config != null && config.Exists)
            {
                ParentOwnableTypes = config["ParentTypes"].AsArray<string>();
                ParentOwnableTypesText = config["ParentLangName"].AsString();
                NameText = config["NameLangName"].AsString("Parent");
            } else
            {
                ParentOwnableTypes = Array.Empty<string>();
                ParentOwnableTypesText = "commercially:label-parent";
                NameText = "commercially:label-name";
            }

            ParentIDs = [];
            ParentNames = [];
        }

        public override bool IsVisible(GuiDialog gui)
        {
            return true;
        }

        public override void OnGuiClosed()
        {
            
        }

        public override void OnGuiOpened()
        {
            
        }
    }
}
