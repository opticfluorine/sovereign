﻿/*
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

namespace Sovereign.ClientCore.Rendering.Configuration;

/// <summary>
///     Exception thrown when there is an issue with a video adapter.
/// </summary>
[Serializable]
public class VideoAdapterException : ApplicationException
{
    public VideoAdapterException()
    {
    }

    public VideoAdapterException(string message) : base(message)
    {
    }

    public VideoAdapterException(string message, Exception inner) : base(message, inner)
    {
    }
}