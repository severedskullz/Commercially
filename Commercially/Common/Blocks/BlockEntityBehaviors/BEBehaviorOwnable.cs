using Commercially.Common.Interfaces;
using System.IO;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Datastructures;
using Vintagestory.API.Server;

namespace Commercially.Common.Blocks.BlockEntityBehaviors
{
    public class BEBehaviorOwnable : BlockEntityBehavior, IOwnable
    {
        public string Name { get; set; }
        public string OwnableType { get; set; }
        public bool IsAdminOwned { get; set; }
        public string OwnerUID { get; set; }
        public string OwnerName { get; set; }
        public BlockEntity Entity => Blockentity;
        protected CommerciallyModSystem ModSystem;

        public BEBehaviorOwnable(BlockEntity blockentity) : base(blockentity)
        {

        }



        public override void Initialize(ICoreAPI api, JsonObject properties)
        {
            base.Initialize(api, properties);
            OwnableType = properties["ownableType"].AsString("Generic");

            ModSystem = api.ModLoader.GetModSystem<CommerciallyModSystem>();

        }

        public override void ToTreeAttributes(ITreeAttribute tree)
        {
            tree.SetString("OwnerUID", OwnerUID);
            tree.SetString("OwnerName", OwnerName);
            tree.SetBool("IsAdminOwned", IsAdminOwned);
            tree.SetString("OwnableType", OwnableType);
            tree.SetString("Name", Name);
        }

        public override void FromTreeAttributes(ITreeAttribute tree, IWorldAccessor world)
        {
            OwnerUID = tree.GetString("OwnerUID");
            OwnerName = tree.GetString("OwnerName");
            IsAdminOwned = tree.GetBool("IsAdminOwned");
            OwnableType = tree.GetString("OwnableType");
            Name = tree.GetString("Name");
        }

        public void SetOwner(IPlayer byPlayer)
        {
            OwnerUID = byPlayer.PlayerUID;
            OwnerName = byPlayer.PlayerName;
            this.Entity.MarkDirty();
        }
        public void SetOwner(string ownerUID, string ownerName)
        {
            OwnerUID = ownerUID;
            OwnerName = ownerName;
            this.Entity.MarkDirty();
        }
        public void SetIsAdminOwned(bool isAdminOwned)
        {
            this.IsAdminOwned = isAdminOwned;
            this.Entity.MarkDirty();
        }

        public void SetName(string name)
        {
            this.Name = name;
            this.Entity.MarkDirty();
        }

        public virtual void UpdateOwnership(string ownerUID, string ownerName, string name, bool isAdminOwned)
        {
            SetOwner(ownerUID, ownerName);
            SetName(name);
            SetIsAdminOwned(isAdminOwned);
        }


        public override void OnBlockPlaced(ItemStack byItemStack = null)
        {
            base.OnBlockPlaced(byItemStack);
        }

        public override void OnBlockBroken(IPlayer byPlayer = null)
        {
            base.OnBlockBroken(byPlayer);
        }

        public bool IsOwner(IPlayer byPlayer)
        {
            return this.OwnerUID == byPlayer.PlayerUID;
        }



        public override void OnReceivedClientPacket(IPlayer player, int packetid, byte[] data)
        {
            switch (packetid)
            {
                case CommerciallyConstants.SET_NAME:
                    using (MemoryStream ms = new MemoryStream(data))
                    {

                        if (player.PlayerUID == OwnerUID)
                        {
                            BinaryReader reader = new BinaryReader(ms);
                            string name = reader.ReadString();
                            SetName(name);
                            UpdateOwnership(OwnerUID, OwnerName, Name, IsAdminOwned);
                        }
                        else
                        {
                            ((IServerPlayer)player).SendMessage(0, Lang.Get("commercially:doesnt-own", []), EnumChatType.OwnMessage);
                        }
                    }
                    break;
                case CommerciallyConstants.SET_ADMIN_OWNED:
                    using (MemoryStream ms = new MemoryStream(data))
                    {

                        if (player.PlayerUID == OwnerUID)
                        {
                            BinaryReader reader = new BinaryReader(ms);
                            IsAdminOwned = reader.ReadBoolean();
                            UpdateOwnership(OwnerUID, OwnerName, Name, IsAdminOwned);
                        }
                        else
                        {
                            ((IServerPlayer)player).SendMessage(0, Lang.Get("commercially:doesnt-own", []), EnumChatType.OwnMessage);
                        }
                    }
                    break;
                default:
                    break;
            }
        }
    }
}
