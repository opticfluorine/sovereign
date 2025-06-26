// Sovereign Engine
// Copyright (c) 2025 opticfluorine
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
using System.Text;
using Hexa.NET.ImGui;

namespace Sovereign.ClientCore.Rendering.Gui;

/// <summary>
///     Workarounds for known issues in ImGui bindings.
/// </summary>
public static class GuiWorkarounds
{
    /// <summary>
    ///     Largest buffer that will be handled via stackalloc.
    /// </summary>
    public const int StackallocMaxBufSize = 512;

    /// <summary>
    ///     Workaround for ImGui.InputText() with the EnterReturnsTrue flag and ref string buffers.
    /// </summary>
    /// <param name="label">Label.</param>
    /// <param name="buffer">Buffer.</param>
    /// <param name="bufferSize">Max buffer size.</param>
    /// <param name="otherFlags">Other flags (aside from EnterReturnsTrue) to use.</param>
    /// <returns>true if enter was pressed while the control was focused; false otherwise.</returns>
    public static bool InputTextEnterReturns(string label, ref string buffer, ulong bufferSize,
        ImGuiInputTextFlags otherFlags = ImGuiInputTextFlags.None)
    {
        return InputTextEnterReturns(Encoding.UTF8.GetBytes(label).AsSpan(), ref buffer, bufferSize, otherFlags);
    }

    /// <summary>
    ///     Workaround for ImGui.InputText() with the EnterReturnsTrue flag and ref string buffers.
    /// </summary>
    /// <param name="label">Label.</param>
    /// <param name="buffer">Buffer.</param>
    /// <param name="bufferSize">Max buffer size.</param>
    /// <param name="otherFlags">Other flags (aside from EnterReturnsTrue) to use.</param>
    /// <returns>true if enter was pressed while the control was focused; false otherwise.</returns>
    public static bool InputTextEnterReturns(ReadOnlySpan<byte> label, ref string buffer, ulong bufferSize,
        ImGuiInputTextFlags otherFlags = ImGuiInputTextFlags.None)
    {
        var flags = otherFlags | ImGuiInputTextFlags.EnterReturnsTrue;
        return bufferSize > StackallocMaxBufSize
            ? InputTextEnterReturnsHeap(label, ref buffer, bufferSize, flags)
            : InputTextEnterReturnsStack(label, ref buffer, bufferSize, flags);
    }

    /// <summary>
    ///     Stackalloc-based implementation of InputTextEnterReturns.
    /// </summary>
    /// <param name="label">Label.</param>
    /// <param name="buffer">Buffer.</param>
    /// <param name="bufferSize">Max buffer size.</param>
    /// <param name="flags">Flags.</param>
    /// <returns>true if enter was pressed while control focused; false otherwise.</returns>
    private static unsafe bool InputTextEnterReturnsStack(ReadOnlySpan<byte> label, ref string buffer, ulong bufferSize,
        ImGuiInputTextFlags flags)
    {
        var bufPtr = stackalloc byte[(int)bufferSize];
        var bytes = Encoding.UTF8.GetBytes(buffer);
        var copyLen = Math.Min((int)bufferSize - 1, bytes.Length);
        Marshal.Copy(bytes, 0, new IntPtr(bufPtr), copyLen);
        bufPtr[copyLen] = 0;

        var enterPressed = ImGui.InputText(label, bufPtr, bufferSize, flags);

        if (ImGui.IsItemDeactivatedAfterEdit()) buffer = Marshal.PtrToStringUTF8(new IntPtr(bufPtr)) ?? "";

        return enterPressed;
    }

    /// <summary>
    ///     Heap-based implementation of InputTextEnterReturns.
    /// </summary>
    /// <param name="label">Label.</param>
    /// <param name="buffer">Buffer.</param>
    /// <param name="bufferSize">Max buffer size.</param>
    /// <param name="flags">Flags.</param>
    /// <returns>true if enter was pressed while control focused; false otherwise.</returns>
    private static unsafe bool InputTextEnterReturnsHeap(ReadOnlySpan<byte> label, ref string buffer, ulong bufferSize,
        ImGuiInputTextFlags flags)
    {
        var bufPtr = (byte*)Marshal.AllocHGlobal((int)bufferSize);
        var bytes = Encoding.UTF8.GetBytes(buffer);
        var copyLen = Math.Min((int)bufferSize - 1, bytes.Length);
        Marshal.Copy(bytes, 0, new IntPtr(bufPtr), copyLen);
        bufPtr[copyLen] = 0;

        var enterPressed = ImGui.InputText(label, bufPtr, bufferSize, flags);

        if (ImGui.IsItemDeactivatedAfterEdit()) buffer = Marshal.PtrToStringUTF8(new IntPtr(bufPtr)) ?? "";

        Marshal.FreeHGlobal(new IntPtr(bufPtr));
        return enterPressed;
    }
}