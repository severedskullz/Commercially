using Commercially.Common.Interfaces;
using Commercially.Common.Inventory.Slots;
using Commercially.Common.Util;
using Commercially.Vinconomy.Interfaces;
using Commercially.Vinconomy.Inventory.Impl;
using Commercially.Vinconomy.Inventory.Slots;
using Commercially.Vinconomy.Trading;
using System;
using System.Collections.Generic;
using Vinconomy.Inventory.Slots;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Datastructures;

namespace Commercially.Vinconomy.Inventory.StallSlots
{
    public class GachaStallSlot : StallSlotBase, IGeneratedProductStall
    {
        public override int StallSlotCount => (GachaContents?.Length ?? 0)  + (Products?.Length ?? 0) + 1;

        public override bool IsInitialized => Products != null;
        public override int InternalSlotCount => 0;

        public GachaProductSlot[] GachaContents;
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
                if (index < GachaContents?.Length)
                    return GachaContents[index];

                index -= GachaContents?.Length ?? 0;
                return Products[index];

            }
            set
            {
                if (slotId == 0)
                    Product = (ProductSlot)value;

                int index = slotId - 1;
                if (index < GachaContents?.Length)
                    GachaContents[index] = (GachaProductSlot)value;

                index -= GachaContents?.Length ?? 0;
                Products[index] = value;
            }
        }

        public GachaStallSlot(VinconBaseInventory inventory, int stallSlot, int contentSlots, int stockSlots) : base(inventory, stallSlot)
        {
            GachaContents = new GachaProductSlot[contentSlots];
            for (int i = 0; i < contentSlots; i++)
            {
                GachaContents[i] = new GachaProductSlot(inventory, stallSlot, i);
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

            tree.SetInt("weight", Weight);
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
            Weight = Math.Max(1, tree.GetInt("weight", 1));

            if (!IsInitialized)
            {
                Products = new ItemSlot[numGachaSlots];
                for (int i = 0; i < numGachaSlots; i++)
                {
                    GachaContents[i] = new GachaProductSlot(Inventory, StallSlot, i);
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


            List<ItemStack> neededItems = new List<ItemStack>(GachaContents.Length);
            //Check for duplicate items and combine them if players have 2 of the same items in 2 or more slots
            for (int i = 0; i < GachaContents.Length; i++)
            {
                ItemStack contents = GachaContents[i].Itemstack;
                if (contents == null) continue;

                ItemStack existing = null;
                foreach (ItemStack desired in neededItems)
                {
                    if (TradingUtil.IsMatchingItem(contents, desired, this.Inventory.Api.World, IsFuzzyMatching))
                    {
                        existing = desired;
                        break;
                    }
                }

                if (existing != null)
                {
                    existing.StackSize += contents.StackSize;
                }
                else
                {
                    neededItems.Add(contents);
                }
            }

            if (neededItems.Count == 0) return 0;

            int amount = Int32.MaxValue;
            // Once we have all of the condensed items, check if we have enough for a trade
            foreach (ItemStack desired in neededItems)
            {
                amount = Math.Min(amount, GetGachaContentQuantity(desired));
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

        public int GetStallWeight()
        {
            int productCount = GetProductQuantity();

            if (productCount <= 0)
                return 0;

            GachaShopInventory gachaInventory = Inventory as GachaShopInventory;
            if (gachaInventory?.IsCountBasedRandomizer == true)
            {
                return productCount * Weight;
            }

            return Weight;
        }

        public ItemStack GenStubbedProduct()
        {
            ItemStack stack = new ItemStack(Inventory.Api.World.GetItem(new AssetLocation("vinconomy:gachaball")), 1);          
            return stack;
        }

        public ItemStack GenProduct()
        {
            ItemStack stack = GenStubbedProduct();

            TreeAttribute contents = new TreeAttribute();

            //ITreeAttribute contents = (TreeAttribute)treeAttr.GetTreeAttribute("Contents");
            int numItems = 0;
            foreach (var itemSlot in GachaContents)
            {
                if (itemSlot.Itemstack != null)
                {
                    contents.SetItemstack("Item" + numItems, itemSlot.Itemstack);
                    numItems++;
                }
            }
            TreeAttribute treeAttr = stack.Attributes as TreeAttribute;

            contents.SetLong("NumContents", numItems);
            treeAttr.SetAttribute("Contents", contents);
            return stack;
        }

        public void RegenProduct()
        {
            int slot = StallSlot;
            if (GetProductQuantity() == 0)
            {
                Product.Itemstack = null;
            } else
            {
                Product.Itemstack = GenProduct();
            }
            
            Product.MarkDirty();
        }

        /*
        public override TradeRequest CreateTradeRequest(IPlayer player, int numPurchases, IShopComponent shop, IStallComponent stall)
        {
            TradeRequest request = new TradeRequest(Inventory.Api, player);
            IOwnable ownable = stall.Ownable;
            ItemStack currencyStack = Currency.Itemstack;
            ItemStack productStack = Product.Itemstack;
            request.WithShop(shop, stall, StallSlot, ownable?.IsAdminOwned ?? false);
            request.WithPurchases(numPurchases);
            request.WithCurrency(currencyStack, TradingUtil.GetAllValidSlotsFor(player, currencyStack), currencyStack.StackSize);

            AggregatedSlots slots = new AggregatedSlots(Inventory.Api);
            int quantity = GetProductQuantity();
            for (int i = 0; i < quantity; i++)
            {
                slots.Add(productStack.Clone());   
            }


            request.WithProduct(productStack, GetProducts(), productStack.StackSize);

            AggregatedSlots coupons = TradingUtil.GetCouponsSlotsFor(player, request.ProductNeeded, shop);
            if (coupons.Slots.Count > 0)
            {
                request.WithCoupons(coupons.Slots[0]);
            }

            request.WithContainers(GetRequiredContainers(player));

            if (shop != null)
            {
                ITradePassProvider inv = shop.GetComponent<IInventoryProvider>()?.Inventory as ITradePassProvider;
                if (inv != null)
                {
                    ItemStack tradePass = inv.GetTradePass();
                    if (tradePass != null)
                    {
                        request.WithTradePass(tradePass, TradingUtil.GetAllValidSlotsFor(player, tradePass));
                    }
                }
            }
            return request.Build();
        }
        */

    }
}
