using Commercially.Common.Slots;
using Commercially.Vinconomy.Trading;
using System;
using Vinconomy.Inventory.Slots;
using Vinconomy.Util;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Datastructures;
using Vintagestory.GameContent;

namespace Commercially.Vinconomy.Inventory.StallSlots
{
    public class MealStallSlot : StallSlotBase
    {
        public override int StallSlotCount => 1;

        public override bool IsInitialized => MealSlot != null;

        public ItemSlot MealSlot;
        private int ServingCapacity = 64;

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

        public override int GetProductQuantity()
        {
            if (Product?.Itemstack == null) return 0;

            return (int)VinUtils.GetMealContainerServings(MealSlot.Itemstack, Inventory.Api);
        }

        public override int AddProductToSlot(ItemSlot sourceSlot, bool bulk)
        {
            return AddProductToSlot(sourceSlot, bulk ? sourceSlot.StackSize : 1);
        }

        public override int AddProductToSlot(ItemSlot source, int amount)
        {
            /*
            if (!CanAcceptFrom(source)) return 0;

            IBlockMealContainer sourceBlock = source.Itemstack?.Block as IBlockMealContainer;
            IWorldAccessor world = Inventory.Api.World;

            ItemStack[] sourceStacks = VinUtils.GetContainerContents(source.Itemstack, Inventory.Api);
            string? sourceRecipeCode = sourceBlock.GetRecipeCode(world, source.Itemstack);
            float sourceServings = sourceBlock.GetQuantityServings(world, source.Itemstack);

            if (MealSlot.Itemstack == null)
            {
                RecipeCode = sourceRecipeCode;

                Block generatedMealBlock = world.GetBlock("game:claypot-gray-cooked");
                IBlockMealContainer genMeal = generatedMealBlock as IBlockMealContainer; // While 9 out of 10 times this is probably going to have the same implementation, better safe than sorry.
                ItemStack stack = new ItemStack(generatedMealBlock, 0); // Yes, 0. We will set the servings below.
                MealSlot.Itemstack = stack;
            }
            */
            //TODO: Figure out how to convert this to an Int later on.
            return AddMeal(source, amount) ? amount : 0;
        }

        public override int TakeProductFromSlot(int amount, out AggregatedStacks returnedItems, ItemSlot outputSlot, bool allowExcess = false)
        {
            returnedItems = null;
            /*
            float moved = ServeIntoStack(outputSlot, MealSlot, Inventory.Api.World);
            if (VinUtils.GetMealContainerServings(MealSlot.Itemstack, Inventory.Api) <= 0)
            {
                MealSlot.Itemstack = null;
                MealSlot.MarkDirty();
                //TODO: Might run into visual issues here if we dont update the block entity? Verify later on! Might need to manully mark the block entity as dirty to re-render the models.
                RecipeCode = null;
            }
            */
            return RemoveMeal(outputSlot, amount) ? amount : 0;
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

        public bool CanAcceptFrom(ItemSlot sourceSlot)
        {
            if (sourceSlot?.Itemstack == null) return false;

            if (VinUtils.IsEmptyContainer(sourceSlot.Itemstack, Inventory.Api) || !VinUtils.IsMealContainer(sourceSlot.Itemstack, Inventory.Api))
            {
                return false;
            }

            ItemStack[] sourceContents = VinUtils.GetContainerContents(sourceSlot.Itemstack, Inventory.Api);
            ItemStack[] productContents = GetProductContents();

            return VinUtils.IsMergableContents(Inventory.Api.World, sourceContents, productContents);
        }

        //Mostly adapted from Tyron's BlockCookedContainerBase code
        public float ServeIntoStack(ItemSlot destSlot, ItemSlot sourceSlot, IWorldAccessor world, float servingCapacityOverride = 0)
        {
            ItemSlot destination = destSlot;
            ItemSlot source = sourceSlot;

            IBlockMealContainer sourceBlock = source.Itemstack?.Block as IBlockMealContainer;
            IBlockMealContainer destBlock = destination.Itemstack?.Block as IBlockMealContainer;

            ItemStack[] sourceStacks = VinUtils.GetContainerContents(source.Itemstack, Inventory.Api);
            ItemStack[] destStacks = VinUtils.GetContainerContents(destination.Itemstack, Inventory.Api);

            string? sourceRecipeCode = sourceBlock.GetRecipeCode(world, source.Itemstack);
            string? destRecipeCode = destBlock.GetRecipeCode(world, destination.Itemstack);

            float sourceServings = sourceBlock.GetQuantityServings(world, source.Itemstack);
            float destServings = destBlock.GetQuantityServings(world, destination.Itemstack);


            float quantityServings = sourceServings;
            string? ownRecipeCode = sourceRecipeCode;
            float servingCapacity = destination.Itemstack?.Block.Attributes["servingCapacity"].AsFloat(1) ?? 1;

            // Overrides the serving capacity of the Pot to the Stall's serving capacity. Should only be set when we are merging food INTO the stall.
            if (servingCapacityOverride > 0)
            {
                servingCapacity = servingCapacityOverride;
            }
        

            // Merge existing servings
            if (destination.Itemstack?.Block is IBlockMealContainer destMealContainer)
            {
               
                if (destStacks != null && destServings > 0)
                {
                    if (sourceStacks.Length != destStacks.Length) return 0;

                    if (ownRecipeCode != destRecipeCode) return 0;

                    float remainingPlaceableServings = servingCapacity - destServings;
                    if (remainingPlaceableServings <= 0) return 0;

                    for (int i = 0; i < sourceStacks.Length; i++)
                    {
                        if (!sourceStacks[i].Equals(world, destStacks[i], GlobalConstants.IgnoredStackAttributes))
                        {
                            return 0;
                        }
                    }

                    if (world.Side == EnumAppSide.Client) return 0;

                    // Ok merge transition states
                    for (int i = 0; i < destStacks.Length; i++)
                    {
                        ItemStackMergeOperation op = new ItemStackMergeOperation(world, EnumMouseButton.Left, 0, EnumMergePriority.ConfirmedMerge, sourceStacks[i].StackSize);
                        op.SourceSlot = new DummySlot(sourceStacks[i]);
                        op.SinkSlot = new DummySlot(destStacks[i]);
                        destStacks[i].Collectible.TryMergeStacks(op);
                    }

                    // Now increase serving size
                    float movedservings = Math.Min(remainingPlaceableServings, quantityServings);
                    destMealContainer.SetQuantityServings(world, destSlot.Itemstack, destServings + movedservings);

                    SetServingsMaybeEmpty(world, sourceSlot, quantityServings - movedservings);

                    sourceSlot.Itemstack?.Attributes.RemoveAttribute("sealed");
                    destSlot.Itemstack?.Attributes.RemoveAttribute("sealed");

                    sourceSlot.MarkDirty();
                    destSlot.MarkDirty();

                    return movedservings;
                }
            }


            if (world.Side == EnumAppSide.Client) return 1;
            ItemStack[] stacks = VinUtils.GetContainerContents(sourceSlot.Itemstack, Inventory.Api);
            string? code = destSlot.Itemstack?.Block.Attributes["mealBlockCode"].AsString();
            if (code == null) return 0;
            Block? mealblock = Inventory.Api.World.GetBlock(new AssetLocation(code));

            float servingsToTransfer = Math.Min(quantityServings, servingCapacity);

            ItemStack stack = new ItemStack(mealblock);
            (mealblock as IBlockMealContainer)?.SetContents(ownRecipeCode, stack, stacks, servingsToTransfer);

            SetServingsMaybeEmpty(world, sourceSlot, quantityServings - servingsToTransfer);
            sourceSlot.Itemstack?.Attributes.RemoveAttribute("sealed");
            sourceSlot.MarkDirty();

            destSlot.Itemstack = stack;
            destSlot.MarkDirty();
            return servingsToTransfer;
        }

        internal void SetServingsMaybeEmpty(IWorldAccessor world, ItemSlot potslot, float value)
        {
            IBlockMealContainer sourceBlock = potslot.Itemstack?.Block as IBlockMealContainer;
            sourceBlock.SetQuantityServings(world, potslot.Itemstack, value);
            if (value <= 0f)
            {
                string? emptyCode = potslot.Itemstack?.Block.Attributes["emptiedBlockCode"].AsString();
                if (emptyCode != null)
                {
                    Block? emptyPotBlock = world.GetBlock(new AssetLocation(emptyCode));
                    if (emptyPotBlock != null) potslot.Itemstack = new ItemStack(emptyPotBlock);
                }
            }
        }


        public bool AddMeal(ItemSlot sourceSlot, int amount)
        {
            ItemStack sourceMeal = sourceSlot.Itemstack;
            if (sourceMeal == null)
                return false;

            IBlockMealContainer sourceMealBlock = sourceMeal.Block as IBlockMealContainer;
            if (sourceMealBlock == null) return false;

            IWorldAccessor world = Inventory.Api.World;


            float servings = sourceMealBlock.GetQuantityServings(world, sourceMeal);
            int servingsToTransfer = Math.Min(Math.Min(amount, (int)servings), ServingCapacity - MealSlot.StackSize);
            float remainingServings = servings - servingsToTransfer;

            if (servingsToTransfer <= 0)
                return false;

            if (MealSlot.Itemstack != null)
            {
                if (!CanMergeMeal(sourceMeal, MealSlot.Itemstack))
                {
                    return false;
                }

                //Disabling for now. Not sure why this is causing MarkDirty() below to throw a NRE for the transitionables...
                /*
                IBlockMealContainer stallMealBlock = mealSlot.Itemstack.Block as IBlockMealContainer;
                ItemStack[] contents = stallMealBlock.GetContents(world, mealSlot.Itemstack);
                for (int i = 0; i < contents.Length; i++)
                {
                    if (contents[i] != null)
                        contents[i].Attributes.GetTreeAttribute("transitionstate")?.SetFloat("transitionedHours", 0);
                }

                //transitionstate.transitionedHours
                stallMealBlock.SetContents(stallMealBlock.GetRecipeCode(world,mealSlot.Itemstack), mealSlot.Itemstack, contents);
                */

                MealSlot.Itemstack.StackSize += servingsToTransfer;
            }
            else
            {

                RecipeCode = sourceMealBlock.GetRecipeCode(world, sourceMeal);

                Block generatedMealBlock = world.GetBlock("game:claypot-gray-cooked");
                IBlockMealContainer genMeal = generatedMealBlock as IBlockMealContainer; // While 9 out of 10 times this is probably going to have the same implementation, better safe than sorry.
                ItemStack stack = new ItemStack(generatedMealBlock);
                genMeal.SetContents(RecipeCode, stack, sourceMealBlock.GetContents(world, sourceMeal), 1);
                stack.StackSize = servingsToTransfer;
                MealSlot.Itemstack = stack;
            }


            //Remove the meal contents from the source block, converting it to the Eaten Block if neccessary
            sourceMeal.Attributes?.RemoveAttribute("sealed");
            if (remainingServings > 0)
            {
                sourceMealBlock.SetQuantityServings(world, sourceMeal, remainingServings);
            }
            else
            {
                // Check if we need to switch item stacks if its a BlockCookedContainer (AKA Cooking Pot) first, because cooking pot does not have `eatenBlock` attribute
                string code = sourceMeal.Block.Attributes["eatenBlock"].AsString();
                if (code == null)
                {
                    sourceMealBlock.SetContents(null, sourceMeal, null, 0);
                }
                else
                {
                    Block mealblock = world.GetBlock(new AssetLocation(code));
                    ItemStack stack = new ItemStack(mealblock);
                    sourceSlot.Itemstack = stack;
                }

            }

            MealSlot.MarkDirty();
            sourceSlot.MarkDirty();
            return true;
        }

        public bool RemoveMeal(ItemSlot targetSlot, int amount)
        {
            if (MealSlot.Itemstack == null)
            {
                return false;
            }

            ItemStack targetMeal = targetSlot.Itemstack;
            if (targetMeal == null)
                return false;

            IWorldAccessor world = Inventory.Api.World;

            // Fuck you Tyron for changing this shit yet again. As if dealing with Bowls wasnt bad enough...
            if (targetMeal.Block is BlockCookingContainer emptyPot)
            {
                Block block = world.GetBlock(targetMeal.Block.CodeWithVariant("type", "cooked"));
                ItemStack mealStack = new ItemStack(block);
                IBlockMealContainer mealStackBlock = mealStack.Block as IBlockMealContainer;
                IBlockMealContainer stallMealBlock = MealSlot.Itemstack.Block as IBlockMealContainer;

                int targetCapacity = targetMeal.Block.Attributes["servingCapacity"].AsInt(0);
                int servingsToTransfer = Math.Min(Math.Min(amount, MealSlot.StackSize), (int)(targetCapacity));
                ItemStack[] stallContents = stallMealBlock.GetContents(world, MealSlot.Itemstack);
                mealStackBlock.SetContents(RecipeCode, mealStack, stallContents, servingsToTransfer);
                targetSlot.Itemstack = mealStack;
                MealSlot.Itemstack.StackSize -= servingsToTransfer;
                if (MealSlot.StackSize <= 0)
                {
                    MealSlot.Itemstack = null;
                    RecipeCode = null;
                }
            }
            else
            {
                IBlockMealContainer targetMealBlock = targetMeal.Block as IBlockMealContainer;
                if (targetMealBlock == null) return false;



                float targetServings = targetMealBlock.GetQuantityServings(world, targetMeal);
                int targetCapacity = targetMeal.Block.Attributes["servingCapacity"].AsInt(0);
                int servingsToTransfer = Math.Min(Math.Min(amount, MealSlot.StackSize), (int)(targetCapacity - targetServings));


                if (servingsToTransfer <= 0)
                    return false;

                //If container is not empty
                if (targetMealBlock.GetNonEmptyContents(world, targetMeal).Length > 0)
                {
                    if (!CanMergeMeal(targetMeal, MealSlot.Itemstack))
                    {
                        return false;
                    }
                    targetMealBlock.SetQuantityServings(world, targetMeal, targetServings + servingsToTransfer);
                }
                else
                {
                    IBlockMealContainer stallMealBlock = MealSlot.Itemstack.Block as IBlockMealContainer;
                    targetMealBlock.SetContents(RecipeCode, targetMeal, stallMealBlock.GetContents(world, MealSlot.Itemstack), servingsToTransfer);
                }

                MealSlot.Itemstack.StackSize -= servingsToTransfer;
                if (MealSlot.StackSize <= 0)
                {
                    MealSlot.Itemstack = null;
                    RecipeCode = null;
                }

                targetMeal.Attributes?.RemoveAttribute("sealed");
            }

            MealSlot.MarkDirty();
            targetSlot.MarkDirty();
            return true;
        }

        public bool CanMergeMeal(ItemStack source, ItemStack target)
        {
            IWorldAccessor world = Inventory.Api.World;

            if (target == null)
                return true;


            IBlockMealContainer containerFrom = source.Block as IBlockMealContainer;
            if (containerFrom == null)
            {
                return false;
            }

            ItemStack[] contentsFrom = containerFrom.GetNonEmptyContents(world, source);
            string recipeCodeFrom = containerFrom.GetRecipeCode(world, source);

            // check if recipe code matches
            if (RecipeCode != recipeCodeFrom)
            {
                return false;
            }


            // check if ingredients match
            IBlockMealContainer sourceMeal = target.Block as IBlockMealContainer;
            ItemStack[] sourceContents = sourceMeal.GetContents(world, target);
            if (sourceContents.Length != 0)
            {
                if (sourceContents.Length != contentsFrom.Length)
                {
                    return false;
                }

                for (int i = 0; i < contentsFrom.Length; i++)
                {
                    ItemStack bowlStack = contentsFrom[i];
                    ItemStack containerStack = sourceContents[i];

                    if (bowlStack.Id != containerStack.Id)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        public override bool MatchesProduct(ItemStack itemStack)
        {
            return CanMergeMeal(itemStack, MealSlot.Itemstack);
        }

    }
}
