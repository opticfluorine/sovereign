// Sovereign Engine
// Copyright (c) 2023 opticfluorine
// 
// Permission is hereby granted, free of charge, to any person obtaining a
// copy of this software and associated documentation files (the "Software"),
// to deal in the Software without restriction, including without limitation
// the rights to use, copy, modify, merge, publish, distribute, sublicense,
// and/or sell copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.

namespace Sovereign.Persistence.Players;

/// <summary>
///     Public API exported by Persistence to provide player-related database services.
/// </summary>
public class PersistencePlayerServices
{
    /// <summary>
    ///     Determines whether the given player name is taken.
    /// </summary>
    /// <param name="name">Player name.</param>
    /// <returns>true if taken, false otherwise.</returns>
    /// <remarks>
    ///     Note that this only checks whether the name exists in the database for a player character.
    ///     It does not check whether a player character with the same name exists in memory and has not
    ///     yet been synchronized to the database.
    /// </remarks>
    public bool IsPlayerNameTaken(string name)
    {
        // TODO
        return false;
    }
}