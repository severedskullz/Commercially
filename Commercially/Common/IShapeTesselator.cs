using Commercially.Common.Util;
using Vintagestory.API.Client;
using Vintagestory.API.Common;

namespace Commercially.Common
{
    public interface IShapeTesselator : IComponent, ITexPositionSource
    {
        // I still have yet to figure out why vanilla code has these variables which dont seem to be used at all.
        public void SetNowTesselatingObj(CollectibleObject collectible);
        public void SetNowTesselatingShape(Shape shape);

    }
}