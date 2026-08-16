using Commercially.Common.Util;
using System.Collections.Generic;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;

namespace Commercially.Common.Interfaces
{
    /// <summary>
    /// Represents an object that can be owned by a player or admin. This is used for shops, stalls, and potentially other objects in the future. It provides a common interface for checking ownership and retrieving owner information.
    /// </summary>
    public interface IOwnable : IBlockEntityComponent
    {
        /// <summary>
        /// Gets or sets the name associated with this instance.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// The type of Ownable this object is
        /// </summary>
        public string OwnableType { get; }

        /// <summary>
        /// Whether or not this Ownable is owned by an admin. This usually means that it should have infinite goods for sale, never break, or otherwise always function.
        /// </summary>
        public bool IsAdminOwned { get; }

        /// <summary>
        /// The UID of the player that owns this Ownable. This is null if the Ownable is admin owned or unowned. This is used to check ownership and retrieve owner information.
        /// </summary>
        public string OwnerUID { get; }

        /// <summary>
        /// The Name of the player that owns this Ownable. This is null if the Ownable is admin owned or unowned. This is used for display purposes and should not be used for ownership checks, as players can change their names.
        /// </summary>
        public string OwnerName { get; }

        void SetOwner(IPlayer byPlayer);

        bool IsOwner(IPlayer byPlayer);

        void SetIsAdminOwned(bool isAdminOwned);

        void SetName(string name);

        public void UpdateOwnership(string ownerUID, string ownerName, string name, bool isAdminOwned);

        public static bool IsCreativePlayer(IPlayer player)
        {
            return player.WorldData.CurrentGameMode == EnumGameMode.Creative && player.HasPrivilege("gamemode");
        }
    }

    /// <summary>
    /// Represents a reference to an ownable object that can be identified by a unique ID.
    /// </summary>
    /// <remarks>Implementations of this interface enable hierarchical ownership structures, where each object
    /// can be referenced by its unique identifier. This is useful for scenarios such as modeling parent-child
    /// relationships (for example, a shop with multiple stalls).</remarks>
    public interface IOwnableReference : IOwnable
    {
        /// <summary>
        /// The ID of the Ownable object. This non-null ID is referenced by IOwnableRoot via the this ID property and by IOwnableLeaf via the ParentID property. 
        /// This allows us to have multiple levels of ownership, such as a shop (root) with multiple stalls (children), and potentially more levels in the future.
        /// This also allows us to have independent Ownable objects that are not part of a hierarchy, as they can simply have no children and/or no parent but still be persisted/referenced by other nodes.
        /// </summary>
        public long ID { get; }

        public BlockPos Position { get; }

        /// <summary>
        /// Sets the ID of this Ownable. This should only be used by the system when creating a new Ownable and assigning it an ID from the database. It should not be used by external code, as it can cause issues with ownership references if changed after creation.
        /// </summary>
        /// <param name="lastId"></param>
        public void SetIDInternal(long lastId);
    }

    public interface IOwnableRoot : IOwnableReference
    {
        /// <summary>
        /// Gets all children of this Ownable
        /// </summary>
        List<IOwnable> GetChildren();
    }

    public interface IOwnableChild : IOwnableReference
    {
        /// <summary>
        /// The Owning Ownable's ID
        /// </summary>
        public long? ParentID { get;}

        public bool HasParent() { return ParentID.HasValue; }

        /// <summary>
        /// The Parent's OwnableType
        /// </summary>
        public string ParentType { get; }

        /// <summary>
        /// Retrieves the parent Ownable
        /// </summary>
        public IOwnable GetParent();

        /// <summary>
        /// Gets the allowed parent types that can be assigned to the Parent of this ownable.
        /// </summary>
        public string[] GetAllowedParentTypes();


        /// <summary>
        /// Sets the owning root for this Ownable
        /// </summary>
        public void SetParent(IOwnableRoot root);
        public void SetParent(long parentId);
    }

    public interface IOwnableNode : IOwnableRoot, IOwnableChild
    {
        // Nothing to do here. Its a Root and a Leaf, so it has an ID and a ParentID. It can have children, but it also has a parent.
        // This is useful for things that require more than 2 levels of ownership, such as a Nation (root) having multiple Cities (nodes) that then can have multiple Villages (leaves).
    }
}
