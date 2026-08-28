using Commercially.Common.Interfaces;
using System;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;

namespace Commercially.Common.GUI.Tabs
{
    public class GuiBlockEntityContainerTab : ModularTab
    {
        public const string CODE = "Commercially.Container";
        public override string Code => CODE;
        public override string TabName => Lang.Get("commercially:tabname-container");

        InventoryBase Inventory;
        int NumColumns;

        public override void Compose(GuiComposer composer, ElementBounds rootBounds)
        {
            if (Inventory != null)
            {

                ElementBounds slotGrid = ElementStdBounds.SlotGrid(EnumDialogArea.None, 0, 0, NumColumns, (int)Math.Ceiling(Inventory.Count / (double)NumColumns)).WithFixedOffset(0, GuiStyle.TitleBarHeight);
                rootBounds.WithChild(slotGrid);

                composer.AddItemSlotGrid(Inventory, (Gui as GUIModularBlockEntity).SendPacket, NumColumns, slotGrid);
            }
            else
            {
                ElementBounds textBounds = ElementBounds.FixedSize(400, 300).WithFixedOffset(0, GuiStyle.TitleBarHeight);
                rootBounds.WithChild(textBounds);

                composer.AddStaticText(Lang.Get("commercially:container-no-inventory"), CairoFont.WhiteSmallText(), textBounds);
            }
        }

        public override void Initialize(IModularGui gui, BlockEntity entity = null)
        {
            base.Initialize(gui, entity);
            Inventory = entity?.GetBehavior<IInventoryProvider>()?.Inventory;
            NumColumns = GetConfiguration()?["NumColumns"].AsInt(10) ?? 10;

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
