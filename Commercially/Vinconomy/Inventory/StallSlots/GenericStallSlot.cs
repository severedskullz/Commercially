using Commercially.Common.Slots;
using Commercially.Vinconomy.Trading;
using Vinconomy.Inventory.Slots;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;

namespace Commercially.Vinconomy.Inventory.StallSlots
{
    public class GenericStallSlot : StallSlotBase
    {
        public override int StallSlotCount => Products?.Length ?? 0;

        public override bool IsInitialized => Products != null;

        public ItemSlot[] Products;

        public override ItemSlot this[int slotId] {
            get 
            {
                if (slotId == 0) return Currency;
                else if (slotId == 1) return Product;
                else
                {
                    return Products[slotId-2];
                }
            }
            set {
                if (slotId == 0) Currency = value;
                else if (slotId == 1) Product = value;
                else
                {
                    Products[slotId - 2] = value;
                }
            } 
        }

        public override ItemSlot[] GetStallSlots()
        {
            return Products;
        }

        public override void ToTreeAttributes(ITreeAttribute tree)
        {
            base.ToTreeAttributes(tree);
            tree.SetInt("numSlots", Products.Length);
            for (int j = 0; j < StallSlotCount; j++)
            {
                if (Products[j].Itemstack != null)
                {
                    tree.SetItemstack("slot" + j, Products[j].Itemstack);
                }
            }
        }

        public override void FromTreeAttributes(ITreeAttribute tree)
        {
            base.FromTreeAttributes(tree);

            int numSlots = tree.GetInt("numSlots");

            if (!IsInitialized)
            {
                Products = new ItemSlot[numSlots];
                for (int i = 0; i < numSlots; i++)
                {
                    Products[i] = new VinconItemSlot(Inventory, StallSlot, i);
                    ItemStack itemStack = tree.GetItemstack("slot" + i);
                    Products[i].Itemstack = itemStack;
                    if (Inventory.Api?.World != null)
                    {
                        itemStack?.ResolveBlockOrItem(Inventory.Api.World);
                    }
                }
            } else
            {
                for (int i = 0; i < numSlots; i++)
                {
                    ItemStack itemStack = tree.GetItemstack("slot" + i);
                    Products[i].Itemstack = itemStack;
                    if (Inventory.Api?.World != null)
                    {
                        itemStack?.ResolveBlockOrItem(Inventory.Api.World);
                    }
                }
            }
            
        }

        public override ItemSlot GetStallSlot(int itemSlot)
        {
            return Products[itemSlot];
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
                Products = new ItemSlot[numSlotsPerStall];
                for (int i = 0; i < numSlotsPerStall; i++)
                {
                    Products[i] = new VinconItemSlot(inventory, StallSlot, i); ;
                }
                Currency = new VinconCloningSlot(inventory);
                Product = new VinconCloningSlot(inventory);
            }
        }

        public override AggregatedSlots GetProducts()
        {
            ICoreAPI api = Inventory.Api;
            AggregatedSlots slots = new AggregatedSlots(api);
            foreach (var slot in Products)
            {
                if (slot.Itemstack != null && TradingUtil.IsMatchingItem(Product.Itemstack, slot.Itemstack, api.World))
                {
                    slots.Add(slot);
                }
            }

            return slots;
        }
    }
}
