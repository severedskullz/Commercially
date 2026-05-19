using Commercially.Common.Interfaces;
using Commercially.Common.Registry;
using System;
using System.IO;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Datastructures;
using Vintagestory.API.Util;

namespace Commercially.Common.GUI.Tabs
{
    public class GuiBlockEntityOwnershipTab : ModularTab
    {
        public const string CODE = "commercially.Ownership";
        public override string Code => CODE;
        public override string TabName => Lang.Get("commercially:tabname-ownership");

        IOwnable Ownable;
        private CommerciallyModSystem ModSystem;
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

            if (Ownable is IOwnableChild)
            {
                composer.AddStaticText(Lang.Get(ParentOwnableTypesText), smallText, parentSelectionLabel);
                composer.AddHoverText(Lang.Get("commercially:tooltip-parent"), hoverText, 500, parentSelectionLabel);
                composer.AddDropDown(ParentIDs, ParentNames, SelectedIndex, new SelectionChangedDelegate(this.OnSelectionChanged), parentSelectBounds, "parentSelection");
            }


            composer.AddIf((Ownable != null && Ownable.IsAdminOwned) || IOwnable.IsCreativePlayer(API.World.Player))
                       .AddStaticText(Lang.Get("commercially:label-admin-owned"), smallText, adminShopLabel)
                       .AddHoverText(Lang.Get("commercially:tooltip-admin-owned"), hoverText, 500, adminShopLabel)
                       .AddSwitch(OnToggleAdminShop, adminShopBounds, "adminOwned")
                   .EndIf();

            GuiElementTextInput textComponent = composer.GetTextInput("shopName");
            textComponent.SetValue(Ownable?.Name);
            textComponent.OnTextChanged = OnTextChanged; // Fuck you Tyron, Give me a way to set the text WITHOUT firing the delegate.

            //GuiElementDropDown parentComponent = composer.GetDropDown("parentSelection");
            GuiElementSwitch isAdminComponent = composer.GetSwitch("adminOwned");
            isAdminComponent.SetValue(Ownable?.IsAdminOwned ?? false);

        }

        private void OnTextChanged(string obj)
        {
            byte[] data;
            using (MemoryStream ms = new MemoryStream())
            {
                BinaryWriter writer = new BinaryWriter(ms);
                writer.Write(obj);
                data = ms.ToArray();
            }
            Gui.SendPacket(CommerciallyConstants.SET_NAME, data);
        }

        private void OnToggleAdminShop(bool obj)
        {
            byte[] data;
            using (MemoryStream ms = new MemoryStream())
            {
                BinaryWriter writer = new BinaryWriter(ms);
                writer.Write(obj);
                data = ms.ToArray();
            }
            Gui.SendPacket(CommerciallyConstants.SET_ADMIN_OWNED, data);
        }

        private void OnSelectionChanged(string code, bool selected)
        {
            byte[] data;
            using (MemoryStream ms = new MemoryStream())
            {
                BinaryWriter writer = new BinaryWriter(ms);
                int id = code.ToInt(-1);
                writer.Write(id);
                data = ms.ToArray();
            }
            Gui.SendPacket(CommerciallyConstants.SET_PARENT_ID, data);
        }

        public override void Initialize(IModularGui gui, BlockEntity entity = null)
        {
            base.Initialize(gui, entity);
            Ownable = entity?.GetBehavior<IOwnable>();
            ModSystem = API.ModLoader.GetModSystem<CommerciallyModSystem>();


            JsonObject config = GetConfiguration("OwnableConfig");
            if (config != null && config.Exists)
            {
                ParentOwnableTypes = config["ParentTypes"].AsArray<string>();
                ParentOwnableTypesText = config["ParentLangName"].AsString();
                NameText = config["NameLangName"].AsString("Parent");
            }
            else if (Ownable is IOwnableChild)
            {
                ParentOwnableTypes = ((IOwnableChild)Ownable).GetAllowedParentTypes();
                ParentOwnableTypesText = "commercially:label-parent";
                NameText = "commercially:label-name";
            } else
            {
                ParentOwnableTypes = Array.Empty<string>();
                ParentOwnableTypesText = "commercially:label-parent";
                NameText = "commercially:label-name";
            }

            if (Ownable is IOwnableChild) { 
                OwnableRegistration[] ownables = ModSystem.OwnableRegistry.GetOwnablesForOwner(Ownable.OwnerUID, ParentOwnableTypes);
                int shopLength = ownables.Length;
                ParentIDs = new string[shopLength + 1];
                ParentNames = new string[shopLength + 1];

                ParentNames[0] = "( None )";
                ParentIDs[0] = "-1";

                for (int i = 0; i < shopLength; i++)
                {
                    ParentNames[i + 1] = ownables[i].Name ?? "Generic Shop";
                    ParentIDs[i + 1] = ownables[i].ID.ToString();
                    if (ownables[i].ID == (Ownable as IOwnableChild).ParentID)
                    {
                        SelectedIndex = i + 1;
                    }
                }
            }

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

        public override void OnRecievedData(byte[] data)
        {
        }

        public override byte[] OnSendData(BlockEntity entity)
        {
            return null;
        }
    }
}
