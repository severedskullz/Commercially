using Vintagestory.API.Common;

namespace Commercially.Common.Delegates
{
    public delegate EnumWorldAccessResponse OnTestAccessDelegate(IPlayer player, BlockSelection blockSelection, EnumBlockAccessFlags accessType, string claimant, EnumWorldAccessResponse response);
}
