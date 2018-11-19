using Sovereign.ClientCore.Rendering.Components;
using Sovereign.EngineCore.Entities;
using Sovereign.EngineCore.Systems.Movement.Components;
using Sovereign.EngineCore.World.Materials.Components;

namespace Sovereign.ClientCore.Entities
{

    /// <summary>
    /// Entity builder for the client.
    /// </summary>
    public sealed class ClientEntityBuilder : AbstractEntityBuilder
    {
        private readonly DrawableComponentCollection drawables;

        public ClientEntityBuilder(ulong entityId,
            PositionComponentCollection positions,
            VelocityComponentCollection velocities,
            DrawableComponentCollection drawables,
            MaterialComponentCollection materials)
            : base(entityId, positions, velocities, materials)
        {
            this.drawables = drawables;
        }

        public override IEntityBuilder Drawable()
        {
            drawables.AddComponent(entityId, true);
            return this;
        }

    }

}
