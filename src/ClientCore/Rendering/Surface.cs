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

using Sovereign.ClientCore.Rendering.Configuration;
using SDL2;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Sovereign.ClientCore.Rendering
{

    /// <summary>
    /// Simple wrapper for an SDL_Surface.
    /// </summary>
    /// 
    /// Surfaces should only be handled from the main thread.
    public class Surface : IDisposable
    {

        /// <summary>
        /// Provides access to the pointer to the underlying SDL_Surface.
        /// </summary>
        public IntPtr SurfacePointer
        {
            get => surfacePointer;

            private set
            {
                surfacePointer = value;
                if (IsValid)
                {
                    properties = new SurfaceProperties(value);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private IntPtr surfacePointer = IntPtr.Zero;

        /// <summary>
        /// Whether the surface has been disposed.
        /// </summary>
        public bool IsDisposed { get; private set; } = false;

        /// <summary>
        /// Whether the surface is backed by a valid pointer.
        /// </summary>
        public bool IsValid { get => SurfacePointer != IntPtr.Zero; }

        /// <summary>
        /// Read-only property that provides access to underlying surface properties.
        /// </summary>
        public SurfaceProperties Properties
        {
            get
            {
                if (!IsValid)
                {
                    throw new InvalidOperationException("Surface is not valid.");
                }
                return properties;
            }
        }

        /// <summary>
        /// Underlying surface properties.
        /// </summary>
        private SurfaceProperties properties = null;

        /// <summary>
        /// Map from DisplayFormat to SDL pixel formats.
        /// </summary>
        private static readonly IDictionary<DisplayFormat, uint> formatMap
            = new Dictionary<DisplayFormat, uint>()
            {
                {DisplayFormat.R8G8B8A8_UNorm, SDL.SDL_PIXELFORMAT_ABGR8888},
            };

        /// <summary>
        /// Wraps an existing SDL_Surface pointer.
        /// </summary>
        /// <param name="surfacePointer">Existing SDL_Surface pointer.</param>
        public Surface(IntPtr surfacePointer)
        {
            SurfacePointer = surfacePointer;
        }

        /// <summary>
        /// Copies an existing surface into a new pixel format.
        /// </summary>
        /// <param name="original">Original surface.</param>
        /// <param name="newFormat">New pixel format.</param>
        public Surface(Surface original, DisplayFormat newFormat)
            : this(SDL.SDL_ConvertSurfaceFormat(original.SurfacePointer, formatMap[newFormat], 0))
        {
            /* Check that surface creation was successful. */
            if (!IsValid)
            {
                throw new SurfaceException(SDL.SDL_GetError());
            }
        }

        /// <summary>
        /// Creates a new surface with the given dimensions and format.
        /// </summary>
        /// <param name="width">Surface width.</param>
        /// <param name="height">Surface height.</param>
        /// <param name="format">Surface format.</param>
        /// <returns>New surface.</returns>
        /// <exception cref="SurfaceException">
        /// Thrown if an error occurs while creating the surface.
        /// </exception>
        public static Surface CreateSurface(int width, int height, DisplayFormat format)
        {
            /* Look up the format. */
            if (SDL.SDL_PixelFormatEnumToMasks(formatMap[format], out int bpp, out uint rmask, out uint gmask,
                out uint bmask, out uint amask) == SDL.SDL_bool.SDL_FALSE)
            {
                throw new SurfaceException(SDL.SDL_GetError());
            }

            /* Create the surface. */
            var surface = new Surface(SDL.SDL_CreateRGBSurface(0, width, height, bpp, 
                rmask, gmask, bmask, amask));
            if (!surface.IsValid)
            {
                throw new SurfaceException(SDL.SDL_GetError());
            }
            return surface;
        }

        /// <summary>
        /// Creates a new surface from a copy of the given raw pixel data.
        /// </summary>
        /// <param name="pixelData">Pointer to pixel data.</param>
        /// <param name="width">Width of the surface in pixels.</param>
        /// <param name="height">Height of the surface in pixels.</param>
        /// <param name="format">Format of the pixel data and of the surface.</param>
        /// <returns>New surface containing a copy of the pixel data in the source format.</returns>
        public static Surface CreateSurfaceFrom(IntPtr pixelData, int width, int height, DisplayFormat format)
        {
            /* Look up the format. */
            if (SDL.SDL_PixelFormatEnumToMasks(formatMap[format], out int bpp, out uint rmask, out uint gmask,
                    out uint bmask, out uint amask) == SDL.SDL_bool.SDL_FALSE)
            {
                throw new SurfaceException(SDL.SDL_GetError());
            }

            /* Create the surface. */
            var pitch = (bpp / 8) * width;
            var sdlSurface = SDL.SDL_CreateRGBSurfaceFrom(pixelData, width, height, bpp,
                pitch, rmask, gmask, bmask, amask);
            var surface = new Surface(sdlSurface);
            if (!surface.IsValid)
            {
                throw new SurfaceException(SDL.SDL_GetError());
            }

            return surface;
        }

        /// <summary>
        /// Blits the entire surface onto a destination surface.
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
        /// Blits a portion of this surface onto a destination surface.s
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
            var srcRect = new SDL.SDL_Rect()
            {
                x = srcX,
                y = srcY,
                w = width,
                h = height,
            };
            var dstRect = new SDL.SDL_Rect()
            {
                x = destX,
                y = destY,
                w = width,
                h = height,
            };

            /* Blit. */
            int res = SDL.SDL_BlitSurface(SurfacePointer, ref srcRect,
                dest.SurfacePointer, ref dstRect);
            if (res < 0)
            {
                throw new SurfaceException(SDL.SDL_GetError());
            }
        }

        /// <summary>
        /// Saves the surface to a PNG file.
        /// </summary>
        /// <param name="filename">Filename to save to.</param>
        /// <exception cref="InvalidOperationException">
        /// Thrown if the surface is not valid.
        /// </exception>
        /// <exception cref="SurfaceException">
        /// Thrown if the image cannot be saved.
        /// </exception>
        public void SavePng(string filename)
        {
            /* Ensure the surface is valid. */
            if (!IsValid)
                throw new InvalidOperationException("Surface is not valid.");

            /* Save the surface to a file. */
            int status = SDL_image.IMG_SavePNG(surfacePointer, filename);
            if (status < 0)
                throw new SurfaceException(SDL.SDL_GetError());
        }

        /// <summary>
        /// Disposes of the surface by freeing the underlying memory.
        /// </summary>
        public void Dispose()
        {
            if (IsValid)
            {
                SDL.SDL_FreeSurface(SurfacePointer);
                SurfacePointer = IntPtr.Zero;
            }
            IsDisposed = true;
        }

        /// <summary>
        /// Proxy class that provides access to underlying surface data.
        /// </summary>
        public class SurfaceProperties
        {

            /// <summary>
            /// Underlying surface.
            /// </summary>
            private readonly SDL.SDL_Surface surface;

            /// <summary>
            /// Surface width.
            /// </summary>
            public int Width => surface.w;

            /// <summary>
            /// Surface height.
            /// </summary>
            public int Height => surface.h;

            /// <summary>
            /// Pitch of the surface.
            /// </summary>
            public int Pitch => surface.pitch;

            /// <summary>
            /// Pointer to pixel data.
            /// </summary>
            public IntPtr Data => surface.pixels;

            internal SurfaceProperties(IntPtr surfacePtr)
            {
                surface = Marshal.PtrToStructure<SDL.SDL_Surface>(surfacePtr);
            }

        }

    }

}
