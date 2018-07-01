using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine8.ClientCore.Rendering.Configuration
{

    /// <summary>
    /// Enumerates the available display modes for an adapter.
    /// </summary>
    public interface IDisplayModeEnumerator
    {

        /// <summary>
        /// Enumerates the available display modes for the given adapter.
        /// </summary>
        /// <returns>Available display modes.</returns>
        /// <exception cref="RendererInitializationException">
        /// Thrown if an error occurs while enumerating the display modes.
        /// </exception>
        IEnumerable<IDisplayMode> EnumerateDisplayModes(IVideoAdapter adapter);

    }

}
