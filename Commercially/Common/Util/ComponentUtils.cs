using System;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;

namespace Commercially.Common.Util
{
    public static class ComponentUtils
    {
        public static T GetComponent<T>(this IComponent component) where T : class
        {
            if (component is BlockEntityBehavior blockEntityBehavior)
            {
                return blockEntityBehavior.Blockentity.GetBehavior<T>();
            }

            if (component is BlockEntity blockEntity)
            {
                return blockEntity.GetBehavior<T>();
            }

            return null;
        }

        public static BlockEntity GetBlockEntity(this IComponent component)
        {
            if (component is BlockEntityBehavior blockEntityBehavior)
            {
                return blockEntityBehavior.Blockentity;
            }

            if (component is BlockEntity blockEntity)
            {
                return blockEntity;
            }

            return null;
        }

        //TODO: Not really what I wanna require. Was hoping to use this in some sort of GetRequiredComponents() method where all we know is Type, but getting entity.Behaviour correctly casted OUT seems impossible
        public static IComponent GetComponent(this IComponent component, Type type)
        {
            BlockEntity entity = component.GetBlockEntity();
            for (int i = 0; i < entity.Behaviors.Count; i++)
            {
                Type beType = entity.Behaviors[i].GetType();

                if (beType.IsAssignableFrom(type))
                {
                    return entity.Behaviors[i] as IComponent;
                }
            }

            return null;
        }

        public static ICoreAPI GetApi(this IComponent component)
        {
            if (component is BlockEntityBehavior blockEntityBehavior)
            {
                return blockEntityBehavior.Api;
            }

            if (component is BlockEntity blockEntity)
            {
                return blockEntity.Api;
            }

            return null;
        }

        public static BlockPos GetPos(this IComponent component)
        {
            return component.GetBlockEntity().Pos;
        }
    }

    public interface IComponent
    {
    }
}
