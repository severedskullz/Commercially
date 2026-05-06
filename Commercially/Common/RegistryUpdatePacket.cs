using Commercially.Common.Registry;
using ProtoBuf;
using System.Collections.Generic;

namespace Commercially.Common
{
    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class RegistryUpdatePacket
    {
        public List<OwnableUpdatePacket> registry;

        public RegistryUpdatePacket() { }

        public RegistryUpdatePacket(List<OwnableUpdatePacket> registry)
        {
            this.registry = registry;
        }

    }
}