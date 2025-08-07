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

using MessagePack;

namespace Sovereign.EngineCore.Events.Details;

[MessagePackObject]
public class DialogueEventDetails : IEventDetails
{
    /// <summary>
    ///     Maximum subject length.
    /// </summary>
    public const int MaxSubjectLength = 64;

    /// <summary>
    ///     Maximum message length.
    /// </summary>
    public const int MaxMessageLength = 400;

    /// <summary>
    ///     Subject of the dialogue message (e.g. who is speaking).
    /// </summary>
    [Key(0)]
    public string Subject { get; set; } = string.Empty;

    /// <summary>
    ///     Dialgoue message.
    /// </summary>
    [Key(1)]
    public string Message { get; set; } = string.Empty;

    /// <summary>
    ///     Event ID of recipient. Only used on the server side.
    /// </summary>
    [IgnoreMember]
    public ulong TargetEntityId { get; set; }
}