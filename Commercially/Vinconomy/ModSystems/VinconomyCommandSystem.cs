using Commercially.Common;
using Commercially.Common.Blocks.BlockEntityBehaviors;
using Commercially.Common.Interfaces;
using Commercially.Common.Registry;
using Commercially.Common.Util;
using Commercially.Vinconomy.Interfaces;
using System;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;

namespace Commercially.Vinconomy.ModSystems
{
    public class VinconomyCommandSystem : ModSystem
    {
        private ICoreServerAPI _CoreServerAPI;

        public override void Start(ICoreAPI api)
        {
            base.Start(api);
        }

        public override void StartClientSide(ICoreClientAPI api)
        {
            base.StartClientSide(api);
        }

        public override void StartServerSide(ICoreServerAPI api)
        {
            _CoreServerAPI = api;
            var parsers = api.ChatCommands.Parsers;
            api.ChatCommands.GetOrCreate("vinconomy")
                .WithAlias("vincon")
                .RequiresPrivilege(Privilege.chat)
                .BeginSubCommand("setowner")
                    .RequiresPrivilege(Privilege.gamemode)
                    .WithDescription("Sets the Owner of the Stall or Register at the given position to the provided player.")
                    .WithArgs(parsers.Word("Player Name"), parsers.Int("Block X"), parsers.Int("Block Y"), parsers.Int("Block Z"))
                    .HandleWith(SetOwner)
                .EndSubCommand();
        }

        private TextCommandResult SetOwner(TextCommandCallingArgs args)
        {

            IServerPlayerData playerData = _CoreServerAPI.PlayerData.GetPlayerDataByLastKnownName((string)args[0]);

            if (playerData == null)
            {
                return TextCommandResult.Error("No player with that name.");
            }
            BlockPos pos = new BlockPos((int)args[1] + (_CoreServerAPI.WorldManager.MapSizeX / 2),
                (int)args[2],
                (int)args[3] + (_CoreServerAPI.WorldManager.MapSizeZ / 2),
                0);

            BlockEntity entity = _CoreServerAPI.World.BlockAccessor.GetBlockEntity(pos);
            if (entity == null)
            {
                return TextCommandResult.Error("Please target a Vinconomy Stall or Register. (BlockEntity)");
            }

            string playerUUID = playerData.PlayerUID;
            BEBehaviorOwnable ownableBehavior = entity.GetBehavior<BEBehaviorOwnable>();
            if (ownableBehavior == null)
            {
                return TextCommandResult.Error("Please target a Vinconomy Stall or Register. (No Ownable Behavior)");
            }

            ownableBehavior.SetOwner(playerUUID, playerData.LastKnownPlayername);
            IOwnableReference shopComponent = ownableBehavior.GetComponent<IOwnableReference>();
            if (shopComponent != null)
            {
                _CoreServerAPI.ModLoader.GetModSystem<CommerciallyModSystem>().UpdateOwnable(shopComponent);
            }
            else
            {
                return TextCommandResult.Error("Please target a Viconomy Stall or Register. (Not a Shop)");
            }

            return TextCommandResult.Success("Set owner to " + playerData.LastKnownPlayername + " for " + pos.ToString());
        }
    }
}
