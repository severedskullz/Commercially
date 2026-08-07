using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;

namespace Commercially.Common.Inventory
{
    public class ConfigurableInventory : InventoryGeneric, ILateInitInventory
    {
        public ConfigurableInventory(ICoreAPI api) : base(api)
        {

        }

        //Is this really so hard to check for?? Protected "slots" and no acessor provided?
        public bool IsSlotsInitialized => slots != null;

        public override void FromTreeAttributes(ITreeAttribute treeAttribute)
        {
            int num = IsSlotsInitialized ? slots.Length : 0;
            slots = SlotsFromTreeAttributes(treeAttribute, slots);
            int amount = num - slots.Length;
            AddSlots(amount);
        }

        public void InitializeFromProperties(JsonObject properties, string className, string instanceID, ICoreAPI api)
        {
            int numSlots = properties["numSlots"].AsInt(9);
            this.instanceID = instanceID;
            this.className = className;
            Api = api;

            // SlotsFromTreeAttributes actually instantiates "slots" so to avoid overwriting the array, check if they are already set
            if (!IsSlotsInitialized)
            {
                slots = new ItemSlot[numSlots];
                for (int i = 0; i < numSlots; i++)
                {
                    slots[i] = new ItemSlot(this);
                }
            }

            //If we already have the slot array, make sure it is atleast greater than numSlots. I don't expect people to be resizing inventories mid-playthrough, but better safe than sorry
            else
            {
                if (slots.Length < numSlots)
                {
                    int amount = numSlots - slots.Length;
                    AddSlots(amount);
                }
            }

            // This has bit us in the but more times than I can count, so lets triple check that InvNetworkUtil is initialized
            if (api != null && InvNetworkUtil == null)
            {
                InvNetworkUtil = api.ClassRegistry.CreateInvNetworkUtil(this, api);
            }

            //Lastly, be sure to resolve the collectible IDs into actual blocks/items 
            AfterBlocksLoaded(api.World);
        }
    }
}
