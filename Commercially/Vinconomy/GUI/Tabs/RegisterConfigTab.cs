using Commercially.Common.GUI;
using Commercially.Common.Interfaces;
using Commercially.Common.Registry;
using System;
using System.IO;
using Vinconomy.Util;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;

namespace Commercially.Vinconomy.GUI.Tabs
{
    public class RegisterConfigTab : ModularTab
    {
        public const string CODE = "Vinconomy.RegisterConfig";
        private ElementBounds shortDescClipBounds;
        private ElementBounds descClipBounds;

        private OwnableRegistration ownable;
        private OwnableShopInformation shopInformation;

        public override string Code => CODE;
        public override string TabName => Lang.Get("vinconomy:tabname-register-config");

        public override void Initialize(IModularGui gui, BlockEntity entity = null)
        {
            base.Initialize(gui, entity);
            shopInformation = new OwnableShopInformation();
            ownable = new OwnableRegistration();
        }

        public override void Compose(GuiComposer composer, ElementBounds rootBounds)
        {
            ElementBounds shopNameLabelBounds = ElementBounds.Fixed(0, 35, 500, 25);
            ElementBounds shopNameInputBounds = ElementBounds.FixedSize(500, 25).FixedUnder(shopNameLabelBounds);

            ElementBounds shortDescriptionLabelBounds = shopNameInputBounds.BelowCopy().WithFixedOffset(0, 10).WithFixedSize(500, 25);

            ElementBounds shortDescInsetBounds = ElementBounds.FixedSize(480, 200).FixedUnder(shortDescriptionLabelBounds);
            shortDescClipBounds = shortDescInsetBounds.ForkContainingChild(GuiStyle.HalfPadding, GuiStyle.HalfPadding, GuiStyle.HalfPadding, GuiStyle.HalfPadding).FixedGrow(0, 0); // I dont know why "Grow" is needed here. It leaves me with 20px of missing space even if padding is 0.
            ElementBounds shortDescContainerBounds = shortDescInsetBounds.ForkContainingChild(GuiStyle.HalfPadding, GuiStyle.HalfPadding, GuiStyle.HalfPadding, GuiStyle.HalfPadding);
            ElementBounds shortDescScrollbarBounds = shortDescInsetBounds.RightCopy().WithFixedWidth(20);



            // ElementBounds shortDescriptionBounds = shortDescriptionLabelBounds.BelowCopy().WithFixedSize(500, 200);
            ElementBounds shortDescriptionSizeLabelBounds = shortDescInsetBounds.BelowCopy().WithFixedSize(500, 25);

            ElementBounds descriptionLabelBounds = shortDescriptionSizeLabelBounds.BelowCopy().WithFixedOffset(0, 10).WithFixedSize(500, 25);
            //ElementBounds descriptionBounds = descriptionLabelBounds.BelowCopy().WithFixedSize(500, 200);
            ElementBounds descriptionInsetBounds = ElementBounds.FixedSize(480, 200).FixedUnder(descriptionLabelBounds);

            descClipBounds = descriptionInsetBounds.ForkContainingChild(GuiStyle.HalfPadding, GuiStyle.HalfPadding, GuiStyle.HalfPadding, GuiStyle.HalfPadding).FixedGrow(0, 0); // I dont know why "Grow" is needed here. It leaves me with 20px of missing space even if padding is 0.
            ElementBounds descriptionContainerBounds = descriptionInsetBounds.ForkContainingChild(GuiStyle.HalfPadding, GuiStyle.HalfPadding, GuiStyle.HalfPadding, GuiStyle.HalfPadding);
            ElementBounds descriptionScrollbarBounds = descriptionInsetBounds.RightCopy().WithFixedWidth(20);

            ElementBounds descriptionSizeLabelBounds = descriptionInsetBounds.BelowCopy().WithFixedSize(500, 25);

            ElementBounds webhookLabelBounds = descriptionSizeLabelBounds.BelowCopy().WithFixedOffset(0, 10).WithFixedSize(500, 25);
            ElementBounds webhookBounds = webhookLabelBounds.BelowCopy().WithFixedOffset(0, 0).WithFixedSize(500, 25);

            ElementBounds saveButtonBounds = webhookBounds.BelowCopy().WithFixedSize(60, 20).WithFixedOffset(0, 10).WithAlignment(EnumDialogArea.RightTop);

            rootBounds.WithChildren(shopNameLabelBounds, shopNameInputBounds, shortDescInsetBounds, descriptionInsetBounds,
                shortDescScrollbarBounds, descriptionScrollbarBounds,
                shortDescriptionLabelBounds, shortDescriptionSizeLabelBounds,
                descriptionLabelBounds, descriptionSizeLabelBounds,
                webhookLabelBounds, webhookBounds, saveButtonBounds);

            CairoFont hoverText = CairoFont.WhiteDetailText();

            composer
                .AddStaticText(Lang.Get("vinconomy:gui-name"), CairoFont.WhiteSmallText(), shopNameLabelBounds)
                .AddTextInput(shopNameInputBounds, null, CairoFont.TextInput(), "shopName")

                .AddStaticText(Lang.Get("vinconomy:gui-short-description"), CairoFont.WhiteSmallText(), shortDescriptionLabelBounds)
                .AddHoverText(Lang.Get("vinconomy:tooltip-short-description"), hoverText, 500, shortDescriptionLabelBounds)

                .AddInset(shortDescInsetBounds, 3)
                .BeginClip(shortDescClipBounds);
            try
            {
                composer.AddTextArea(shortDescContainerBounds, UpdateShortDesc, CairoFont.TextInput(), "shortDescription");
            }
            catch (Exception ex)
            {
                composer.AddRichtext(Lang.Get("vinconomy:gui-error-tell-the-dev") + ex.Message, CairoFont.WhiteDetailText(), shortDescContainerBounds, "description");
            }
            composer.EndClip()
            .AddVerticalScrollbar(OnNewShortDescScrollbarValue, shortDescScrollbarBounds, "shortdescription-scrollbar")
            .AddDynamicText("0 / 250", CairoFont.WhiteSmallishText(), shortDescriptionSizeLabelBounds, "shortDescriptionLength")

            .AddStaticText(Lang.Get("vinconomy:gui-description"), CairoFont.WhiteSmallText(), descriptionLabelBounds)
            .AddHoverText(Lang.Get("vinconomy:tooltip-description"), hoverText, 500, descriptionLabelBounds)
            //.AddTextArea(descriptionBounds, UpdateLongCount, CairoFont.TextInput(), "description")
            .AddInset(descriptionInsetBounds, 3)
            .BeginClip(descClipBounds);
            try
            {
                composer.AddTextArea(descriptionContainerBounds, UpdateLongDesc, CairoFont.TextInput(), "description");
            }
            catch (Exception ex)
            {
                composer.AddRichtext("There was an error in the store's description. Exception " + ex.Message, CairoFont.WhiteDetailText(), descriptionContainerBounds, "description");
            }
            composer.EndClip()
            .AddVerticalScrollbar(OnNewDescriptionScrollbarValue, descriptionScrollbarBounds, "description-scrollbar")
            .AddDynamicText("0 / 2500", CairoFont.WhiteSmallishText(), descriptionSizeLabelBounds, "descriptionLength")

            .AddStaticText(Lang.Get("vinconomy:gui-webhook"), CairoFont.WhiteSmallText(), webhookLabelBounds)
            .AddHoverText(Lang.Get("vinconomy:tooltip-webhook"), hoverText, 500, webhookLabelBounds)
            .AddTextInput(webhookBounds, null, CairoFont.TextInput(), "webhook")

            .AddButton(Lang.Get("vinconomy:gui-save"), OnSaveShopConfigPressed, saveButtonBounds, EnumButtonStyle.Small, "save");


            
            int shortLength = shopInformation.ShortDescription == null ? 0 : shopInformation.ShortDescription.Length;
            int longLength = shopInformation.Description == null ? 0 : shopInformation.Description.Length;
            composer.GetTextInput("shopName").SetValue(ownable.Name);

            GuiElementTextArea shortDesc = composer.GetTextArea("shortDescription");
            shortDesc.SetValue(shopInformation.ShortDescription);

            GuiElementTextArea longDesc = composer.GetTextArea("description");
            longDesc.SetValue(shopInformation.Description);

            composer.GetTextInput("webhook").SetValue(shopInformation.WebHook);
            composer.GetDynamicText("shortDescriptionLength").SetNewText($"{shortLength} / 250");
            composer.GetDynamicText("descriptionLength").SetNewText($"{longLength} / 1024");
            
            UpdateShortDescScrollbar();
            UpdateDescScrollbar();

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

        public override byte[] OnSendData(BlockEntity entity, Caller caller, BlockSelection blockSel, string key)
        {
            return null;
        }

        private bool OnSaveShopConfigPressed()
        {
            byte[] data;
            using (MemoryStream ms = new MemoryStream())
            {
                BinaryWriter writer = new BinaryWriter(ms);
                writer.Write(Gui.Composer.GetTextInput("shopName").GetText());
                writer.Write(Gui.Composer.GetTextArea("description").GetText());
                writer.Write(Gui.Composer.GetTextArea("shortDescription").GetText());
                writer.Write(Gui.Composer.GetTextInput("webhook").GetText());
                data = ms.ToArray();
            }
            Api.Network.SendBlockEntityPacket(BlockEntityPosition, VinConstants.SET_SHOP_NAME, data);
            return true;
        }

        private void UpdateShortDescScrollbar()
        {
            float descScrollVisibleHeight = (float)descClipBounds.fixedHeight;
            double descScrollTotalHeight = Gui.Composer.GetTextArea("shortDescription").Bounds.fixedHeight;
            Gui.Composer.GetScrollbar("shortdescription-scrollbar").SetHeights(descScrollVisibleHeight, (float)descScrollTotalHeight);
        }

        private void UpdateDescScrollbar()
        {
            float descScrollVisibleHeight = (float)descClipBounds.fixedHeight;
            double descScrollTotalHeight = Gui.Composer.GetTextArea("description").Bounds.fixedHeight;
            Gui.Composer.GetScrollbar("description-scrollbar").SetHeights(descScrollVisibleHeight, (float)descScrollTotalHeight);
        }

        private void OnNewDescriptionScrollbarValue(float value)
        {
            ElementBounds bounds = Gui.Composer.GetTextArea("description").Bounds;
            bounds.fixedY = 5 - value;
            bounds.CalcWorldBounds();
        }

        private void OnNewShopAccessScrollbarValue(float value)
        {
            ElementBounds bounds = Gui.Composer.GetStaticText("container").Bounds;
            bounds.fixedY = 5 - value;
            bounds.CalcWorldBounds();
        }


        private void OnNewShortDescScrollbarValue(float value)
        {
            ElementBounds bounds = Gui.Composer.GetTextArea("shortDescription").Bounds;
            bounds.fixedY = 5 - value;
            bounds.CalcWorldBounds();
        }

        private void UpdateShortDesc(string obj)
        {
            if (!Gui.Composer.Composed) return;
            GuiElementDynamicText desc = Gui.Composer.GetDynamicText("shortDescriptionLength");

            desc.SetNewText($"{obj.Length} / 250");

            if (obj.Length > 250)
            {
                GuiElementTextArea area = Gui.Composer.GetTextArea("shortDescription");
                area.SetValue(obj.Substring(0, 250));
            }

            if (obj.Length >= 250)
            {
                desc.Font.Color[0] = 255;
                desc.Font.Color[1] = 0;
                desc.Font.Color[2] = 0;
            }
            else
            {
                desc.Font.Color[0] = 255;
                desc.Font.Color[1] = 255;
                desc.Font.Color[2] = 255;
            }
            desc.RecomposeText();
            UpdateShortDescScrollbar();
        }

        private void UpdateLongDesc(string obj)
        {
            if (!Gui.Composer.Composed) return;
            GuiElementDynamicText desc = Gui.Composer.GetDynamicText("descriptionLength");
            if (obj.Length > 1024)
            {
                GuiElementTextArea area = Gui.Composer.GetTextArea("description");
                area.SetValue(obj.Substring(0, 1024));
            }

            desc.SetNewText($"{obj.Length} / 1024");
            if (obj.Length >= 1024)
            {
                desc.Font.Color[0] = 255;
                desc.Font.Color[1] = 0;
                desc.Font.Color[2] = 0;
            }
            else
            {
                desc.Font.Color[0] = 255;
                desc.Font.Color[1] = 255;
                desc.Font.Color[2] = 255;
            }
            desc.RecomposeText();
            UpdateDescScrollbar();
        }
    }
}
