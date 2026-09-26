// Sovereign Engine
// Copyright (c) 2026 opticfluorine
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

using System.Runtime.InteropServices;
using SDL2;
using Sovereign.ClientCore.Rendering;
using Sovereign.ClientCore.Systems.DebugInterface.Protocol;

namespace SovereignClientMcp;

/// <summary>
///     Writes captured screenshot pixel data to PNG files via SDL_image.
/// </summary>
public static class ScreenshotWriter
{
    private static readonly object initLock = new();
    private static bool pngSupportChecked;
    private static bool pngSupported;

    /// <summary>
    ///     Saves a captured screenshot as a PNG file.
    /// </summary>
    /// <param name="details">Screenshot response details.</param>
    /// <param name="requestId">Request ID used to name the file.</param>
    /// <param name="directory">Directory in which to save the file.</param>
    /// <returns>Absolute path of the saved PNG file.</returns>
    /// <exception cref="DebugClientException">If the screenshot could not be saved.</exception>
    public static string SavePng(ScreenshotResponseDetails details, uint requestId, string directory)
    {
        EnsurePngSupport();
        Directory.CreateDirectory(directory);

        var fileName = $"screenshot-{requestId}-{DateTime.Now:HHmmss}.png";
        var path = Path.GetFullPath(Path.Combine(directory, fileName));

        var handle = GCHandle.Alloc(details.Pixels, GCHandleType.Pinned);
        try
        {
            var surface = SDL.SDL_CreateRGBSurfaceWithFormatFrom(
                handle.AddrOfPinnedObject(), details.Width, details.Height, 32,
                details.Width * 4, GetSdlPixelFormat(details.Format));
            if (surface == IntPtr.Zero)
                throw new DebugClientException(
                    $"Failed to create a surface for the screenshot: {SDL.SDL_GetError()}");

            try
            {
                if (SDL_image.IMG_SavePNG(surface, path) != 0)
                    throw new DebugClientException(
                        $"Failed to save the screenshot: {SDL.SDL_GetError()}");
            }
            finally
            {
                SDL.SDL_FreeSurface(surface);
            }
        }
        finally
        {
            handle.Free();
        }

        return path;
    }

    /// <summary>
    ///     Releases SDL_image resources if PNG support was initialized. Called at shutdown.
    /// </summary>
    public static void Shutdown()
    {
        lock (initLock)
        {
            if (!pngSupported) return;
            pngSupported = false;
            SDL_image.IMG_Quit();
        }
    }

    /// <summary>
    ///     Maps a captured pixel format to the SDL pixel format with the same byte layout.
    /// </summary>
    /// <param name="format">Captured pixel format.</param>
    /// <returns>SDL pixel format constant.</returns>
    /// <exception cref="DebugClientException">If the pixel format is not supported.</exception>
    private static uint GetSdlPixelFormat(CapturedPixelFormat format)
    {
        return format switch
        {
            CapturedPixelFormat.Bgra8 => SDL.SDL_PIXELFORMAT_ARGB8888,
            CapturedPixelFormat.Bgra8Srgb => SDL.SDL_PIXELFORMAT_ARGB8888,
            CapturedPixelFormat.Rgba8 => SDL.SDL_PIXELFORMAT_ABGR8888,
            CapturedPixelFormat.Rgba8Srgb => SDL.SDL_PIXELFORMAT_ABGR8888,
            _ => throw new DebugClientException(
                $"Unsupported captured pixel format: {format}.")
        };
    }

    /// <summary>
    ///     Lazily initializes SDL_image PNG support, tolerating missing native libraries
    ///     with a descriptive error.
    /// </summary>
    /// <exception cref="DebugClientException">If PNG support is not available.</exception>
    private static void EnsurePngSupport()
    {
        lock (initLock)
        {
            if (pngSupportChecked)
            {
                if (!pngSupported) throw CreatePngSupportException();
                return;
            }

            try
            {
                var initFlags = SDL_image.IMG_Init(SDL_image.IMG_InitFlags.IMG_INIT_PNG);
                if ((initFlags & (int)SDL_image.IMG_InitFlags.IMG_INIT_PNG) == 0)
                    throw new DebugClientException(
                        $"Failed to initialize SDL_image PNG support: {SDL.SDL_GetError()}");
                pngSupported = true;
            }
            catch (Exception e) when (e is DllNotFoundException or EntryPointNotFoundException)
            {
                throw CreatePngSupportException(e);
            }
            finally
            {
                pngSupportChecked = true;
            }
        }
    }

    /// <summary>
    ///     Creates an exception describing the steps needed to make PNG support available.
    /// </summary>
    /// <param name="innerException">Optional inner exception.</param>
    /// <returns>Descriptive exception.</returns>
    private static DebugClientException CreatePngSupportException(Exception? innerException = null)
    {
        return new DebugClientException(
            "SDL_image PNG support is not available; screenshots cannot be saved. " +
            "Ensure that the SDL2 and SDL2_image native libraries are installed.",
            innerException);
    }
}
