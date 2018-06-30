using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine8.ClientCore.Rendering.Configuration
{

    /// <summary>
    /// Responsible for selecting the video adapter to use.
    /// </summary>
    public class AdapterSelector
    {

        /// <summary>
        /// Scaling factor for dedicated GPU memory.
        /// </summary>
        private const long DedicatedGpuFactor = 1000;

        /// <summary>
        /// Scaling factor for dedicated system memory.
        /// </summary>
        private const long DedicatedCpuFactor = 10;

        /// <summary>
        /// Scaling factor for shared system memory.
        /// </summary>
        private const long SharedFactor = 1;

        /// <summary>
        /// Video adapter enumerator.
        /// </summary>
        private readonly IAdapterEnumerator adapterEnumerator;

        public AdapterSelector(IAdapterEnumerator adapterEnumerator)
        {
            this.adapterEnumerator = adapterEnumerator;
        }

        /// <summary>
        /// Selects the video adapter to be used.
        /// </summary>
        /// <returns>Selected video adapter.</returns>
        /// <exception cref="VideoAdapterException">
        /// Thrown if no suitable video adapter is found.
        /// </exception>
        public IVideoAdapter SelectAdapter()
        {
            /* 
             * Select the adapter with the best memory, preferring dedicated
             * GPU memory over other types of memory. In most cases, this should
             * yield the most powerful video adapter available.
             */
            var adapters = adapterEnumerator.EnumerateVideoAdapters();
            try
            {
                return adapters.OrderByDescending(
                        adapter => adapter.DedicatedGraphicsMemory * DedicatedGpuFactor
                                + adapter.DedicatedSystemMemory * DedicatedCpuFactor
                                + adapter.SharedSystemMemory * SharedFactor
                    ).First();
            }
            catch (InvalidOperationException e)
            {
                /* No suitable adapters found. */
                throw new VideoAdapterException("No suitable video adapter found.", e);
            }
        }

    }

}
