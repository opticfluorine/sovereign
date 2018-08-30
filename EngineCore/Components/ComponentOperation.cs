using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sovereign.EngineCore.Components
{

    /// <summary>
    /// Describes the operation to be performed on the component.
    /// </summary>
    public enum ComponentOperation
    {

        /// <summary>
        /// Sets the value of the component to a new value.
        /// </summary>
        Set = 0,

        /// <summary>
        /// Adds a constant to the value of the component.
        /// </summary>
        Add = 1,

        /// <summary>
        /// Multiplies the value of the component by a constant.
        /// </summary>
        Multiply = 2,

        /// <summary>
        /// Divides the value of the component by a constant.
        /// </summary>
        Divide = 3,

    }

}
