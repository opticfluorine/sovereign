using SDL2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine8.ClientCore.Rendering.Sprites
{

    /// <summary>
    /// Responsible for collecting spritesheets into a single texture atlas.
    /// </summary>
    public class TextureAtlas
    {

        public SDL.SDL_Surface AtlasSurface { get; }

        public TextureAtlas()
        {

        }

    }

}
