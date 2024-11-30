/*
 * Sovereign Engine
 * Copyright (c) 2018 opticfluorine
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <https://www.gnu.org/licenses/>.
 */

using System;
using System.Linq;
using System.Text;
using Microsoft.Extensions.Logging;

namespace Sovereign.ClientCore.Rendering.Configuration;

/// <summary>
///     Responsible for selecting the video adapter to use.
/// </summary>
public class AdapterSelector
{
    /// <summary>
    ///     Scaling factor for dedicated GPU memory.
    /// </summary>
    private const long DedicatedGpuFactor = 1000;

    /// <summary>
    ///     Scaling factor for dedicated system memory.
    /// </summary>
    private const long DedicatedCpuFactor = 10;

    /// <summary>
    ///     Scaling factor for shared system memory.
    /// </summary>
    private const long SharedFactor = 1;

    /// <summary>
    ///     Video adapter enumerator.
    /// </summary>
    private readonly IAdapterEnumerator adapterEnumerator;

    private readonly ILogger<AdapterSelector> logger;

    public AdapterSelector(IAdapterEnumerator adapterEnumerator, ILogger<AdapterSelector> logger)
    {
        this.adapterEnumerator = adapterEnumerator;
        this.logger = logger;
    }

    /// <summary>
    ///     Selects the video adapter to be used.
    /// </summary>
    /// <returns>Selected video adapter.</returns>
    /// <exception cref="VideoAdapterException">
    ///     Thrown if no suitable video adapter is found.
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
            logger.LogInformation(CreateLogMessageForAdapter(selected));
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