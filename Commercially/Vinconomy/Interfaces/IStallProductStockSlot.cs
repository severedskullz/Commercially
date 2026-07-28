using Vintagestory.API.Common;

namespace Commercially.Vinconomy.Interfaces
{
    public interface IStallProductStockSlot
    {
        public int GetStall();
        public int GetProductSlot();
        public bool ShouldUpdateProductSlot(ItemSlot productSlot, ItemSlot sourceSlot);
        public bool ItemMatchesProduct(ItemStack product, ItemSlot sourceSlot);

        /// <summary>
        /// Updates the stall's Product slot with the source slot
        /// </summary>
        /// <param name="productSlot"></param>
        /// <param name="sourceSlot"></param>
        public void UpdateProductSlot(ItemSlot productSlot, ItemSlot sourceSlot);
    }
}