using Engine8.EngineCore.Components;

namespace Engine8.EngineCore.Systems.Movement.Components
{

    /// <summary>
    /// Component collection for entity velocities along the x axis.
    /// </summary>
    public class VelocityXComponentCollection : BaseComponentCollection<float>
    {

        /// <summary>
        /// Initial number of components to allocate.
        /// </summary>
        private const int BASE_SIZE = 65536;

        public VelocityXComponentCollection() : base(BASE_SIZE, ComponentOperators.FloatOperators) { }

    }

}
