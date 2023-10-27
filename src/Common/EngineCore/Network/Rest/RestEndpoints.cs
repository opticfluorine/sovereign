/*
 * Sovereign Engine
 * Copyright (c) 2023 opticfluorine
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

namespace Sovereign.EngineCore.Network.Rest;

/// <summary>
///     Contains constants that specify the relative URIs for REST endpoints.
/// </summary>
public sealed class RestEndpoints
{
    /// <summary>
    ///     Relative path to REST endpoint for the account registration service.
    /// </summary>
    public const string AccountRegistration = "/register";

    /// <summary>
    ///     Relative path to REST endpoint for the authentication service.
    /// </summary>
    public const string Authentication = "/login";

    /// <summary>
    ///     Relative path to REST endpoint for the world segment service.
    /// </summary>
    public const string WorldSegment = "/world";

    /// <summary>
    ///     Relative path to REST endpoint for the debug service.
    /// </summary>
    public const string Debug = "/debug";

    /// <summary>
    ///     Relative path to REST endpoint for player services.
    /// </summary>
    public const string Player = "/player";
}