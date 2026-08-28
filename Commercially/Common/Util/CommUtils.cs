using Commercially.Common.Interfaces;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Text;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;

namespace Commercially.Common.Util
{
    public class CommUtils
    {
        public static string SerializeToJson(object payload)
        {

            JsonSerializer serializer = new JsonSerializer();
            StringBuilder stringBuilder = new StringBuilder();
            using (var stringWriter = new StringWriter(stringBuilder))
            {
                serializer.Serialize(stringWriter, payload);
            }
            var jsonStr = stringBuilder.ToString();
            return jsonStr;
        }

        public static T DeserializeFromJson<T>(string payload)
        {
            JsonSerializer serializer = new JsonSerializer();
            using (var stringReader = new StringReader(payload))
            {
                using (var jsonReader = new JsonTextReader(stringReader))
                {
                    return serializer.Deserialize<T>(jsonReader);
                }
            }
        }

        public static ItemStack ResolveBlockOrItem(ICoreAPI api, string code, int size)
        {
            AssetLocation location = new AssetLocation(code);
            Item item = api.World.GetItem(location);
            if (item != null)
            {
                return new ItemStack(item, size);
            }

            Block block = api.World.GetBlock(location);
            if (block != null)
            {
                return new ItemStack(block, size);
            }
            return null;
        }

        public static ItemStack DeserializeProduct(ICoreAPI api, string code, int quantity, byte[] attributes)
        {
            if (code == null)
            {
                return null;
            }

            ItemStack productStack = ResolveBlockOrItem(api, code, quantity);

            if (productStack == null)
                return null;

            try
            {
                if (attributes != null)
                {
                    TreeAttribute attr = new TreeAttribute();
                    attr.FromBytes(attributes);

                    // Remove transition state from any food items. SQL entries are the last time it was inserted and isnt updated
                    attr.RemoveAttribute("transitionstate");

                    productStack.Attributes = attr;

                }
            }
            catch (Exception ex) { }
            return productStack;
        }

        public static bool ReadStreamBool(byte[] data)
        {
            bool value;
            using (MemoryStream ms = new MemoryStream(data))
            {
                BinaryReader reader = new BinaryReader(ms);
                value = reader.ReadBoolean();
            }
            return value;
        }

        public static string ReadStreamString(byte[] data)
        {
            string value;
            using (MemoryStream ms = new MemoryStream(data))
            {
                BinaryReader reader = new BinaryReader(ms);
                value = reader.ReadString();
            }
            return value;
        }

        public static int ReadStreamInt(byte[] data)
        {
            int value;
            using (MemoryStream ms = new MemoryStream(data))
            {
                BinaryReader reader = new BinaryReader(ms);
                value = reader.ReadInt32();
            }
            return value;
        }

        public static bool IsLocalPlayerOwner(BlockEntity blockEntity, ICoreClientAPI api)
        {
            if (blockEntity is IOwnable ownable)
            {
                return ownable.IsOwner(api.World.Player);
            }

            return blockEntity.GetBehavior<IOwnable>()?.IsOwner(api.World.Player) ?? false;
        }

        public static bool IsCreativePlayer(IPlayer player)
        {
            if (player == null) return false;

            return player.WorldData.CurrentGameMode == EnumGameMode.Creative && player.HasPrivilege("gamemode");
        }
    }
}
