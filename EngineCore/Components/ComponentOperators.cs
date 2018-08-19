using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Engine8.EngineCore.Components
{

    /// <summary>
    /// Standard operators for common component types.
    /// </summary>
    public static class ComponentOperators
    {

        /// <summary>
        /// Standard operators for float-valued components.
        /// </summary>
        public static readonly IDictionary<ComponentOperation, Func<float, float, float>>
            FloatOperators = new Dictionary<ComponentOperation, Func<float, float, float>>()
            {
                {ComponentOperation.Set, (a, b) => b},
                {ComponentOperation.Add, (a, b) => a + b},
                {ComponentOperation.Multiply, (a, b) => a * b},
                {ComponentOperation.Divide, (a, b) => a / b},
            };

        /// <summary>
        /// Standard operators for vector-valued components.
        /// </summary>
        public static readonly IDictionary<ComponentOperation, Func<Vector3, Vector3, Vector3>>
            VectorOperators = new Dictionary<ComponentOperation, Func<Vector3, Vector3, Vector3>>()
            {
                {ComponentOperation.Set, (a, b) => b},
                {ComponentOperation.Add, Vector3.Add},
            };

    }

}
