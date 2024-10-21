// Sovereign Engine
// Copyright (c) 2024 opticfluorine
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Runtime.InteropServices;
using Veldrid;

namespace Sovereign.VeldridRenderer.Rendering.Resources;

/// <summary>
///     Uniform buffer that supports dynamic binding.
/// </summary>
public class DynamicUniformBuffer<T> : IDisposable where T : struct
{
    private readonly uint alignment;
    private readonly byte[] buffer;
    private readonly IntPtr bufferBase;
    private GCHandle gcHandle;

    public DynamicUniformBuffer(VeldridDevice device, int sizeInT)
    {
        // Compute alignments and allocate memory.
        Length = sizeInT;
        var tSize = (uint)Marshal.SizeOf<T>();
        var minAlignment = device.Device!.UniformBufferMinOffsetAlignment;
        alignment = (tSize / minAlignment + 1) * minAlignment;

        buffer = new byte[sizeInT * alignment];
        gcHandle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
        bufferBase = gcHandle.AddrOfPinnedObject();

        // Allocate device buffer.
        var desc = new BufferDescription((uint)buffer.Length,
            BufferUsage.Dynamic | BufferUsage.StructuredBufferReadOnly);
        DeviceBuffer = device.Device!.ResourceFactory.CreateBuffer(desc);
    }

    /// <summary>
    ///     Device buffer.
    /// </summary>
    public DeviceBuffer DeviceBuffer { get; }

    /// <summary>
    ///     Length of the buffer.
    /// </summary>
    public int Length { get; }

    public T this[int i]
    {
        get
        {
            if (i < 0 || i >= Length) throw new IndexOutOfRangeException();
            return Marshal.PtrToStructure<T>(bufferBase + (IntPtr)(i * alignment));
        }
        set
        {
            if (i < 0 || i >= Length) throw new IndexOutOfRangeException();
            Marshal.StructureToPtr(value, bufferBase + (IntPtr)(i * alignment), false);
        }
    }

    public void Dispose()
    {
        DeviceBuffer.Dispose();
        gcHandle.Free();
    }

    /// <summary>
    ///     Gets the byte offset for the given position.
    /// </summary>
    /// <param name="i">Position.</param>
    /// <returns>Byte offset.</returns>
    /// <exception cref="IndexOutOfRangeException">Thrown if position is out of bounds.</exception>
    public uint GetOffset(int i)
    {
        if (i < 0 || i > Length) throw new IndexOutOfRangeException();
        return (uint)i * alignment;
    }

    /// <summary>
    ///     Updates the buffer on the GPU.
    /// </summary>
    /// <param name="commandList">Command list.</param>
    public void Update(CommandList commandList)
    {
        commandList.UpdateBuffer(DeviceBuffer, 0, buffer);
    }
}