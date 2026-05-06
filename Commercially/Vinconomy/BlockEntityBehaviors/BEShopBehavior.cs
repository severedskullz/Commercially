using Commercially.Common.BlockEntityBehaviors;
using Commercially.Common.Interfaces;
using Commercially.Common.Util;
using Commercially.Vinconomy.Interfaces;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;


namespace Commercially.Vinconomy.BlockEntityBehaviors
{
    /// <summary>
    /// A generic Shop.
    /// </summary>
    public class BEShopBehavior : BEBehaviorOwnableRoot, IShopComponent
    {
        public IShopInventoryProvider ShopInventoryProvider => _ShopInventoryProvider;
        IShopInventoryProvider _ShopInventoryProvider;

        public IOwnableRoot Ownable => _Ownable;
        IOwnableRoot _Ownable;

        public ItemSlot TradePass => _ShopInventoryProvider.TradePass;
        public ItemSlot[] CurrencySlots => _ShopInventoryProvider.CurrencySlots;
        public ItemSlot[] CouponSlots => _ShopInventoryProvider.CouponSlots;

        public BEShopBehavior(BlockEntity blockentity) : base(blockentity)
        {
           
        }

        public override void Initialize(ICoreAPI api, JsonObject properties)
        {
            base.Initialize(api, properties);
            _ShopInventoryProvider = this.GetComponent<IShopInventoryProvider>();
            _Ownable = this.GetComponent<IOwnableRoot>();
        }

        public override void OnBlockBroken(IPlayer byPlayer = null)
        {
            base.OnBlockBroken(byPlayer);
        }
    }
}
