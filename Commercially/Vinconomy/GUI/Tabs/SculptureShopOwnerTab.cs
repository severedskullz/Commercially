using Commercially.Common.GUI;
using System;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;

namespace Commercially.Vinconomy.GUI.Tabs
{
    public class SculptureShopOwnerTab : ModularTab
    {

        public const string CODE = "Vinconomy.SculptureOwner";
        public override string Code => CODE;

        public override string TabName => Lang.Get("vinconomy:tabname-sculpture-owner");

        public override void Compose(GuiComposer composer, ElementBounds rootBounds)
        {
            throw new NotImplementedException();
        }

        public override bool IsVisible(GuiDialog gui)
        {
            throw new NotImplementedException();
        }

        public override void OnGuiClosed()
        {
            throw new NotImplementedException();
        }

        public override void OnGuiOpened()
        {
            throw new NotImplementedException();
        }

        public override void OnRecievedData(byte[] data)
        {
            throw new NotImplementedException();
        }

        public override byte[] OnSendData(BlockEntity entity, Caller caller, BlockSelection blockSel, string key)
        {
            throw new NotImplementedException();
        }
    }
}
