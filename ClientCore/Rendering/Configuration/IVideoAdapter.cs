using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine8.ClientCore.Rendering.Configuration
{

    /// <summary>
    /// Video adapter description.
    /// </summary>
    public interface IVideoAdapter
    {

        /// <summary>
        /// The number of bytes of dedicated system memory available to the adapter.
        /// </summary>
        long DedicatedSystemMemory { get; }

        /// <summary>
        /// The number of bytes of dedicated GPU memory available to the adapter.
        /// </summary>
        long DedicatedGraphicsMemory { get; }

        /// <summary>
        /// The number of bytes of system memory shared between the adapter and the CPU.
        /// </summary>
        long SharedSystemMemory { get; }

    }

}
