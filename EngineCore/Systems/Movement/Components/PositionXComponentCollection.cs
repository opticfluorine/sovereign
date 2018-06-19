using Engine8.EngineCore.Components;

namespace Engine8.EngineCore.Systems.Movement.Components
{

    /// <summary>
    /// Component collection for entity positions along the x axis.
    /// </summary>
    public class PositionXComponentCollection : BaseComponentCollection<float>
    {

        /// <summary>
        /// Initial number of allocated components.
        /// </summary>
        private const int BASE_SIZE = 65536;

        public PositionXComponentCollection() : base(BASE_SIZE, ComponentOperators.FloatOperators) { }

    }

}
