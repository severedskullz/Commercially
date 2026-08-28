#nullable enable
using Newtonsoft.Json.Linq;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;

namespace Commercially.Common.Interfaces
{
    public interface IInteraction
    {
        /// <summary>
        /// Returns the list of interactions associated with this IInteraction.
        /// </summary>
        public WorldInteraction[] GetInteractions(IWorldAccessor world, Caller caller, BlockEntity blockEntity, BlockSelection blockSel, string key = "default", JsonObject? properties = null, ITreeAttribute? activationArgs = null);

        /// <summary>
        /// Gets the interaction count to help determine the size of the array to be created in GetInteractions.
        /// </summary>
        public int GetInteractionCount(IWorldAccessor world, Caller caller, BlockEntity blockEntity, BlockSelection blockSel, string key = "default", JsonObject? properties = null, ITreeAttribute? activationArgs = null);

        /// <summary>
        /// Whether or not the current interaction can be handled. If true, GetInteractions should show the list of interactions that can be performed.
        /// </summary>
        public bool CanHandle(IWorldAccessor world, Caller caller, BlockEntity blockEntity, BlockSelection blockSel, string key = "default", JsonObject? properties = null, ITreeAttribute? activationArgs = null);

        /// <summary>
        /// Whether or not Interact should be called. This is independent of CanHandle which only determines if the interaction should be shown as an option.
        ///</summary>
        public bool ShouldHandle(IWorldAccessor world, Caller caller, BlockEntity blockEntity, BlockSelection blockSel, string key = "default", JsonObject? properties = null, ITreeAttribute? activationArgs = null);

        /// <summary>
        /// Runs the actual interaction. This should only happen if both CanHandle and ShouldHandle return true.
        /// </summary>
        public bool Interact(IWorldAccessor world, Caller caller, BlockEntity blockEntity, BlockSelection blockSel, string key = "default", JsonObject? properties = null, ITreeAttribute? activationArgs = null);
    }
}
