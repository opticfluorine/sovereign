/*
 * Sovereign Engine
 * Copyright (c) 2020 opticfluorine
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Affero General Public License as
 * published by the Free Software Foundation, either version 3 of the
 * License, or (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Affero General Public License for more details.
 *
 * You should have received a copy of the GNU Affero General Public License
 * along with this program.  If not, see <https://www.gnu.org/licenses/>.
 */

using System;
using System.Runtime.Serialization;

namespace Sovereign.ClientCore.Rendering.Sprites
{

    /// <summary>
    /// Exception type thrown when an error occurs while loading a spritesheet definition.
    /// </summary>
    public class SpriteSheetDefinitionException : ApplicationException
    {
        public SpriteSheetDefinitionException()
        {
        }

        public SpriteSheetDefinitionException(string message) : base(message)
        {
        }

        public SpriteSheetDefinitionException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected SpriteSheetDefinitionException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }

}
