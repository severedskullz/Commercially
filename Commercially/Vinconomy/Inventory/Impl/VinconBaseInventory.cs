using Commercially.Common.Interfaces;
using Commercially.Common.Inventory;
using Commercially.Vinconomy.Interfaces;
using Commercially.Vinconomy.Inventory.StallSlots;
using System;
using System.Collections.Generic;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;
using Vintagestory.API.Util;

namespace Commercially.Vinconomy.Inventory
{
    public abstract class VinconBaseInventory : InventoryBase, ILateInitInventory, IStallStockUpdater
    {
        public BlockEntity BlockEntity { get; protected set; }
        public ItemSlot[] InternalSlots { get; protected set; }
        public StallSlotBase[] StallSlots { get; protected set; }
        public Type StallType { get; protected set; }
        public int SlotsPerStall {  get; protected set; }

        public bool IsInternalSlotsInitialized => InternalSlots != null;
        public bool IsSlotsInitialized => StallSlots != null;

        protected VinconomyModSystem modSystem;
        public event OnStockUpdatedDelegate OnStockUpdated;

        public IStallComponent StallComponent;


        public override int Count
        {
            get
            {
                int i = InternalSlots?.Length ?? 0;

                if (StallSlots == null)
                    return i;

                foreach (StallSlotBase stall in StallSlots)
                {
                    i += stall.TotalSlotCount;
                }

                return i;
            }
        }

        public VinconBaseInventory(BlockEntity entity,ICoreAPI api) : base("-", api)
        {
            //modSystem = Api.ModLoader.GetModSystem<VinconomyModSystem>();

            // A Non-Instantiated inventory. Will error out unless Initialize is called
            InitializeInternalSlots();

        }

        public VinconBaseInventory(string inventoryName, Type stallType, int numStalls, int slotsPerStall, ICoreAPI coreAPI) : base(inventoryName, coreAPI)
        {
            Initialize(stallType, numStalls, slotsPerStall, coreAPI);
        }


        public T GetStall<T>(int slot) where T : StallSlotBase
        {
            return (T)GetStall(slot);
        }

        public StallSlotBase GetStall(int slot)
        {
            if (slot >= StallSlots.Length || slot < 0)
            {
                throw new System.IndexOutOfRangeException($"Stall slot index {slot} is out of range for inventory {InventoryID} with {StallSlots.Length} stall slots.");
            }

            return StallSlots[slot];
        }

        protected Type GetStallType(string className)
        {
            /*
            Type type = Type.GetType(className);

            if (type == null)
            {
                throw new InvalidCastException($"Class {className} not found as a valid class for Stall Slot");
            }

            if (type != null && typeof(StallSlotBase).IsAssignableFrom(type))
            {
                return type;
            }
            else
            {
                throw new InvalidCastException("Stall Slot Type must be of StallSlotBase");
            }
            */
            return VinconomyModSystem.GetStallType(className);
        }

        protected StallSlotBase InstantiateStallType(Type type)
        {
            StallSlotBase instance = (StallSlotBase)Activator.CreateInstance(type);
            return instance;
        }

        /// <summary>
        /// Initialize the Internal Stall Slot array. This method should keep already existing arrays in tact if IsInternalSlotsInitialized is true (in this case, it was instantiated in FromTreeAttributes)
        /// otherwise it should initialize the array with empty item slots;
        /// </summary>
        public virtual void InitializeInternalSlots()
        {
            if (!IsInternalSlotsInitialized)
                InternalSlots = Array.Empty<ItemSlot>();
        }

        /*
        /// <summary>
        /// Converts a the specified stall index into the appropriate stall type. If stallTypes is provided, it will return that index's entry from the array, otherwise it will return stallType for all types.
        /// </summary>
        protected string GetStallTypeForStall(int stallIndex, string[] stallTypes, string stallType)
        {
            if (stallTypes == null)
            {
                return stallType;
            }

            return stallTypes[stallIndex];
        }
        */

        public void Initialize(string stallType, int numStalls, int numSlotsPerStall, ICoreAPI api)
        {
            Type type = GetStallType(stallType);
            Initialize(type, numStalls, numSlotsPerStall, api);
          
        }

        public void Initialize(Type stallType, int numStalls, int numSlotsPerStall, ICoreAPI api)
        {
            StallType = stallType;
            /*
             string[] stallTypes = properties["stallTypes"].AsArray<string>();
             if (stallTypes != null && stallTypes.Length != numStalls)
             {
                 throw new ArgumentException($"Number of stall types present in the array must match the length of numStalls of {numStalls}");
             }
             else if (stallTypes != null && stallType != null)
             {
                 throw new ArgumentException($"Choose either stallTypes or stallType - not both");
             }
             string curStallType = GetStallTypeForStall(0, stallTypes, stallType);
             */

            InitializeInternalSlots();
            InitializeStallSlots(stallType, numStalls, numSlotsPerStall);

            // This has bit us in the but more times than I can count, so lets triple check that InvNetworkUtil is initialized
            if (api != null && InvNetworkUtil == null)
            {
                InvNetworkUtil = api.ClassRegistry.CreateInvNetworkUtil(this, api);
            }

            //Lastly, be sure to resolve the collectible IDs into actual blocks/items 
            AfterBlocksLoaded(api.World);
        }

        
        public override void OnItemSlotModified(ItemSlot slot)
        {
            base.OnItemSlotModified(slot);
            this.OnStockModified(slot);
        }
        

        public void Initialize(JsonObject properties, string className, string instanceID, ICoreAPI api)
        {
            Api = api;
            this.instanceID = instanceID;
            this.className = className;
            modSystem = Api.ModLoader.GetModSystem<VinconomyModSystem>();

            int numStalls = properties["numStalls"].AsInt(4);
            int numSlotsPerStall = properties["numSlotsPerStall"].AsInt(16);
            string stallType = properties["stallType"].AsString("GenericStallSlot");

            Initialize(stallType, numStalls, numSlotsPerStall, api);
        }

        public override void ResolveBlocksOrItems()
        {
            int id = 0;
            using IEnumerator<ItemSlot> enumerator = GetEnumerator();
            while (enumerator.MoveNext())
            {
                ItemSlot current = enumerator.Current;
                if (current.Itemstack != null && !current.Itemstack.ResolveBlockOrItem(Api.World))
                {
                    current.Itemstack = null;
                }
                id++;
            }
        }

        /// <summary>
        /// Initialize the Stall Slot array. This method should keep already existing arrays in tact if IsSlotsInitialized is true (in this case, it was instantiated in FromTreeAttributes)
        /// otherwise it should initialize the array with empty item slots;
        /// </summary>
        public virtual void InitializeStallSlots(Type stallType, int numStalls, int numSlotsPerStall)
        {


            if (!IsSlotsInitialized)
            {
                StallSlots = new StallSlotBase[numStalls];
                for (int i = 0; i < numStalls; i++)
                {
                    StallSlotBase instance =  InstantiateStallType(stallType);
                    instance.Initialize(this, i, numSlotsPerStall);
                    StallSlots[i] = instance;
                }
            }

            // If we already have the slot array, make sure it is atleast greater than numSlots. I don't expect people to be resizing inventories mid-playthrough, but better safe than sorry
            // We *DO NOT* support shrinking inventories. In that case, the items are lost - so *DON'T DO IT*.
            else
            {
                if (StallSlots.Length < numStalls)
                {
                    int amount = numStalls - StallSlots.Length;
                    AddStallSlots(stallType, amount, numSlotsPerStall);
                }

                //TODO: Figure out how to grow individual stall's product slot size for numSlotsPerStall
            }
        }

        public void AddStallSlots(Type type, int amount, int numSlotsPerStall)
        {
            while (amount-- > 0)
            {
                StallSlotBase instance = InstantiateStallType(type);
                instance.Initialize(this, StallSlots.Length, numSlotsPerStall);

                //TODO: Inneffecient - Store to list and add the whole thing in one pass instead of allocating new array and copying over each time
                StallSlots = StallSlots.Append(instance);
            }
        }

        /// <summary>
        /// Retrieves the item slot corresponding to the specified global slot index.
        /// </summary>
        /// <remarks>The global index spans both internal slots and all stall slots in order. Use this
        /// method to access an item slot without needing to know whether it is in the internal or stall
        /// collection.</remarks>
        /// <param name="index">The zero-based global index of the item slot to retrieve. Must be within the range of available slots.</param>
        /// <returns>The item slot at the specified global index.</returns>
        /// <exception cref="IndexOutOfRangeException">Thrown if the specified index is less than zero or greater than or equal to the total number of slots.</exception>
        //TODO: Im sure this can be done better for stalls of equal size, but I do want to support variable sized stalls at some point in the future.
        //If thats the case, then this becomes entirely neccesary
        public ItemSlot GetItemSlotFromID(int index)
        {
            int i = index;
            if (i < InternalSlots.Length)
            {
                return InternalSlots[i];
            }
            else
            {
                i -= InternalSlots.Length;
            }

            foreach (StallSlotBase stall in StallSlots)
            {
                if (i < stall.TotalSlotCount)
                {
                    return stall[i];
                }
                else
                {
                    i -= stall.TotalSlotCount;
                }
            }

            throw new IndexOutOfRangeException($"Index {index} out of bounds for stall. It only has {this.Count} total slots");

        }
        public override ItemSlot this[int slotId] {
            get { return GetItemSlotFromID(slotId); }
            set {
                int i = slotId;
                if (i < InternalSlots.Length)
                {
                    InternalSlots[i] = value;
                }
                else
                {
                    i -= InternalSlots.Length;
                }

                foreach (StallSlotBase stall in StallSlots)
                {
                    if (i < stall.TotalSlotCount)
                    {
                        stall[i] = value;
                    }
                    else
                    {
                        i -= stall.TotalSlotCount;
                    }
                }

                throw new IndexOutOfRangeException($"Index {slotId} out of bounds for stall. It only has {this.Count} total slots");
            }
        }

        public override void FromTreeAttributes(ITreeAttribute tree)
        {
            int numStalls = tree.GetInt("numStalls");
            if (!IsSlotsInitialized)
            {

                SlotsPerStall = tree.GetInt("numSlotsPerStall", 9);
                StallType = GetStallType(tree.GetString("stallType", "GenericStallSlot"));

                StallSlots = new StallSlotBase[numStalls];
                for (int i = 0; i < numStalls; i++)
                {
                    StallSlotBase stall = InstantiateStallType(StallType);
                    ITreeAttribute stallTree = tree.GetOrAddTreeAttribute("stall" + i);
                    stall.PreInitialize(this, i);
                    stall.FromTreeAttributes(stallTree);
                    StallSlots[i] = stall;
                }

                //TODO: How to handle resizing of internal slots? Is this even supported?
                ITreeAttribute internalSlots = tree.GetOrAddTreeAttribute("internalSlots");
                //int numInternalSlots = internalSlots.GetInt("numSlots",0);
                for (int i = 0; i < InternalSlots.Length; i++)
                {
                    ItemStack stack = internalSlots.GetItemstack("slot" + i);
                    InternalSlots[i].Itemstack = stack;

                    if (Api?.World == null)
                    {
                        continue;
                    }

                    stack?.ResolveBlockOrItem(Api.World);
                }
            } else
            {
                for (int i = 0; i < numStalls; i++)
                {
                    StallSlotBase stall = GetStall(i);
                    ITreeAttribute stallTree = tree.GetOrAddTreeAttribute("stall" + i);
                    stall.FromTreeAttributes(stallTree);
                }

                ITreeAttribute internalSlots = tree.GetOrAddTreeAttribute("internalSlots");
                for (int i = 0; i < InternalSlots.Length; i++)
                {
                    ItemStack stack = internalSlots.GetItemstack("slot" + i);
                    InternalSlots[i].Itemstack = stack;

                    if (Api?.World == null)
                    {
                        continue;
                    }

                    stack?.ResolveBlockOrItem(Api.World);
                }
            }

        }

        public override void ToTreeAttributes(ITreeAttribute tree)
        {
            tree.SetInt("numStalls", StallSlots.Length);
            tree.SetString("stallType", StallType?.Name);
            tree.SetInt("numSlotsPerStall", SlotsPerStall);

            for (int i = 0; i < StallSlots.Length; i++)
            {
                ITreeAttribute stallTree = tree.GetOrAddTreeAttribute("stall" + i);
                StallSlots[i].ToTreeAttributes(stallTree);
            }

            ITreeAttribute internalSlots = tree.GetOrAddTreeAttribute("internalSlots");
            //internalSlots.SetInt("numSlots", InternalSlots.Length);
            for (int i = 0; i < InternalSlots.Length; i++)
            {
                internalSlots.SetItemstack("slot" + i, InternalSlots[i].Itemstack);
                internalSlots.SetString("slot" + i + "-name", InternalSlots[i].Itemstack?.ToString());
            }
        }

        public void OnStockModified(ItemSlot slot)
        {
            if (Api.Side == EnumAppSide.Client) return;

            if (slot is IStallProductSlot stallProductSlot)
            {
                int stallSlot = stallProductSlot.GetStall();
                StallSlotBase stall = this.GetStall(stallSlot);
                ItemStack product = stall.Product?.Itemstack?.Clone();
                ItemStack currency = stall.Currency?.Itemstack?.Clone();
                int stockCount = stall.GetProducts().TotalCount;

                OnStockUpdated?.Invoke(StallComponent, stallSlot, product, stockCount, currency);
            }
        }

        public void UpdateStockForSlot(IStallComponent shop, int stallSlot, ItemStack product, int stockCount, ItemStack currency)
        {
            // Really strange way of doing this, I know. I wanted to keep the inventory decoupled from the block entity as much as possible
            // This was the simplest way I could think of without having to pass a reference to the BE into each inventory instance.

            // TODO: This is now redundant, as the BE is now passed into the inventory on to check if the stall is an admin shop or not... Whoops! Remove this and just call modSystem.UpdateStockForSlot directly from the BE
            // Do I even need that event anymore? would it be useful to have a generic event for when stock is updated? Could be useful for other mods to hook into
            modSystem.UpdateStockForSlot(shop, stallSlot, product, stockCount, currency);
        }

        public override float GetTransitionSpeedMul(EnumTransitionType transType, ItemStack stack)
        {
            // Clones should never transition.
            // Tyron, it would be fucking GREAT if you gave us the ItemSlot instead!!! That way I can check the class, damnit!
            foreach (StallSlotBase stall in StallSlots)
            {
                if (stall.Currency.Itemstack == stack || stall.Product.Itemstack == stack) return 0;
            }

            VinconomyConfig config = modSystem.Config;
            bool fooldDecaysInShops = config?.FoodDecaysInShops ?? false;
            bool isAdminOwned = StallComponent?.Ownable?.IsAdminOwned ?? false;
            if (fooldDecaysInShops && !isAdminOwned)
            {
                return base.GetDefaultTransitionSpeedMul(transType) * modSystem.Config.StallPerishRate;
            }
            else
            {
                return 0;
            }

        }
    }
}
