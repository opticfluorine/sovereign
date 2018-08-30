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
    /// Manages the velocities of all mobile entities.
    /// </summary>
    public sealed class VelocityComponentCollection : BaseComponentCollection<Vector3>
    {

        /// <summary>
        /// Default size of component buffer.
        /// </summary>
        private const int BaseSize = 65536;

        public VelocityComponentCollection() : base(BaseSize, ComponentOperators.VectorOperators) { }

    }

}
