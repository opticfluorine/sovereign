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

using Sovereign.EngineCore.Components;

namespace Sovereign.ClientCore.Rendering.Components
{

    /// <summary>
    /// Describes a component that indicates whether an entity can be drawn.
    /// </summary>
    /// 
    /// This component is of type bool, but the value has no meaning; the existence of
    /// any Drawable component for an entity indicates that the entity is drawable.
    public sealed class DrawableComponentCollection : BaseComponentCollection<bool>
    {

        private const int BaseSize = 65536;

        public DrawableComponentCollection(ComponentManager componentManager)
            : base(componentManager, BaseSize, ComponentOperators.BoolOperators,
                  ComponentType.Drawable)
        {
        }

    }
}
