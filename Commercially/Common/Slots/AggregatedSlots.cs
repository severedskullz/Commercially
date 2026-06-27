using System;
using System.Collections;
using System.Collections.Generic;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;
using Vintagestory.GameContent;

namespace Commercially.Common.Slots
{
    public class AggregatedSlots : IEnumerable<ItemSlot>
    {
        public AggregatedSlots(ICoreAPI api)
        {
            Api = api;
        }

        public ICoreAPI Api; // This is rediculous Tyron - just to get meal contents?

        public List<ItemSlot> Slots { get; set; } = new List<ItemSlot>();
        public int TotalCount { get; set; }

        public virtual void Add(ItemSlot item)
        {
            Slots.Add(item);
            TotalCount += item.StackSize;
        }

        public virtual List<ItemStack> Remove(int num)
        {
            int left = num;
            List<ItemStack> stacks = new List<ItemStack>();
            foreach (ItemSlot item in Slots)
            {
                ItemStack stack = item.TakeOut(left);
                
                if (stack != null)
                {
                    TotalCount -= stack.StackSize;
                    left -= stack.StackSize;
                    stacks.Add(stack);
                }

                if (left <= 0)
                {
                    break;
                }
            }

            return stacks;
        }

        public ItemSlot this[int index] { get => Slots[index]; }
        IEnumerator IEnumerable.GetEnumerator() => Slots.GetEnumerator();

        public IEnumerator<ItemSlot> GetEnumerator()
        {
            return Slots.GetEnumerator();
        }
    }

    public class CapacityAggregatedSlots : AggregatedSlots
    {
        public float TotalCapacity { get; set; }

        public CapacityAggregatedSlots(ICoreAPI api) : base(api)
        {
        }


    }

    public class ServingCapacityAggregatedSlots : CapacityAggregatedSlots
    {

        public ServingCapacityAggregatedSlots(ICoreAPI api) : base(api)
        {

        }

        
        public override void Add(ItemSlot item)
        {
            int curServings = 0;

            Slots.Add(item);
            TotalCount += item.StackSize;

            IBlockMealContainer meal = item.Itemstack.Block as IBlockMealContainer;
            if (meal != null)
                curServings = (int)Math.Ceiling(meal.GetQuantityServings(Api.World, item.Itemstack)); //We assume it can merge, anyway...

            JsonObject attr = item.Itemstack.Block.Attributes;

            int capacity = 0;
            if (attr.KeyExists("servingCapacity"))
            {
                capacity = attr["servingCapacity"].AsInt();
            }
            TotalCapacity += Math.Max(0, capacity - curServings);
        }
    }

    public class LiquidCapacityAggregatedSlots : CapacityAggregatedSlots
    {
        public LiquidCapacityAggregatedSlots(ICoreAPI api) : base(api)
        {

        }

        public override void Add(ItemSlot item)
        {
            Slots.Add(item);
            TotalCount += item.StackSize;

            BlockLiquidContainerBase container = item.Itemstack.Block as BlockLiquidContainerBase;
            //ItemStack containerContent = null;
            //if (container != null)
            //    containerContent = container.GetContent(item.Itemstack);

            float capacity = container.CapacityLitres;
            float curLiters = container.GetCurrentLitres(item.Itemstack);
            TotalCapacity += Math.Max(0, (capacity - curLiters) * item.StackSize);
        }
    }

    //TODO: Figure out how to get durabilty for tools. For now we can just manually set it
    public class DurabilityAggregatedSlots : AggregatedSlots
    {
        public DurabilityAggregatedSlots(ICoreAPI api) : base(api)
        {
        }

        public int TotalDurability { get; set; }
        public override void Add(ItemSlot item)
        {
            Slots.Add(item);
            TotalCount += item.StackSize;
            //TotalDurability += item.Durability ????
        }
    }
}
