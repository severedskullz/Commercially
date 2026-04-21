using Commercially.Common;
using Commercially.Common.GUI;
using Commercially.Common.Interfaces;
using Commercially.Vinconomy.Interfaces;
using Commercially.Vinconomy.Inventory;
using Commercially.Vinconomy.Inventory.StallSlots;
using System;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;

namespace Commercially.Vinconomy.GUI.Tabs
{
    public class GuiBlockEntityShopCustomerTab : ModularTab
    {

        public const string CODE = "vinconomy.ShopCustomer";
        public override string Code => CODE;

        public override string TabName => Lang.Get("vinconomy:tabname-generic-customer");

        VinconBaseInventory Inventory;
        IStallInventoryProvider StallProvider;
        private IOwnable Ownable;
        int StallSlot;



        int NumColumns;

        public override void Compose(GuiComposer composer, ElementBounds rootBounds)
        {
            if (Inventory != null && StallProvider != null)
            {
                CairoFont hoverText = CairoFont.WhiteDetailText();
                CairoFont smallText = CairoFont.WhiteSmallText();
                CairoFont labelTextFont = CairoFont.WhiteSmallText().WithOrientation(EnumTextOrientation.Center);
                string labelText = "Page 1 of 4"; //Lang.Get("vinconomy:gui-slot", new object[] { StallSlot + 1, stall.StallSlotCount });


                int shopLength = 10;
                string[] shopsNames = new string[shopLength];
                string[] shopsKeys = new string[shopLength];

                for (int i = 0; i < shopLength; i++)
                {
                    shopsNames[i] = "blah";
                    shopsKeys[i] = "blah";
                }

                // Figure out the slot indexes for SlotGrid
                StallSlotBase stall = StallProvider.GetStallSlot(StallSlot);
                int[] slotGridIDs = new int[stall.StallSlotCount];
                int stallSlotOffset = Inventory.InternalSlots.Length;
                for (int i = 0; i < StallSlot; i++)
                {
                    stallSlotOffset += StallProvider.GetStallSlot(i).TotalSlotCount;
                }


                for (int i = 0; i < slotGridIDs.Length; i++)
                {
                    slotGridIDs[i] = stallSlotOffset + stall.InternalSlotCount + i;
                }

                int numColumns = (int)Math.Ceiling(Math.Sqrt(slotGridIDs.Length));
                int slotGridWidth = (int) (numColumns * (GuiElementPassiveItemSlot.unscaledSlotSize + GuiElementItemSlotGridBase.unscaledSlotPadding));


                ElementBounds settingBounds = ElementBounds.FixedSize(250, 150).WithFixedOffset(10, GuiStyle.TitleBarHeight+10);
                settingBounds.BothSizing = ElementSizing.FitToChildren;
                rootBounds.WithChild(settingBounds);

                ElementBounds shopSelectionLabel = ElementBounds.Fixed(0, 0, 75, 30);
                ElementBounds shopSelectBounds = shopSelectionLabel.BelowCopy().WithFixedWidth(250);
                settingBounds.WithChildren(shopSelectBounds, shopSelectionLabel);
                composer.AddStaticText(Lang.Get("vinconomy:gui-shop"), smallText, shopSelectionLabel);
                composer.AddHoverText(Lang.Get("vinconomy:tooltip-shop"), hoverText, 500, shopSelectionLabel);
                composer.AddDropDown(shopsKeys, shopsNames, 0, this.OnShopChanged, shopSelectBounds, "shopSelection");



                ElementBounds chiselLabel = ElementBounds.FixedSize(200, 25).FixedUnder(shopSelectBounds,15);
                ElementBounds chiselSlotBounds = ElementStdBounds.SlotGrid(EnumDialogArea.None, 0, 0, 1, 1).FixedUnder(chiselLabel);
                settingBounds.WithChildren(chiselLabel, chiselSlotBounds);
                composer.AddStaticText(Lang.Get("vinconomy:gui-decoration-block"), smallText, chiselLabel);
                composer.AddHoverText(Lang.Get("vinconomy:tooltip-decoration-block"), hoverText, 500, chiselLabel);
                composer.AddItemSlotGrid(Inventory, new Action<object>(this.SetCurrencySlot), 1, new int[] { 0 }, chiselSlotBounds, "chisel");

                ElementBounds adminShopBounds = ElementBounds.FixedSize(40, 40).WithAlignment(EnumDialogArea.LeftBottom).FixedUnder(chiselSlotBounds).WithFixedOffset(0, 10);
                ElementBounds adminShopLabel = ElementBounds.FixedSize(100, 25).WithAlignment(EnumDialogArea.LeftBottom).FixedUnder(chiselSlotBounds).FixedRightOf(adminShopBounds).WithFixedOffset(0, 10);

                settingBounds.WithChildren(adminShopLabel, adminShopBounds);
                composer.AddSwitch(this.OnToggleAdminShop, adminShopBounds, "admin");
                composer.AddStaticText(Lang.Get("vinconomy:gui-admin-shop"), smallText, adminShopLabel);
                composer.AddHoverText(Lang.Get("vinconomy:tooltip-admin-shop"), hoverText, 500, adminShopLabel);

                ElementBounds pageBounds = ElementBounds.FixedSize(250, 30).FixedRightOf(settingBounds, 15).WithFixedOffset(0, GuiStyle.TitleBarHeight);
                rootBounds.WithChild(pageBounds);
                ElementBounds pagePrev = ElementBounds.FixedSize(30, 30);
                ElementBounds pageLabel = ElementBounds.FixedSize(Math.Max(slotGridWidth, 250), 25).WithFixedAlignmentOffset(0, 5).FixedRightOf(pagePrev, 10);
                ElementBounds pageNext = ElementBounds.FixedSize(30, 30).FixedRightOf(pageLabel, 10);
                pageBounds.WithChildren(pagePrev, pageLabel, pageNext);
                composer.AddButton("<", null, pagePrev, EnumButtonStyle.Small, "prevPage");
                composer.AddDynamicText(labelText, labelTextFont, pageLabel, "pageLabel");
                composer.AddButton(">", null, pageNext, EnumButtonStyle.Small, "nextPage");


                ElementBounds stallBounds = ElementBounds.FixedSize(250, 200).FixedRightOf(settingBounds, 15).FixedUnder(pageBounds,10);
                stallBounds.BothSizing = ElementSizing.FitToChildren;
                rootBounds.WithChild(stallBounds);
                //composer.AddInset(stallBounds);

                ElementBounds priceLabel = ElementBounds.FixedSize(100, 30).WithFixedOffset(5,10);
                ElementBounds priceSlotBounds = ElementStdBounds.SlotGrid(EnumDialogArea.None, 0, 0, 1, 1).WithFixedOffset(5, 0).FixedUnder(priceLabel);
                ElementBounds priceInputBounds = ElementBounds.FixedSize(75, 30).FixedUnder(priceLabel).FixedRightOf(priceSlotBounds).WithFixedOffset(10, 10);
                stallBounds.WithChildren(priceLabel, priceSlotBounds, priceInputBounds);
                composer.AddStaticText(Lang.Get("vinconomy:gui-price"), smallText, priceLabel);
                composer.AddHoverText(Lang.Get("vinconomy:tooltip-price"), hoverText, 500, priceLabel);
                composer.AddItemSlotGrid(Inventory, this.SetCurrencySlot, 1, new int[] { stallSlotOffset }, priceSlotBounds, "currency");

                //ElementBounds priceInputLabel = ElementBounds.FixedSize(150, 30).FixedUnder(priceSlotBounds).WithFixedOffset(0, 15);
                

                //composer.AddStaticText(Lang.Get("vinconomy:gui-cost-per-purchase"), smallText, priceInputLabel);
                //composer.AddHoverText(Lang.Get("vinconomy:tooltip-cost-per-purchase"), hoverText, 500, priceInputLabel);
                composer.AddNumberInput(priceInputBounds, this.OnCostQuantityChanged, smallText, "costQuantity");

                ElementBounds productLabel = ElementBounds.FixedSize(100, 30).FixedRightOf(priceLabel, 80).WithFixedOffset(0,10);
                ElementBounds productSlotBounds = ElementStdBounds.SlotGrid(EnumDialogArea.None, 0, 0, 1, 1).FixedUnder(productLabel).FixedRightOf(priceLabel, 80);
                ElementBounds productInputBounds = ElementBounds.FixedSize(75, 30).FixedUnder(productLabel).FixedRightOf(productSlotBounds).WithFixedOffset(10, 10);
                stallBounds.WithChildren(productLabel, productSlotBounds, productInputBounds);
                composer.AddStaticText(Lang.Get("vinconomy:gui-product"), smallText, productLabel);
                composer.AddHoverText(Lang.Get("vinconomy:tooltip-product"), hoverText, 500, productLabel);
                composer.AddItemSlotGrid(Inventory, this.SetProductSlot, 1, new int[] { stallSlotOffset + 1 }, productSlotBounds, "product");

                //ElementBounds productInputLabel = ElementBounds.FixedSize(150, 30).FixedUnder(priceInputLabel).WithFixedOffset(0, 15);
                
                //stallBounds.WithChildren(productInputLabel, productInputBounds);
                //composer.AddStaticText(Lang.Get("vinconomy:gui-items-per-purchase"), smallText, productInputLabel);
                //composer.AddHoverText(Lang.Get("vinconomy:tooltip-items-per-purchase"), hoverText, 500, productInputLabel);
                composer.AddNumberInput(productInputBounds, this.OnSellQuantityChanged, smallText, "sellQuantity");


                ElementBounds stockLabel = ElementBounds.FixedSize(Math.Max(slotGridWidth, 250) + 80, 25).FixedUnder(productSlotBounds, 15);
                stallBounds.WithChildren(stockLabel);
                composer.AddStaticText(Lang.Get("vinconomy:gui-product"), labelTextFont, stockLabel);
                composer.AddHoverText(Lang.Get("vinconomy:tooltip-product"), hoverText, 500, stockLabel);

                ElementBounds slotGrid = ElementStdBounds.SlotGrid(EnumDialogArea.CenterTop, 0, 20, numColumns, numColumns).FixedUnder(stockLabel,-20);
                stallBounds.WithChild(slotGrid);
                composer.AddItemSlotGrid(Inventory, (Gui as GUIModularBlockEntity).SendPacket, (int)Math.Ceiling(Math.Sqrt(slotGridIDs.Length)), slotGridIDs, slotGrid, "inventory");



            }
            else if (StallProvider == null)
            {
                ElementBounds textBounds = ElementBounds.FixedSize(400, 300).WithFixedOffset(0, GuiStyle.TitleBarHeight);
                rootBounds.WithChild(textBounds);
                composer.AddStaticText(Lang.Get("vinconomy:not-a-shop"), CairoFont.WhiteSmallText(), textBounds);
            }
            else
            {
                ElementBounds textBounds = ElementBounds.FixedSize(400, 300).WithFixedOffset(0, GuiStyle.TitleBarHeight);
                rootBounds.WithChild(textBounds);
                composer.AddStaticText(Lang.Get("commercially:container-no-inventory"), CairoFont.WhiteSmallText(), textBounds);
            }
        }

        private void SetProductSlot(object obj)
        {
            
        }

        private void OnToggleAdminShop(bool obj)
        {
            
        }

        private void SetCurrencySlot(object obj)
        {
            
        }

        private void OnSellQuantityChanged(string obj)
        {
            
        }

        private void OnCostQuantityChanged(string obj)
        {
            
        }

        private void OnShopChanged(string code, bool selected)
        {
        }

        public override void Initialize(GuiDialog gui, BlockEntity entity = null)
        {
            base.Initialize(gui, entity);
            Inventory = entity?.GetBehavior<IInventoryProvider>()?.Inventory as VinconBaseInventory;
            StallProvider = entity?.GetBehavior<IStallInventoryProvider>();
            Ownable = entity?.GetBehavior<IOwnable>();
            NumColumns = GetConfiguration()?["NumColumns"].AsInt(10) ?? 10;

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
