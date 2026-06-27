using Commercially.Common.Slots;
using Commercially.Vinconomy.Trading;
using System;
using Vinconomy.Inventory.Slots;
using Vinconomy.Util;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;
using Vintagestory.GameContent;

namespace Commercially.Vinconomy.Inventory.StallSlots
{
    public class MealStallSlot : StallSlotBase
    {
        public override int StallSlotCount => 1;

        public override bool IsInitialized => MealSlot != null;

        public ItemSlot MealSlot;
        public string RecipeCode { get; set; }

        public override ItemSlot this[int slotId] {
            get 
            {
                if (slotId == 0) return Currency;
                else if (slotId == 1) return Product;
                else return MealSlot;
            }
            set {
                if (slotId == 0) Currency = value;
                else if (slotId == 1) Product = value;
                else MealSlot = value;
            } 
        }

        public override ItemSlot[] GetStallSlots()
        {
            return [MealSlot];
        }

        public override void ToTreeAttributes(ITreeAttribute tree)
        {
            base.ToTreeAttributes(tree);
            tree.SetItemstack("slot0", MealSlot.Itemstack);
           
        }

        public override void FromTreeAttributes(ITreeAttribute tree)
        {
            base.FromTreeAttributes(tree);
           
            MealSlot = new VinconItemSlot(Inventory, StallSlot, 0);
                    
            ItemStack itemStack = tree.GetItemstack("slot0");
            MealSlot.Itemstack = itemStack;
            if (Inventory.Api?.World != null)
            {
                itemStack?.ResolveBlockOrItem(Inventory.Api.World);
            }
        }

        public override ItemSlot GetStallSlot(int itemSlot)
        {
            return MealSlot;
        }

        public override void PreInitialize(VinconBaseInventory inventory, int stallSlot)
        {
            Inventory = inventory;
            StallSlot = stallSlot;
        }

        public override void Initialize(VinconBaseInventory inventory, int stallSlot, int numSlotsPerStall)
        {
            PreInitialize(inventory, stallSlot);

            if (!IsInitialized)
            {
                MealSlot = new VinconItemSlot(inventory, StallSlot, 0);
                Currency = new VinconCloningSlot(inventory);
                Product = new VinconCloningSlot(inventory);
            }
        }

        public override AggregatedSlots GetProducts()
        {
            ICoreAPI api = Inventory.Api;
            AggregatedSlots slots = new AggregatedSlots(api);

            if (MealSlot.Itemstack != null && TradingUtil.IsMatchingItem(Product.Itemstack, MealSlot.Itemstack, api.World))
            {
                slots.Add(MealSlot);
            }

            return slots;
        }

        public override int AddProductToSlot(ItemSlot sourceSlot, bool bulk)
        {
            return AddProductToSlot(sourceSlot, bulk ? sourceSlot.StackSize : 1);
        }

        public override int AddProductToSlot(ItemSlot sourceSlot, int amount)
        {
            return 0;
        }

        public override int TakeProductFromSlot(int amount, out AggregatedStacks returnedItems, ItemSlot outputSlot, bool allowExcess = false)
        {
            return base.TakeProductFromSlot(amount, out returnedItems, outputSlot, allowExcess);
        }

        public ItemStack[] GetProductContents()
        {
            BlockCookedContainerBase block = MealSlot?.Itemstack?.Block as BlockCookedContainerBase;
            if (block != null)
            {
                return block.GetContents(this.Inventory.Api.World, MealSlot.Itemstack);
            }
            return null;
        }

        public bool CanAcceptFrom(IPlayer byPlayer, ItemSlot sourceSlot)
        {
            if (sourceSlot?.Itemstack == null) return false;

            if (VinUtils.IsEmptyContainer(sourceSlot.Itemstack, Inventory.Api) || !VinUtils.IsMealContainer(sourceSlot.Itemstack, Inventory.Api))
            {
                return false;
            }

            ItemStack[] sourceContents = VinUtils.GetContainerContents(sourceSlot.Itemstack, Inventory.Api);
            ItemStack[] productContents = GetProductContents();

            return VinUtils.IsMergableContents(sourceContents, productContents);
        }
    }
}
