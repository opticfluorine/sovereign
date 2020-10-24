/*
 * Sovereign Engine
 * Copyright (c) 2018 opticfluorine
 *
 * Permission is hereby granted, free of charge, to any person obtaining a 
 * copy of this software and associated documentation files (the "Software"), 
 * to deal in the Software without restriction, including without limitation 
 * the rights to use, copy, modify, merge, publish, distribute, sublicense, 
 * and/or sell copies of the Software, and to permit persons to whom the 
 * Software is furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER 
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING 
 * FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
 * DEALINGS IN THE SOFTWARE.
 */

using Castle.Core.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sovereign.ClientCore.Rendering.Configuration
{

    /// <summary>
    /// Responsible for selecting the video adapter to use.
    /// </summary>
    public class AdapterSelector
    {

        public ILogger Logger { private get; set; } = NullLogger.Instance;

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
                var selected = adapters.Where(adapter => adapter.OutputCount > 0)
                    .OrderByDescending(
                        adapter => adapter.DedicatedGraphicsMemory * DedicatedGpuFactor
                                + adapter.DedicatedSystemMemory * DedicatedCpuFactor
                                + adapter.SharedSystemMemory * SharedFactor
                    ).First();
                Logger.Info(() => CreateLogMessageForAdapter(selected));
                return selected;
            }
            catch (InvalidOperationException e)
            {
                /* No suitable adapters found. */
                throw new VideoAdapterException("No suitable video adapter found.", e);
            }
        }

        private string CreateLogMessageForAdapter(IVideoAdapter selectedAdapter)
        {
            var sb = new StringBuilder();
            const int conversionFactor = 1024 * 1024;
            sb.Append("Selected video adapter:\n")

                .Append("  Name                 = ")
                .Append(selectedAdapter.AdapterName)
                .Append("\n")

                .Append("  Dedicated GPU memory = ")
                .Append(selectedAdapter.DedicatedGraphicsMemory / conversionFactor)
                .Append(" MB\n")

                .Append("  Dedicated CPU memory = ")
                .Append(selectedAdapter.DedicatedSystemMemory / conversionFactor)
                .Append(" MB\n")

                .Append("  Shared CPU memory    = ")
                .Append(selectedAdapter.SharedSystemMemory / conversionFactor)
                .Append(" MB");

            return sb.ToString();
        }

    }

}
