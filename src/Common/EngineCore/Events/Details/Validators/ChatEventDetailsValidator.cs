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

using System.Text.RegularExpressions;
using Sovereign.EngineCore.Configuration;

namespace Sovereign.EngineCore.Events.Details.Validators;

/// <summary>
///     Event details validator for ChatEventDetails.
/// </summary>
public class ChatEventDetailsValidator : IEventDetailsValidator
{
    private static readonly Regex ValidRegex = new(@"^[\S ]+$");

    public bool IsValid(IEventDetails? details)
    {
        if (details is not ChatEventDetails chatDetails) return false;
        return chatDetails.Message.Length <= ChatConstants.MaxMessageLengthChars
               && ValidRegex.IsMatch(chatDetails.Message);
    }
}