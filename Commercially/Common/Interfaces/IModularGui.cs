using Vintagestory.API.Client;
using Vintagestory.API.Common;

namespace Commercially.Common.Interfaces
{
    public interface IModularGui
    {
        public InventoryBase Inventory { get; }
        public GuiComposer Composer { get; }
        public void LoadTabs(string[] tabs);
        public GuiTab[] GetGUITabs();
        public void FullRecompose();
        public void SendPacket(int packetId, byte[] packet);
        public void SendPacket(int packetId, object packet);
        public void SendPacket(object obj);
    }
}
