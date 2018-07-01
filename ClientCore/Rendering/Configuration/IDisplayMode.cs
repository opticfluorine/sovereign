using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine8.ClientCore.Rendering.Configuration
{

    /// <summary>
    /// Display mode.
    /// </summary>
    public interface IDisplayMode
    {

        /// <summary>
        /// Display width.
        /// </summary>
        int Width { get; }

        /// <summary>
        /// Display height.
        /// </summary>
        int Height { get; }

        /// <summary>
        /// Display format.
        /// </summary>
        DisplayFormat DisplayFormat { get; }

    }

}
