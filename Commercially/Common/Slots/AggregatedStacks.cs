using System.Collections.Generic;
using Vintagestory.API.Common;
using Vintagestory.API.Util;

namespace Commercially.Common.Slots
{

    public class AggregatedStacks
    {
        private List<ItemStack> Slots { get; set; } = new List<ItemStack>();
        public int TotalCount { get; set; }

        public virtual void Add(ItemStack item)
        {
            Slots.Add(item);
            TotalCount += item.StackSize;
        }

        public virtual List<ItemStack> Remove(int num)
        {
            int left = num;
            List<ItemStack> stacks = new List<ItemStack>();
            int slotCount = Slots.Count;
            for (int i = 0; i < slotCount; i++)
            {
                ItemStack stack = Slots[0];
                if (stack.StackSize <= left)
                {
                    Slots.RemoveAt(0);
                    left -= stack.StackSize;
                    stacks.Add(stack);
                } else
                {
                    ItemStack newItem = stack.Clone();
                    newItem.StackSize = left;
                    stack.StackSize -= newItem.StackSize;
                    TotalCount -= newItem.StackSize;
                    stacks.Add(newItem);
                    break;
                }

                if (left <= 0)
                {
                    break;
                }
            }
            return stacks;
        }

        public virtual ItemStack RemoveStack()
        {
            ItemStack stack = Slots.PopFirst();
            if (stack != null)
                TotalCount -= stack.StackSize;
            return stack;
        }

        public virtual bool CanRemoveStack() {
            return TotalCount > 0 && Slots.Count > 0; 
        }
    }
}
