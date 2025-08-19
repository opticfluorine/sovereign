// Sovereign Engine
// Copyright (c) 2025 opticfluorine
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

using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Sovereign.ServerCore.Systems.Scripting;

namespace Sovereign.ServerNetwork.Network.Rest.TemplateEntities;

/// <summary>
///     REST endpoint for providing ScriptInfo data.
/// </summary>
public sealed class ScriptInfoRestService(ScriptingServices scriptingServices)
{
    /// <summary>
    ///     GET endpoint for retrieving ScriptInfo data.
    /// </summary>
    /// <returns>Result.</returns>
    public Task<IResult> ScriptInfoGet()
    {
        return Task.FromResult(Results.Ok(scriptingServices.GetLoadedScriptInfo()));
    }
}