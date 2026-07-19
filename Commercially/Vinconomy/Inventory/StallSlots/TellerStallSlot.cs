using Commercially.Common.Inventory.Slots;
using Commercially.Vinconomy.Trading;
using System;
using Vinconomy.Inventory.Slots;
using Vintagestory.API.Common;

namespace Commercially.Vinconomy.Inventory.StallSlots
{
    public class TellerStallSlot : StallSlotBase
    {
        public TellerStallSlot(InventoryBase inventory, int stallSlot) : base(inventory, stallSlot)
        {
        }

        public override ItemSlot this[int slotId] { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public override bool IsInitialized => Currency != null && Product != null;

        public override void ExtractProductFromStall(TradeResult result)
        {
            throw new NotImplementedException();
        }

        public override AggregatedSlots GetProducts()
        {
            return new AggregatedSlots(Inventory?.Api);
        }

        public override ItemSlot GetProductSlot(int itemSlot)
        {
            return null;
        }

        public override ItemSlot[] GetProductSlots()
        {
            return [];
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
                Currency = new VinconCloningSlot(inventory);
                Product = new VinconCloningSlot(inventory);
            }
        }

        public override void TransferProdutToPlayer(TradeResult result)
        {
            throw new NotImplementedException();
        }
    }
}
