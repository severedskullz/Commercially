using Commercially.Common;

namespace Commercially.Vinconomy.Interfaces
{
    public interface IShopComponent : IComponent
    {
        public IShopInventoryProvider ShopInventoryProvider { get; }
        public IOwnableRoot Ownable { get; }
    }
}