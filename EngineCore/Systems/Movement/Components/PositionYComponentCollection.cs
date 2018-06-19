using Engine8.EngineCore.Components;

namespace Engine8.EngineCore.Systems.Movement.Components
{

    /// <summary>
    /// Component collection for entity positions along the y axis.
    /// </summary>
    public class PositionYComponentCollection : BaseComponentCollection<float>
    {

        /// <summary>
        /// Initial number of components to allocate.
        /// </summary>
        private const int BASE_SIZE = 65536;

        public PositionYComponentCollection() : base(BASE_SIZE, ComponentOperators.FloatOperators) { }

    }

}
