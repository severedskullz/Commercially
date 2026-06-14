#nullable enable
using Commercially.Common.Interfaces;
using System.Collections.Generic;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;

namespace Commercially.Common.BlockEntityBehaviors
{
    public class InteractionManager : BlockEntityBehavior, IInteractionManager
    {
        //TODO: Sorted List does not allow duplicated keys for some stupid reason, need to find a better way to sort interactions by priority. Dont want to implement my own sorted list just for this
        //private readonly SortedList<int, IInteraction> interactions = [];
        private readonly List<Interaction> interactions = new List<Interaction>();


        public InteractionManager(BlockEntity blockentity) : base(blockentity)
        {

        }

        public override void Initialize(ICoreAPI api, JsonObject properties)
        {
            base.Initialize(api, properties);
            CommerciallyModSystem modSystem = api.ModLoader.GetModSystem<CommerciallyModSystem>();

            InteractionConfig?[]? interactionConfigs = properties["interactions"]?.AsArray<InteractionConfig>();
            if (interactionConfigs != null)
            {
                foreach (InteractionConfig? config in interactionConfigs)
                {
                    if (config != null)
                    {
                        IInteraction? interaction = modSystem.GetInteraction(config.Code);
                        if (interaction != null)
                        {
                            interactions.Add(new Interaction { Code = config.Code, Priority = config.Priority, Handler = interaction });
                        } else
                        {
                            modSystem.Mod.Logger.Error($"Failed to find interaction with code {config.Code} for block entity at {Blockentity.Pos}");
                        }
                    }
                }
            }
        }

        public IInteraction? GetInteraction(string key, Caller caller, BlockSelection blockSel) {
            
            foreach (var interaction in interactions)
            {
                bool canHandle = interaction.Handler.CanHandle(Api.World, caller, Blockentity, blockSel, key);
                bool shouldHandle = interaction.Handler.ShouldHandle(Api.World, caller, Blockentity, blockSel, key);
                if ( canHandle && shouldHandle )
                {
                    return interaction.Handler;
                }
            }
            return null;
        }

        public virtual bool Interact(IWorldAccessor world, Caller caller, BlockSelection blockSel, string key = "default", ITreeAttribute? activationArgs = null) {
            IInteraction? interaction = GetInteraction(key, caller, blockSel);
            if (interaction != null)
            {
                return interaction.Interact(world, caller, Blockentity, blockSel, key, activationArgs);
            }
            return false;
        }

        public WorldInteraction[] GetInteractions(IWorldAccessor world, Caller caller, BlockEntity blockEntity, BlockSelection blockSel, string key = "default", ITreeAttribute? activationArgs = null)
        {
            int count = GetInteractionCount(world, caller, blockEntity, blockSel, key, activationArgs);
            List<WorldInteraction> worldInteractions = new List<WorldInteraction>(count);

            foreach (var interaction in interactions)
            {
                if (interaction.Handler.CanHandle(Api.World, caller, Blockentity, blockSel, key)) {
                    worldInteractions.AddRange(interaction.Handler.GetInteractions(world, caller, blockEntity, blockSel, key, activationArgs));
                }
            }
            return worldInteractions.ToArray();
        }

        public int GetInteractionCount(IWorldAccessor world, Caller caller, BlockEntity blockEntity, BlockSelection blockSel, string key = "default", ITreeAttribute? activationArgs = null)
        {
            int count = 0;
            foreach (var interaction in interactions)
            {
                if (interaction.Handler.CanHandle(Api.World, caller, Blockentity, blockSel, key))
                {
                    count += interaction.Handler.GetInteractionCount(world, caller, Blockentity, blockSel, key, activationArgs);
                }
            }
            return count;
        }
    }

    public class InteractionConfig
    {
        public required string Code { get; set; }
        public int Priority { get; set; } = 0;
    }

    public class Interaction : InteractionConfig
    {
        public required IInteraction Handler { get; set; }
    }
}
