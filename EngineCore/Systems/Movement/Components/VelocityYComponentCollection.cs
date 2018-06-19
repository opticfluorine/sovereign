using Engine8.EngineCore.Components;

namespace Engine8.EngineCore.Systems.Movement.Components
{

    /// <summary>
    /// Component collection for entity velocities along the y axis.
    /// </summary>
    public class VelocityYComponentCollection : BaseComponentCollection<float>
    {

        /// <summary>
        /// Initial number of components to allocate.
        /// </summary>
        private const int BASE_SIZE = 65536;

        public VelocityYComponentCollection() : base(BASE_SIZE, ComponentOperators.FloatOperators) { }

    }

}
