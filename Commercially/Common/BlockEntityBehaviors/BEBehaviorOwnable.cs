using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;

namespace Commercially.Common.BlockEntityBehaviors
{
    public class BEBehaviorOwnable : BlockEntityBehavior, IOwnable
    {
        public string Name { get; set; }
        public string OwnableType { get; set; }
        public bool IsAdminOwned { get; set; }
        public string OwnerUID { get; set; }
        public string OwnerName { get; set; }
        public BlockEntity Entity => Blockentity;

        public BEBehaviorOwnable(BlockEntity blockentity) : base(blockentity)
        {

        }



        public override void Initialize(ICoreAPI api, JsonObject properties)
        {
            base.Initialize(api, properties);
            OwnableType = properties["ownableType"].AsString("generic");

        }

        public override void ToTreeAttributes(ITreeAttribute tree)
        {
            tree.SetString("OwnerUID", OwnerUID);
            tree.SetString("OwnerName", OwnerName);
            tree.SetBool("IsAdminOwned", IsAdminOwned);
            //tree.SetString("OwnableType", OwnableType);
        }

        public override void FromTreeAttributes(ITreeAttribute tree, IWorldAccessor world)
        {
            OwnerUID = tree.GetString("OwnerUID");
            OwnerName = tree.GetString("OwnerName");
            IsAdminOwned = tree.GetBool("IsAdminOwned");
            //OwnableType = tree.GetString("OwnableType");
        }

        public void SetOwner(IPlayer byPlayer)
        {
            OwnerUID = byPlayer.PlayerUID;
            OwnerName = byPlayer.PlayerName;
        }

        public void UpdateOwnership(string ownerUID, string ownerName, string name, bool isAdminOwned)
        {
            throw new System.NotImplementedException();
        }

        public override void OnBlockPlaced(ItemStack byItemStack = null)
        {
            base.OnBlockPlaced(byItemStack);
        }

        public override void OnBlockBroken(IPlayer byPlayer = null)
        {
            base.OnBlockBroken(byPlayer);
        }

        void IOwnable.SetOwner(IPlayer byPlayer)
        {
            throw new System.NotImplementedException();
        }

        void IOwnable.UpdateOwnership(string ownerUID, string ownerName, string name, bool isAdminOwned)
        {
            throw new System.NotImplementedException();
        }
    }
}
