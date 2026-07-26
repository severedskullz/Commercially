using System;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;
using Vintagestory.GameContent;

namespace Vinconomy.Filters
{
    public class CommonFilters
    {
        public static bool IsHeadDressType(ItemSlot slot)
        {
            return IsDressType(slot, EnumCharacterDressType.Head);
        }
        public static bool IsShoulderDressType(ItemSlot slot)
        {
            return IsDressType(slot, EnumCharacterDressType.Shoulder);
        }

        public static bool IsUpperBodyDressType(ItemSlot slot)
        {
            return IsDressType(slot, EnumCharacterDressType.UpperBody);
        }

        public static bool IsUpperBodyOverDressType(ItemSlot slot)
        {
            return IsDressType(slot, EnumCharacterDressType.UpperBodyOver);
        }

        public static bool IsLowerBodyDressType(ItemSlot slot)
        {
            return IsDressType(slot, EnumCharacterDressType.LowerBody);
        }

        public static bool IsFootDressType(ItemSlot slot)
        {
            return IsDressType(slot, EnumCharacterDressType.Foot);
        }

        public static bool IsNeckDressType(ItemSlot slot)
        {
            return IsDressType(slot, EnumCharacterDressType.Neck);
        }

        public static bool IsEmblemDressType(ItemSlot slot)
        {
            return IsDressType(slot, EnumCharacterDressType.Emblem);
        }

        public static bool IsFaceDressType(ItemSlot slot)
        {
            return IsDressType(slot, EnumCharacterDressType.Face);
        }

        public static bool IsArmDressType(ItemSlot slot)
        {
            return IsDressType(slot, EnumCharacterDressType.Arm);
        }

        public static bool IsHandDressType(ItemSlot slot)
        {
            return IsDressType(slot, EnumCharacterDressType.Hand);
        }

        public static bool IsWaistDressType(ItemSlot slot)
        {
            return IsDressType(slot, EnumCharacterDressType.Waist);
        }

        public static bool IsArmorHeadDressType(ItemSlot slot)
        {
            return IsDressType(slot, EnumCharacterDressType.ArmorHead);
        }

        public static bool IsArmorBodyDressType(ItemSlot slot)
        {
            return IsDressType(slot, EnumCharacterDressType.ArmorBody);
        }

        public static bool IsArmorLegsDressType(ItemSlot slot)
        {
            return IsDressType(slot, EnumCharacterDressType.ArmorLegs);
        }

        public static bool IsDressType(ItemSlot slot, params EnumCharacterDressType[] dressTypes)
        {
            if (slot == null || slot.Itemstack == null)
                return false;

            JsonObject attr = slot.Itemstack.Collectible.Attributes;
            if (attr == null)
            {
                return false;
            }

            string stackDressType = attr["clothescategory"].AsString(null);
            if (stackDressType == null)
                return false;

            foreach (var dressType in dressTypes)
            {
                if (dressType.ToString().Equals(stackDressType, StringComparison.InvariantCultureIgnoreCase))
                    return true;
            }

            return false;

        }

        public static bool IsDressType(ItemSlot slot, EnumCharacterDressType dressType)
        {
            if (slot == null || slot.Itemstack == null)
                return false;

            JsonObject attr = slot.Itemstack.Collectible.Attributes;
            if (attr == null)
            {
                return false;
            }

            string stackDressType = attr["clothescategory"].AsString(null);
            if (stackDressType == null)
                return false;


            return dressType.ToString().Equals(stackDressType, StringComparison.InvariantCultureIgnoreCase);
        }

        public static bool IsDressType(ItemSlot slot, params string[] dressTypes)
        {
            if (slot == null || slot.Itemstack == null)
                return false;

            JsonObject attr = slot.Itemstack.Collectible.Attributes;
            if (attr == null)
            {
                return false;
            }

            string stackDressType = attr["clothescategory"].AsString(null);
            if (stackDressType == null)
                return false;

            foreach ( var dressType in dressTypes)
            {
                if (dressType.Equals(stackDressType, StringComparison.InvariantCultureIgnoreCase))
                    return true;
            }

            return false;
        }

        public static bool IsClothingOrArmor(ItemSlot slot)
        {
            if (slot == null || slot.Itemstack == null)
                return false;

            JsonObject attr = slot.Itemstack.Collectible.Attributes;
            if (attr == null)
            {
                return false;
            }

            return attr["clothescategory"].AsString(null) != null;
        }

        public static bool IsToolOrWeapon(ItemSlot slot)
        {
            if (slot == null || slot.Itemstack == null)
                return false;

            // Shields seem to have toolrackTransforms now for some reason
            if (IsShield(slot))
                return false;

            return slot.Itemstack.Item?.Attributes?.KeyExists("toolrackTransform") == true 
                || slot.Itemstack.Item?.Code.Path.StartsWith("bugnet") == true
                || slot.Itemstack.Item?.Code.Path.StartsWith("arrow-") == true;
        }

        public static bool IsShield(ItemSlot slot)
        {
            if (slot == null || slot.Itemstack == null)
                return false;

            return slot.Itemstack.Item?.Code.Path.StartsWith("shield") == true;
        }

        public static bool IsGenericItem(ItemSlot slot)
        {
            if (slot == null || slot.Itemstack == null)
                return false;

            bool isToolOrWeapon = IsToolOrWeapon(slot);
            bool isClothingOrArmor = IsClothingOrArmor(slot);
            bool isShield = IsShield(slot);


            return IsFaceDressType(slot) || (!isToolOrWeapon && !isClothingOrArmor && !isShield); 
        }

        public static bool IsEmptyGachaSlot(ItemSlot slot)
        {
            return slot.Itemstack.Item?.Code.Path == "gachaball" && slot.Itemstack.Attributes.GetTreeAttribute("Contents") == null;
        }

        public static bool IsFilledGachaSlot(ItemSlot slot)
        {
            return slot.Itemstack.Item?.Code.Path == "gachaball" && slot.Itemstack.Attributes.GetTreeAttribute("Contents") != null;
        }

        public static bool IsMicroblock(ItemSlot slot)
        {
            return slot.Itemstack?.Block is BlockMicroBlock;
        }

        public static bool IsBlock(ItemSlot slot)
        {
            return slot.Itemstack?.Block != null;
        }

        public static bool IsFoodContainer(ItemSlot slot)
        {
            ItemStack stack = slot.Itemstack;
            if (stack == null) return false;

            return stack.Block is IBlockMealContainer || stack.Block is BlockCookingContainer; // || (stack.Block is BlockContainer container && stack.Block?.Attributes["mealContainer"]?.AsBool() == true);
        }
    }
}
