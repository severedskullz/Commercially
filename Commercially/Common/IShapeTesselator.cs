using Vintagestory.API.Client;
using Vintagestory.API.Common;

namespace Commercially.Common
{
    public interface IShapeTesselator : IComponent, ITexPositionSource
    {
        public void SetNowTesselatingObj(CollectibleObject collectible);
        public void SetNowTesselatingShape(Shape shape);

    }
}