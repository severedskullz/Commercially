using Vintagestory.API.Common;

namespace Commercially.Common.Interfaces
{
    public interface IDecocratedBlock
    {
        public ItemStack GetDecorationBlock();

        public ItemSlot GetDecorationSlot();

    }
}