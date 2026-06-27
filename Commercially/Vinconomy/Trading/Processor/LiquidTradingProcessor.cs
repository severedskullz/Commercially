using System;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Datastructures;
using Vintagestory.GameContent;

namespace Commercially.Vinconomy.Trading.Processor
{
    public class LiquidTradingProcessor
    {
        public static AssetLocation fillSound = new AssetLocation("sounds/effect/water-fill.ogg");

        public static void TransferProductToPlayer(TradeResult res)
        {
            if (res.ProductStacks.TotalCount == 0)
            {
                GenericTradingProcessor.AuditLogError(res, "Tried to give player products, but has nothing to give");
            }

            IPlayer player = res.Request.Customer;
            while(res.ProductStacks.CanRemoveStack())
            {
                ItemStack item = res.ProductStacks.RemoveStack();

                foreach (ItemSlot containerSlot in res.Request.ContainerSourceSlots.Slots)
                {
                    //We need to loop again for stacks of buckets!
                    int stackCount = containerSlot.StackSize;
                    for (int i = 0; i < stackCount; i++)
                    {
                        ItemStack tool = containerSlot.Itemstack;
                        int moved = TransferToLiquidContainer(player, containerSlot, item);
                        if (moved > 0)
                            containerSlot.MarkDirty();

                        item.StackSize -= moved;

                        // Break the StackSize loop
                        if (item.StackSize <= 0)
                            break;
                    }

                    //Break the container Slot loop
                    if (item.StackSize <= 0)
                        break;

                }
            }

            res.Request.Api.World.PlaySoundAt(fillSound, player.Entity, player, true, 16f, 1f);
        }

        public static int TransferLiquidToItemStack(ItemStack containerStack, ItemStack liquidStacks)
        {
            Block block = containerStack?.Block;
            if (block == null)
            {
                return 0;
            }

            if (block is BlockLiquidContainerBase container)
            {

                if (container.GetCurrentLitres(containerStack) >= container.CapacityLitres)
                    return 0;

                int moved = container.TryPutLiquid(containerStack, liquidStacks, ConvertStackToLiters(liquidStacks));
                return moved;

            }
            return 0;
        }

        public static int TransferToLiquidContainer(IPlayer player, ItemSlot containerSlot, ItemStack liquidStacks)
        {
            ICoreAPI api = player.Entity.Api;
            if (containerSlot == null)
                return 0;

            if (liquidStacks == null)
                return 0;

            if (containerSlot.StackSize == 1)
            {
                return TransferLiquidToItemStack(containerSlot.Itemstack, liquidStacks);
            }
            else
            {
                ItemStack containerStack = containerSlot.TakeOut(1);
                int moved = TransferLiquidToItemStack(containerStack, liquidStacks);
                if (!player.InventoryManager.TryGiveItemstack(containerStack))
                {
                    api.World.SpawnItemEntity(containerStack, player.Entity.Pos.XYZ.AddCopy(0.5, 0.5, 0.5), null);
                }
                return moved;

            }
        }

        public static float ConvertStackToLiters(ItemStack stack)
        {
            WaterTightContainableProps contentProps = BlockLiquidContainerBase.GetContainableProps(stack);
            if (contentProps == null)
            {
                return 0;
            }
            return stack.StackSize / contentProps.ItemsPerLitre;
        }

        public static float ConvertStackToLiters(ItemStack stack, int amount)
        {
            WaterTightContainableProps contentProps = BlockLiquidContainerBase.GetContainableProps(stack);
            if (contentProps == null)
            {
                return 0;
            }
            return amount / contentProps.ItemsPerLitre;
        }

        public static int ConvertLitersToStack(ItemStack stack, float liters)
        {
            WaterTightContainableProps contentProps = BlockLiquidContainerBase.GetContainableProps(stack);
            if (contentProps == null)
            {
                return 0;
            }
            return (int)(liters * contentProps.ItemsPerLitre);
        }

        public static bool IsLiquidContainer(ItemStack stack)
        {
            return stack?.Block is BlockLiquidContainerBase container;
        }

        public static bool IsEmptyLiquidContainer(ItemStack stack)
        {
            return IsLiquidContainer(stack) && ((BlockLiquidContainerBase)stack.Block).GetCurrentLitres(stack) == 0;
        }

        public static bool CanHoldLiquid(IWorldAccessor world, ItemStack sourceStack, ItemStack targetStack)
        {
            if (sourceStack == null || targetStack == null)
                return false;

            BlockLiquidContainerBase container = sourceStack.Block as BlockLiquidContainerBase;
            if (container == null)
                return false;

            if (container.GetContent(sourceStack) == null)
                return true;

            if (container.GetCurrentLitres(sourceStack) >= container.CapacityLitres)
                return false;

            return targetStack.Equals(world, container.GetContent(sourceStack), GlobalConstants.IgnoredStackAttributes);
        }
    }
}
