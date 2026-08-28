#nullable enable
using Commercially.Common.Interfaces;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;

namespace Commercially.Common.Blocks.BlockEntityBehaviors
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
                            interactions.Add(new Interaction { Code = config.Code, Priority = config.Priority, Handler = interaction, Properties = config.Properties });
                        } else
                        {
                            modSystem.Mod.Logger.Error($"Failed to find interaction with code {config.Code} for block entity at {Blockentity.Pos}");
                        }
                    }
                }
            }
        }

        public Interaction? GetInteraction(string key, Caller caller, BlockSelection blockSel) {
            
            foreach (var interaction in interactions)
            {
                bool canHandle = interaction.Handler.CanHandle(Api.World, caller, Blockentity, blockSel, key);
                bool shouldHandle = interaction.Handler.ShouldHandle(Api.World, caller, Blockentity, blockSel, key);
                if ( canHandle && shouldHandle )
                {
                    return interaction;
                }
            }
            return null;
        }

        public virtual bool Interact(IWorldAccessor world, Caller caller, BlockSelection blockSel, string key = "default", ITreeAttribute? activationArgs = null) {
            Interaction? interaction = GetInteraction(key, caller, blockSel);
            if (interaction != null)
            {
                return interaction.Handler.Interact(world, caller, Blockentity, blockSel, key, interaction.GetProperties(), activationArgs);
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
                    worldInteractions.AddRange(interaction.Handler.GetInteractions(world, caller, blockEntity, blockSel, key, interaction.GetProperties(), activationArgs));
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
                    count += interaction.Handler.GetInteractionCount(world, caller, Blockentity, blockSel, key, interaction.GetProperties(), activationArgs);
                }
            }
            return count;
        }


    }

    public class InteractionConfig
    {
        public required string Code { get; set; }
        public int Priority { get; set; } = 0;
        private JsonObject? _properties;

        // We need 'object' so Newtonsoft parses the dynamic data safely without crashing:
        // (Newtonsoft.Json.JsonSerializationException: Cannot deserialize the current JSON object (e.g. {"name":"value"}) into type 'Vintagestory.API.Datastructures.JsonObject' because the type requires a JSON array (e.g.) to deserialize correctly.)
        public object? Properties
        {
            // Returns the raw internal JToken so Newtonsoft serializes your dynamic fields cleanly without wrapping them inside a "Token" object
            get => _properties?.Token;
            set
            {
                if (value is JToken token)
                {
                    _properties = new JsonObject(token);
                }
                else if (value is JsonObject vsObject)
                {
                    _properties = vsObject;
                }
                else if (value == null)
                {
                    _properties = null;
                }
            }
        }

        public virtual JsonObject? GetProperties()
        {
            return _properties;
        }
    }

    public class Interaction : InteractionConfig
    {
        public required IInteraction Handler { get; set; }

        //Helper Function.
        public bool Interact(IWorldAccessor world, Caller caller, BlockEntity blockEntity, BlockSelection blockSel, string key = "default", JsonObject? properties = null, ITreeAttribute? activationArgs = null)
        {
            return Handler?.Interact(world,caller,blockEntity,blockSel, key, properties, activationArgs) ?? false;
        }
    }
}
