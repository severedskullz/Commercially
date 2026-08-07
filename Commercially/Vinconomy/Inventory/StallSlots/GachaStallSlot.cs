using Commercially.Common.Inventory.Slots;
using Commercially.Common.Util;
using Commercially.Vinconomy.Trading;
using System;
using Vinconomy.Inventory.Slots;
using Vinconomy.Util;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;

namespace Commercially.Vinconomy.Inventory.StallSlots
{
    public class GachaStallSlot : StallSlotBase
    {
        public override int StallSlotCount => (GachaContents?.Length ?? 0)  + (Products?.Length ?? 0);

        public override bool IsInitialized => Products != null;
        public override int InternalSlotCount => 0;

        public VinconCloningSlot[] GachaContents;
        public ItemSlot[] Products;
        public int Weight;

        // The currency will always be the same slot for the entire stall, since one of the slots will be selected at random to be the product.
        public override CurrencySlot Currency { get { return (CurrencySlot)Inventory.InternalSlots[0]; } protected set { Inventory.InternalSlots[0] = value; } }

        public override ItemSlot this[int slotId]
        {
            get
            {
                if (slotId == 0)
                    return Product;

                int index = slotId - 1;
                int gachaLength = GachaContents?.Length ?? 0;

                // Correct boundary check: valid indices are 0 to (gachaLength - 1)
                if (index < gachaLength)
                    return GachaContents[index];

                // Subtract the exact offset to shift into the Products array index space
                index -= gachaLength;
                return Products[index];

            }
            set
            {
                if (slotId == 0)
                    Product = (ProductSlot)value;

                int index = slotId - 1;
                if (index < GachaContents?.Length)
                    GachaContents[index] = (VinconCloningSlot)value;

                index -= GachaContents?.Length ?? 0;
                Products[index] = value;
            }
        }

        public GachaStallSlot(VinconBaseInventory inventory, int stallSlot, int contentSlots, int stockSlots) : base(inventory, stallSlot)
        {
            GachaContents = new VinconCloningSlot[contentSlots];
            for (int i = 0; i < contentSlots; i++)
            {
                GachaContents[i] = new VinconCloningSlot(inventory);
            }
            Products = new ItemSlot[stockSlots];
            for (int i = 0; i < stockSlots; i++)
            {
                Products[i] = new ItemSlot(inventory);
            }

        }

        public ItemSlot[] GetGachaContentSlots()
        {
            return GachaContents;
        }


        public override ItemSlot[] GetProductSlots()
        {
            return Products;
        }

        public void SetGachaStackCount(int slot, int amount)
        {
            GachaContents[slot].Itemstack?.StackSize = amount;
        }

        public override void ToTreeAttributes(ITreeAttribute tree)
        {
            // We do NOT want Currency / Product to tree, as these are generated or come from the Inventory itself.
            //base.ToTreeAttributes(tree);

            tree.SetInt("numSlots", Products.Length);
            for (int j = 0; j < Products.Length; j++)
            {
                if (Products[j].Itemstack != null)
                {
                    tree.SetItemstack("slot" + j, Products[j].Itemstack);
                }
            }

            tree.SetInt("numGachaSlots", GachaContents.Length);
            for (int j = 0; j < GachaContents.Length; j++)
            {
                if (GachaContents[j].Itemstack != null)
                {
                    tree.SetItemstack("gachaSlot" + j, GachaContents[j].Itemstack);
                }
            }
        }

        public override void FromTreeAttributes(ITreeAttribute tree)
        {
            // We do NOT want Currency / Product from tree, as these are generated or come from the Inventory itself.
            //base.FromTreeAttributes(tree);

            int numSlots = tree.GetInt("numSlots");
            int numGachaSlots = tree.GetInt("numGachaSlots");

            if (!IsInitialized)
            {
                Products = new ItemSlot[numGachaSlots];
                for (int i = 0; i < numGachaSlots; i++)
                {
                    GachaContents[i] = new VinconCloningSlot(Inventory);
                    ItemStack itemStack = tree.GetItemstack("gachaSlot" + i);
                    GachaContents[i].Itemstack = itemStack;
                    if (Inventory.Api?.World != null)
                    {
                        itemStack?.ResolveBlockOrItem(Inventory.Api.World);
                    }
                }

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
            }
            else
            {
                for (int i = 0; i < numGachaSlots; i++)
                {
                    ItemStack itemStack = tree.GetItemstack("gachaSlot" + i);
                    GachaContents[i].Itemstack = itemStack;
                    if (Inventory.Api?.World != null)
                    {
                        itemStack?.ResolveBlockOrItem(Inventory.Api.World);
                    }
                }

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

        public override void Initialize(VinconBaseInventory inventory, int stallSlot, int numSlotsPerStall)
        {
            base.Initialize(inventory, stallSlot, numSlotsPerStall);

            if (!IsInitialized)
            {
                Products = new ItemSlot[numSlotsPerStall];
                for (int i = 0; i < numSlotsPerStall; i++)
                {
                    Products[i] = new StockItemSlot(inventory, StallSlot, i); ;
                }
            }
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
                    this.Inventory.modSystem.Mod.Logger.Debug($"Adding {stack.StackSize}x {stack} product to Parent");
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
            int totalProductToMove = result.TotalProductAmount;
            AggregatedStacks productStacks = result.ProductStacks;

            if (result.Request.IsAdminShop)
            {
                int maxStackSize = result.Request.ProductNeeded.Collectible.MaxStackSize;
                while (totalProductToMove > 0)
                {
                    ItemStack transferStack = result.Request.ProductNeeded.Clone();
                    int stackSize = Math.Min(totalProductToMove, maxStackSize);
                    transferStack.StackSize = stackSize;
                    productStacks.Add(transferStack);
                    totalProductToMove -= stackSize;
                }
            }
            else
            {
                AggregatedSlots products = result.Request.ProductSourceSlots;
                foreach (ItemSlot slot in products)
                {
                    ItemStack takenStack = slot.TakeOut(totalProductToMove);
                    if (takenStack != null)
                    {
                        this.Inventory.modSystem.Mod.Logger.Debug($"Took out {takenStack.StackSize}x {takenStack} product from Product Stacks");
                        totalProductToMove -= takenStack.StackSize;
                        productStacks.Add(takenStack);
                        slot.MarkDirty();
                    }

                    if (totalProductToMove <= 0)
                    {
                        if (totalProductToMove < 0)
                        {
                            this.Inventory.modSystem.Mod.Logger.Error($"Somehow removed {Math.Abs(totalProductToMove)} extra items from Product");
                        }
                        break;
                    }
                }
            }

            if (totalProductToMove > 0)
            {
                this.Inventory.modSystem.Mod.Logger.Error($"Somehow missing {totalProductToMove}  items from Product");
            }
        }



        public override int GetProductQuantity()
        {
            int amount = Int32.MaxValue;
            if (Product?.Itemstack == null) return 0;

            for (int i = 0; i < StallSlotCount; i++)
            {
                ItemStack contents = GachaContents[i].Itemstack;
                if (contents != null)
                {
                    amount = Math.Min(amount, GetGachaContentQuantity(contents));
                }
            }

            return amount;
        }

        private int GetGachaContentQuantity(ItemStack contents)
        {
            int amount = 0;
            
            if (contents == null) return 0;

            foreach (ItemSlot item in Products)
            {
                if (TradingUtil.IsMatchingItem(contents, item.Itemstack, this.Inventory.Api.World, IsFuzzyMatching))
                {
                    amount += item.Itemstack.StackSize;
                }
            }
            return amount / contents.StackSize;
        }
    }
}
