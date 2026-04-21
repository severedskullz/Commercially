using Vintagestory.API.Common;

namespace Commercially.Common.Interfaces
{
    /// <summary>
    /// Mostly just a compiler convienience interface for when we need to get the BlockEntity off of one of the Behaviors. A BlockEntityBehavior will always have a Blockentity variable, but since our behaviors implement
    /// the various interfaces and not the other way around, we need a way to reference that implemented Behavior's Blockentity variable. This was the simplest and least intrusive solution I could come up with.
    /// 
    /// This way I dont have to check what class the given interface instance is, cast it, then retrieve the block entity from the BEBehavior instance itself. In most cases I just implement it as
    /// "public BlockEntity Entity => Blockentity;"
    /// and then we have an external reference to itself without knowing what class the behavior is and without any casting needed.
    /// </summary>
    public interface IEntityProvider
    {
        BlockEntity Entity { get; }
    }

    public interface IBlockEntityProvider
    {
        BlockEntity Blockentity { get; }
    }
}
