using Commercially.Vinconomy.Inventory.StallSlots;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;
using Vintagestory.GameContent;

namespace Commercially.Vinconomy.BlockEntityBehaviors.DisplayProviders
{
    public class DisplayMealContentsBehavior : BaseDisplayContentsBehavior
    {


        public DisplayMealContentsBehavior(BlockEntity blockentity) : base(blockentity)
        {
        }

        public override void Initialize(ICoreAPI api, JsonObject properties)
        {
            base.Initialize(api, properties);
        }


        public override bool OnTesselation(ITerrainMeshPool mesher, ITesselatorAPI tessThreadTesselator)
        {
            TesselateDisplayedItems(mesher, tessThreadTesselator);
            return false;
        }

        protected override MeshData GenMesh(ItemSlot stack, int stallSlot)
        {
            MealMeshCache mesher = (this.Api as ICoreClientAPI).ModLoader.GetModSystem<MealMeshCache>(true);
            Block block = stack.Itemstack?.Block;
            MealStallSlot mealStall = _InventoryProvider.GetStallSlot<MealStallSlot>(stallSlot);
            string recipe = mealStall.GetRecipeCode();
            ItemStack[] contents = mealStall.GetProductContents();

            CookingRecipe fromRecipe = Api.GetCookingRecipe(recipe);
            return mesher.GenMealMesh(fromRecipe, contents);

            /*
            if (this.ownBlock == null)
            {
                return null;
            }
            ItemStack[] stacks = base.GetNonEmptyContentStacks(true);
            if (stacks == null || stacks.Length == 0)
            {
                return null;
            }
            return (this.Api as ICoreClientAPI).ModLoader.GetModSystem<MealMeshCache>(true).GenMealInContainerMesh(block, this.FromRecipe, stacks, null);*/
        }
    }
}
