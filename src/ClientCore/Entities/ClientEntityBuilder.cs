using Sovereign.ClientCore.Rendering.Components;
using Sovereign.EngineCore.Entities;
using Sovereign.EngineCore.Systems.Block.Components;
using Sovereign.EngineCore.Systems.Movement.Components;

namespace Sovereign.ClientCore.Entities
{

    /// <summary>
    /// Entity builder for the client.
    /// </summary>
    public sealed class ClientEntityBuilder : AbstractEntityBuilder
    {
        private readonly DrawableComponentCollection drawables;
        private readonly AnimatedSpriteComponentCollection animatedSprites;

        public ClientEntityBuilder(ulong entityId,
            PositionComponentCollection positions,
            VelocityComponentCollection velocities,
            DrawableComponentCollection drawables,
            MaterialComponentCollection materials,
            MaterialModifierComponentCollection materialModifiers,
            AboveBlockComponentCollection aboveBlocks,
            AnimatedSpriteComponentCollection animatedSprites)
            : base(entityId, positions, velocities, materials, materialModifiers,
                  aboveBlocks)
        {
            this.drawables = drawables;
            this.animatedSprites = animatedSprites;
        }

        public override IEntityBuilder Drawable()
        {
            drawables.AddComponent(entityId, true);
            return this;
        }

        public override IEntityBuilder AnimatedSprite(int animatedSpriteId)
        {
            animatedSprites.AddComponent(entityId, animatedSpriteId);
            return this;
        }

    }

}
