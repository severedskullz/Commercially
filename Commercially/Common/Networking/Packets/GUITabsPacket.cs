#nullable enable
using ProtoBuf;
using System.Collections.Generic;

namespace Commercially.Common.Networking.Packets
{
    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class GUITabsPacket
    {
        public Dictionary<string, GUITabPacket> Tabs = new Dictionary<string, GUITabPacket>();

        public void AddTab(string tab, byte[] data)
        {
            Tabs[tab] = new GUITabPacket { TabCode = tab, Data = data };
        }

        public byte[]? GetData(string tab)
        {
            if (Tabs.TryGetValue(tab, out GUITabPacket? packet))
            {
                return packet.Data;
            }
            return null;
        }
    }
}
