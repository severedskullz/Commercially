using System.Collections.Generic;
using Vintagestory.API.Client;

namespace Commercially.Common.Interfaces
{
    public interface IModularGui
    {
        public GuiComposer Composer { get; }
        public void LoadTabs(string[] tabs);
        public GuiTab[] GetTabs();
        public void FullRecompose();
        public void SendPacket(int packetId, byte[] packet);
        public void SendPacket(int packetId, object packet);
        public void SendPacket(object obj);
    }
}
