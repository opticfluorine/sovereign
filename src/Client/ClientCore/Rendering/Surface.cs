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
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using SDL3;
using Sovereign.ClientCore.Rendering.Configuration;

namespace Sovereign.ClientCore.Rendering;

/// <summary>
///     Simple wrapper for an SDL_Surface.
/// </summary>
/// Surfaces should only be handled from the main thread.
public class Surface : IDisposable
{
    /// <summary>
    ///     Map from DisplayFormat to SDL pixel formats.
    /// </summary>
    private static readonly Dictionary<DisplayFormat, SDL.PixelFormat> FormatMap = new()
    {
        { DisplayFormat.R8G8B8A8_UNorm, SDL.PixelFormat.ABGR8888 },
        { DisplayFormat.B8G8R8A8_UNorm, SDL.PixelFormat.ARGB8888 }
    };

    /// <summary>
    ///     Inverse map of formatMap.
    /// </summary>
    private static readonly Dictionary<SDL.PixelFormat, DisplayFormat> InvFormatMap
        = new()
        {
            { SDL.PixelFormat.ABGR8888, DisplayFormat.R8G8B8A8_UNorm },
            { SDL.PixelFormat.ARGB8888, DisplayFormat.B8G8R8A8_UNorm }
        };

    /// <summary>
    ///     Underlying surface properties.
    /// </summary>
    private SurfaceProperties? properties;

    /// <summary>
    /// </summary>
    private IntPtr surfacePointer = IntPtr.Zero;

    /// <summary>
    ///     Wraps an existing SDL_Surface pointer.
    /// </summary>
    /// <param name="surfacePointer">Existing SDL_Surface pointer.</param>
    public Surface(IntPtr surfacePointer)
    {
        SurfacePointer = surfacePointer;
    }

    /// <summary>
    ///     Copies an existing surface into a new pixel format.
    /// </summary>
    /// <param name="original">Original surface.</param>
    /// <param name="newFormat">New pixel format.</param>
    public Surface(Surface original, DisplayFormat newFormat)
        : this(SDL.ConvertSurface(original.SurfacePointer, FormatMap[newFormat]))
    {
        /* Check that surface creation was successful. */
        if (!IsValid) throw new SurfaceException(SDL.GetError());
    }

    /// <summary>
    ///     Provides access to the pointer to the underlying SDL_Surface.
    /// </summary>
    public IntPtr SurfacePointer
    {
        get => surfacePointer;

        private set
        {
            surfacePointer = value;
            if (IsValid) properties = new SurfaceProperties(value);
        }
    }

    /// <summary>
    ///     Whether the surface has been disposed.
    /// </summary>
    public bool IsDisposed { get; private set; }

    /// <summary>
    ///     Whether the surface is backed by a valid pointer.
    /// </summary>
    [MemberNotNullWhen(true, "properties")]
    public bool IsValid => SurfacePointer != IntPtr.Zero;

    /// <summary>
    ///     Read-only property that provides access to underlying surface properties.
    /// </summary>
    public SurfaceProperties Properties
    {
        get
        {
            if (!IsValid) throw new InvalidOperationException("Surface is not valid.");
            return properties;
        }
    }

    /// <summary>
    ///     Disposes of the surface by freeing the underlying memory.
    /// </summary>
    public void Dispose()
    {
        if (IsValid)
        {
            SDL.Free(SurfacePointer);
            SurfacePointer = IntPtr.Zero;
        }

        IsDisposed = true;
    }

    /// <summary>
    ///     Creates a new surface with the given dimensions and format.
    /// </summary>
    /// <param name="width">Surface width.</param>
    /// <param name="height">Surface height.</param>
    /// <param name="format">Surface format.</param>
    /// <returns>New surface.</returns>
    /// <exception cref="SurfaceException">
    ///     Thrown if an error occurs while creating the surface.
    /// </exception>
    public static Surface CreateSurface(int width, int height, DisplayFormat format)
    {
        var surface = new Surface(SDL.CreateSurface(width, height, FormatMap[format]));
        if (!surface.IsValid) throw new SurfaceException(SDL.GetError());
        return surface;
    }

    /// <summary>
    ///     Creates a new surface from a copy of the given raw pixel data.
    /// </summary>
    /// <param name="pixelData">Pointer to pixel data.</param>
    /// <param name="width">Width of the surface in pixels.</param>
    /// <param name="height">Height of the surface in pixels.</param>
    /// <param name="format">Format of the pixel data and of the surface.</param>
    /// <returns>New surface containing a copy of the pixel data in the source format.</returns>
    public static Surface CreateSurfaceFrom(IntPtr pixelData, int width, int height, DisplayFormat format)
    {
        var bpp = 0;
        uint rmask = 0, gmask = 0, bmask = 0, amask = 0;
        if (!SDL.GetMasksForPixelFormat(FormatMap[format], ref bpp, ref rmask, ref gmask, ref bmask, ref amask))
            throw new SurfaceException(SDL.GetError());

        var pitch = bpp / 8 * width;
        var sdlSurface = SDL.CreateSurfaceFrom(width, height, FormatMap[format], pixelData, pitch);
        var surface = new Surface(sdlSurface);
        if (!surface.IsValid) throw new SurfaceException(SDL.GetError());

        return surface;
    }

    /// <summary>
    ///     Blits the entire surface onto a destination surface.
    /// </summary>
    /// <param name="dest">Destination surface.</param>
    /// <param name="destX">Top left x coordinate of destination.</param>
    /// <param name="destY">Top left y coordinate of destination.</param>
    public void Blit(Surface dest, int destX, int destY)
    {
        /* Ensure the source is valid. */
        if (!IsValid)
            throw new InvalidOperationException("Source surface is not valid.");

        /* Blit the entire surface. */
        Blit(dest, destX, destY, 0, 0, Properties.Width, Properties.Height);
    }

    /// <summary>
    ///     Blits a portion of this surface onto a destination surface.s
    /// </summary>
    /// <param name="dest">Destination surface.</param>
    /// <param name="destX">Top left x coordinate of destination.</param>
    /// <param name="destY">Top left y coordinate of destination.</param>
    /// <param name="srcX">Top left x coordinate of source.</param>
    /// <param name="srcY">Top left y coordinate of source.</param>
    /// <param name="width">Width of source region.</param>
    /// <param name="height">Height of source region.</param>
    public void Blit(Surface dest, int destX, int destY, int srcX,
        int srcY, int width, int height)
    {
        /* Ensure the source and destination are valid. */
        if (!IsValid)
            throw new InvalidOperationException("Source surface is not valid.");
        if (!dest.IsValid)
            throw new InvalidOperationException("Destination surface is not valid.");

        /* Prepare blit structures. */
        var srcRect = new SDL.Rect
        {
            X = srcX,
            Y = srcY,
            W = width,
            H = height
        };
        var dstRect = new SDL.Rect
        {
            X = destX,
            Y = destY,
            W = width,
            H = height
        };

        /* Blit. */
        if (!SDL.BlitSurface(SurfacePointer, srcRect, dest.SurfacePointer, dstRect))
            throw new SurfaceException(SDL.GetError());
    }

    /// <summary>
    ///     Saves the surface to a PNG file.
    /// </summary>
    /// <param name="filename">Filename to save to.</param>
    /// <exception cref="InvalidOperationException">
    ///     Thrown if the surface is not valid.
    /// </exception>
    /// <exception cref="SurfaceException">
    ///     Thrown if the image cannot be saved.
    /// </exception>
    public void SavePng(string filename)
    {
        /* Ensure the surface is valid. */
        if (!IsValid)
            throw new InvalidOperationException("Surface is not valid.");

        /* Save the surface to a file. */
        if (!Image.SavePNG(surfacePointer, filename))
            throw new SurfaceException(SDL.GetError());
    }

    /// <summary>
    ///     Proxy class that provides access to underlying surface data.
    /// </summary>
    public class SurfaceProperties
    {
        /// <summary>
        ///     Underlying surface.
        /// </summary>
        private readonly SDL.Surface surface;

        internal SurfaceProperties(IntPtr surfacePtr)
        {
            // Grab the surface.
            surface = Marshal.PtrToStructure<SDL.Surface>(surfacePtr);

            // Determine surface format.
            if (InvFormatMap.ContainsKey(surface.Format))
                Format = InvFormatMap[surface.Format];
            else
                // Intermediate format - mark as not supported for GPU use.
                Format = DisplayFormat.CpuUseOnly;

            var bpp = 0;
            uint rmask = 0, gmask = 0, bmask = 0, amask = 0;
            if (!SDL.GetMasksForPixelFormat(surface.Format, ref bpp, ref rmask, ref gmask, ref bmask, ref amask))
                throw new SurfaceException(SDL.GetError());

            BytesPerPixel = (uint)bpp;
        }

        /// <summary>
        ///     Surface width.
        /// </summary>
        public int Width => surface.Width;

        /// <summary>
        ///     Surface height.
        /// </summary>
        public int Height => surface.Height;

        /// <summary>
        ///     Pitch of the surface.
        /// </summary>
        public int Pitch => surface.Pitch;

        /// <summary>
        ///     Pointer to pixel data.
        /// </summary>
        public IntPtr Data => surface.Pixels;

        /// <summary>
        ///     Pixel format.
        /// </summary>
        public DisplayFormat Format { get; private set; }

        /// <summary>
        ///     Bytes per pixel.
        /// </summary>
        public uint BytesPerPixel { get; private set; }
    }
}