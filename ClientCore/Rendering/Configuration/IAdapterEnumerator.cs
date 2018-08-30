using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sovereign.ClientCore.Rendering.Configuration
{

    /// <summary>
    /// Enumerates the available video adapters.
    /// </summary>
    public interface IAdapterEnumerator
    {

        /// <summary>
        /// Enumerates the available video adapters that are compatible
        /// with the renderer.
        /// </summary>
        /// <returns>Enumerated video adapters.</returns>
        IEnumerable<IVideoAdapter> EnumerateVideoAdapters();

    }

}
