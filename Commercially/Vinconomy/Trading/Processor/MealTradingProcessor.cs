using System;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;
using Vintagestory.GameContent;

namespace Commercially.Vinconomy.Trading.Processor
{
    public class MealTradingProcessor
    {
        public static int TransferToMealBlock(IPlayer player, ItemSlot containerSlot, string recipe, ItemStack[] mealStacks, int servings)
        {
            int servingsToTransfer = 0;
            int capacity = 0;

            ICoreAPI api = player.Entity.Api;

            // Why the fuck isnt the servingCapacity also on the meal block code?
            // I have to be missing something here.
            JsonObject attr = containerSlot.Itemstack.Block.Attributes;
            if (attr.KeyExists("servingCapacity"))
            {
                capacity = attr["servingCapacity"].AsInt();
            }
            if (capacity <= 0)
            {
                return 0;
            }

            if (containerSlot.Itemstack.Block is IBlockMealContainer meal)
            {
                int currentServings = (int)Math.Ceiling(meal.GetQuantityServings(api.World, containerSlot.Itemstack));
                if (currentServings >= capacity)
                    return 0;

                servingsToTransfer = Math.Min(servings, capacity - currentServings);
                meal.SetContents(recipe, containerSlot.Itemstack, mealStacks, currentServings + servingsToTransfer);
                containerSlot.Itemstack.Attributes.RemoveAttribute("sealed");

                player.InventoryManager.NotifySlot(player, containerSlot);
                containerSlot.MarkDirty();
            }
            else
            {
                ItemStack mealStack = ConvertToMealContainer(api, containerSlot.Itemstack);
                if (mealStack != null)
                {
                    if (mealStack.Block is not IBlockMealContainer mealBlock)
                    {
                        throw new Exception("Somehow got a meal stack that wasn't a meal container");
                    }

                    servingsToTransfer = Math.Min(servings, capacity);
                    mealBlock.SetContents(recipe, mealStack, mealStacks, servingsToTransfer);
                    containerSlot.TakeOut(1);
                    containerSlot.MarkDirty();

                    if (!player.InventoryManager.TryGiveItemstack(mealStack, true))
                    {
                        api.World.SpawnItemEntity(mealStack, player.Entity.Pos.XYZ.AddCopy(0.5, 0.5, 0.5), null);
                    }
                }
            }

            return servingsToTransfer;
        }

        public static ItemStack ConvertToMealContainer(ICoreAPI api, ItemStack stack)
        {
            if (stack.Block is IBlockMealContainer)
                return stack;

            // Cooking Pot - always empty, block type changes when it is turned into claypot-cooked
            if (!(stack.Block is BlockCookingContainer || stack.Block is BlockContainer))
                return null;

            JsonObject attr = stack.Block.Attributes;
            if (attr == null)
                return null;

            string code = attr["mealBlockCode"]?.AsString();
            if (code == null)
                return null;

            int capacity = 0;
            if (attr.KeyExists("servingCapacity"))
            {
                capacity = attr["servingCapacity"].AsInt(); ;
            }
            if (capacity <= 0)
            {
                return null;
            }

            Block mealblock = api.World.GetBlock(code);
            if (mealblock == null)
                return null;

            return new ItemStack(mealblock);
        }

        public static void TransferProductToPlayer(TradeResult result)
        {
            if (result.ProductStacks.TotalCount == 0) return;

            IBlockMealContainer mealContainer = result.Request.ProductNeeded.Block as IBlockMealContainer;
            if (mealContainer == null)
                return;

            string recipeCode = mealContainer.GetRecipeCode(result.Request.Api.World, result.Request.ProductNeeded);
            ItemStack[] mealStacks = mealContainer.GetContents(result.Request.Api.World, result.Request.ProductNeeded);

            int totalServingsLeftToTransfer = result.ProductStacks.TotalCount;
            // loop through player's containers and convert to meal blocks
            foreach (ItemSlot containerSlot in result.Request.ContainerSourceSlots.Slots)
            {
                // Save stacksize as variable. We will be taking items OUT of this stack, so it would exit the loop early.
                // Eg. Had 2 bowls, loop ran, took one out, 'i' is now 1, and stack size is 1, so loop terminates and doesnt run on second bowl.
                int numAttempts = containerSlot.StackSize;
                for (int i = 0; i < numAttempts; i++)
                {
                    int capacity = containerSlot.Itemstack.Block.Attributes["servingCapacity"].AsInt();
                    int servingsToTransfer = Math.Min(totalServingsLeftToTransfer, capacity);
                    int moved = TransferToMealBlock(result.Request.Customer, containerSlot, recipeCode, mealStacks, totalServingsLeftToTransfer);
                    totalServingsLeftToTransfer -= moved;

                    //TODO: ProductStacks is was not modified in old Vinconomy Code. I retrofitted it here, but need to ensure its working properly
                    result.ProductStacks.Remove(moved);


                    if (totalServingsLeftToTransfer <= 0)
                        break;
                }

                if (totalServingsLeftToTransfer <= 0)
                    return;

            }

            if (totalServingsLeftToTransfer > 0)
            {
                GenericTradingProcessor.AuditLogError(result, "Somehow allowed purchase of " + totalServingsLeftToTransfer + " extra servings even though we didnt have enough containers");
            }
        }
    }
}
