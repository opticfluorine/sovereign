/*
 * Sovereign Engine
 * Copyright (c) 2021 opticfluorine
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

using Sovereign.ClientCore.Rendering.Configuration;

namespace Sovereign.VeldridRenderer.Rendering.Configuration;

/// <summary>
///     Dummy implementation of IVideoAdapter for the Veldrid renderer.
/// </summary>
/// Since the Veldrid library doesn't provide a direct method for querying
/// (or selecting) specific video adapters, only one dummy adapter will be
/// presented for selection.
public class VeldridVideoAdapter : IVideoAdapter
{
    public long DedicatedSystemMemory => 0;
    public long DedicatedGraphicsMemory => 0;
    public long SharedSystemMemory => 0;
    public int OutputCount => 1;
    public string AdapterName => "Veldrid Default";
}