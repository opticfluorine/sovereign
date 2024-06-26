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

namespace Sovereign.EngineCore.Main;

/// <summary>
///     Exception thrown when an error condition requires the engine to halt.
/// </summary>
/// If this exception escapes the IoC container, it will not be reported as
/// an unhandled exception.
public class FatalErrorException : ApplicationException
{
    public FatalErrorException()
    {
    }

    public FatalErrorException(string message) : base(message)
    {
    }

    public FatalErrorException(string message, Exception innerException) : base(message, innerException)
    {
    }
}