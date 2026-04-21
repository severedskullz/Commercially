using System;
using Vintagestory.API.Common;

namespace Commercially.Common.Interfaces
{
    public interface IPersistableStackAttributes
    {
        /// <summary>
        /// Called from the CommercialEvents block behavior when a block with this behavior is broken and drops an item stack.
        /// This allows us to add any necessary attributes to the item stack that need to be persisted before it is dropped in the world, such as the materials of a stall or the owner of a shop.
        /// </summary>
        /// <param name="stack"></param>
        /// <remarks>Be sure to call the base class implementation if overriding this method.</remarks>
        public void AddAttributes(ItemStack stack);
    }
}
