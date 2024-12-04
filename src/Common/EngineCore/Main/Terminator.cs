// Sovereign Engine
// Copyright (c) 2024 opticfluorine
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

using System.Threading;

namespace Sovereign.EngineCore.Main;

/// <summary>
///     Responsible for signaling graceful termination of the engine.
/// </summary>
public class Terminator
{
    private readonly CancellationTokenSource tokenSource = new();

    /// <summary>
    ///     CancellationToken used to signal engine termination.
    /// </summary>
    public CancellationToken TerminationToken => tokenSource.Token;

    /// <summary>
    ///     Terminates the engine gracefully.
    /// </summary>
    public void Terminate()
    {
        tokenSource.Cancel();
    }
}