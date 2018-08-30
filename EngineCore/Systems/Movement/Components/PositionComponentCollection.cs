using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Sovereign.EngineCore.Components;

namespace Sovereign.EngineCore.Systems.Movement.Components
{

    /// <summary>
    /// Manages the positions of all positionable entities.
    /// </summary>
    public sealed class PositionComponentCollection : BaseComponentCollection<Vector3>
    {

        /// <summary>
        /// Initial number of allocated components.
        /// </summary>
        private const int BaseSize = 65536;

        public PositionComponentCollection() : base(BaseSize, ComponentOperators.VectorOperators)
        {
        }

    }

}
