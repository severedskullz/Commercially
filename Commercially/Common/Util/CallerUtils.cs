using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;

namespace Commercially.Common.Util
{

    /// <summary>
    /// A Helper class for converting players and entities to callers. Mostly future-proofing for if we want to be able to distinguish between Players, NPCs, and Cross-Server mechanics.
    /// This way, if we want to activate a block entity from an NPC or a cross-server event, we can easily convert that into a caller and not have to change any of the interaction code.
    /// </summary>
    public static class CallerUtils
    {
        public static Caller ToCaller(IPlayer player)
        {
            return new Caller()
            {
                Player = player
            };
        }

        public static Caller ToCaller(Entity entity)
        {
            return new Caller()
            {
                Entity = entity,
                Type = EnumCallerType.Entity
            };
        }
    }
}
