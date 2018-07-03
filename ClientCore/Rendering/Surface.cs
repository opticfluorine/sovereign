using Engine8.ClientCore.Rendering.Configuration;
using SDL2;
using System;
using System.Collections.Generic;

namespace Engine8.ClientCore.Rendering
{

    /// <summary>
    /// Simple wrapper for an SDL_Surface.
    /// </summary>
    /// 
    /// Surfaces should only be handled from the main thread.
    public class Surface : IDisposable
    {

        /// <summary>
        /// Pointer to the underlying SDL_Surface.
        /// </summary>
        public IntPtr SurfacePointer { get; private set; }

        /// <summary>
        /// Whether the surface has been disposed.
        /// </summary>
        public bool IsDisposed { get; private set; } = false;

        /// <summary>
        /// Whether the surface is backed by a valid pointer.
        /// </summary>
        public bool IsValid { get => SurfacePointer != IntPtr.Zero; }

        /// <summary>
        /// Map from DisplayFormat to SDL pixel formats.
        /// </summary>
        private static readonly IDictionary<DisplayFormat, uint> formatMap
            = new Dictionary<DisplayFormat, uint>()
            {
                {DisplayFormat.R8G8B8A8_UNorm, SDL.SDL_PIXELFORMAT_RGBA8888},
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
            int bpp;
            uint amask, rmask, gmask, bmask;
            if (SDL.SDL_PixelFormatEnumToMasks(formatMap[format], out bpp, out rmask, out gmask,
                out bmask, out amask) == SDL.SDL_bool.SDL_FALSE)
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

    }

}
