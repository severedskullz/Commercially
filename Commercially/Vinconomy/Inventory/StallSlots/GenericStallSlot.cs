using Commercially.Common.Inventory.Slots;
using Commercially.Vinconomy.Trading;
using Commercially.Vinconomy.Trading.Processor;
using System;
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
                if (slotId == 0) Currency = (VinconCloningSlot) value;
                else if (slotId == 1) Product = (FilteredItemSlot) value;
                else
                {
                    Products[slotId - 2] = value;
                }
            } 
        }

        public GenericStallSlot(InventoryBase inventory, int stallSlot) : base(inventory, stallSlot) { 
        }


        public override ItemSlot[] GetProductSlots()
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
                    Products[i] = new StockItemSlot(Inventory, StallSlot, i);
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

        public override ItemSlot GetProductSlot(int itemSlot)
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
                    Products[i] = new StockItemSlot(inventory, StallSlot, i); ;
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

        public override void TransferProdutToPlayer(TradeResult result)
        {
            if (result.ProductStacks.TotalCount == 0) return;

            IPlayer player = result.Request.Customer;
            AssetLocation sound = null;
            while (result.ProductStacks.CanRemoveStack())
            {
                ItemStack stack = result.ProductStacks.RemoveStack();

                if (stack != null)
                {
                    this.Inventory.Api.ModLoader.GetModSystem<VinconomyModSystem>().Mod.Logger.Debug($"Adding {stack.StackSize}x {stack} product to Parent");
                    if (stack.Block?.Sounds?.Place.Location != null)
                    {
                        sound = stack.Block?.Sounds?.Place.Location;
                    }

                    player.InventoryManager.TryGiveItemstack(stack, true);
                    if (stack.StackSize > 0)
                    {
                        result.Request.Api.World.SpawnItemEntity(stack, player.Entity.Pos.XYZ.Add(0.5), null);
                    }
                }
            }

            result.Request.Api.World.PlaySoundAt(sound ?? new AssetLocation("sounds/player/build"), result.Request.Customer.Entity, result.Request.Customer, true, 16f, 1f);
        }

        public override void ExtractProductFromStall(TradeResult result)
        {
            AggregatedSlots products = result.Request.ProductSourceSlots;
            int totalProductToMove = result.Request.GetFinalProductNeededPerPurchase() * result.Request.NumPurchases;
            AggregatedStacks productStacks = result.ProductStacks;

            foreach (ItemSlot slot in products)
            {
                ItemStack takenStack = slot.TakeOut(totalProductToMove);
                if (takenStack != null)
                {
                    GenericTradingProcessor.AuditLogDebug(result, $"Took out {takenStack.StackSize}x {takenStack} product from Product Stacks");
                    totalProductToMove -= takenStack.StackSize;
                    productStacks.Add(takenStack);
                    slot.MarkDirty();
                }

                if (totalProductToMove <= 0)
                {
                    if (totalProductToMove < 0)
                    {
                        GenericTradingProcessor.AuditLogError(result, $"Somehow removed {Math.Abs(totalProductToMove)} extra items from Product");
                    }
                    break;
                }

            }
        }
    }
}
