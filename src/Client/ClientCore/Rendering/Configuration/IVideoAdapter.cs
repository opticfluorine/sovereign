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

namespace Sovereign.ClientCore.Rendering.Configuration;

/// <summary>
///     Video adapter description.
/// </summary>
public interface IVideoAdapter
{
    /// <summary>
    ///     The number of bytes of dedicated system memory available to the adapter.
    /// </summary>
    long DedicatedSystemMemory { get; }

    /// <summary>
    ///     The number of bytes of dedicated GPU memory available to the adapter.
    /// </summary>
    long DedicatedGraphicsMemory { get; }

    /// <summary>
    ///     The number of bytes of system memory shared between the adapter and the CPU.
    /// </summary>
    long SharedSystemMemory { get; }

    /// <summary>
    ///     The number of outputs supported by this device.
    ///     Used to filter devices that cannot be used directly.
    /// </summary>
    int OutputCount { get; }

    /// <summary>
    ///     Short name of the adapter.
    /// </summary>
    string AdapterName { get; }
}