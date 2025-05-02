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

using System;

namespace Sovereign.EngineCore.Systems.Data;

/// <summary>
///     Exposes an event-based interface for receiving notifications of data changes.
/// </summary>
public interface IDataChangeNotifier
{
    /// <summary>
    ///     Event that signals a global key-value pair has been added.
    /// </summary>
    event Action<string, string> GlobalAdded;

    /// <summary>
    ///     Event that signals a global key-value pair has been modified.
    /// </summary>
    event Action<string, string> GlobalModified;

    /// <summary>
    ///     Event that signals a global key-value pair has been removed.
    /// </summary>
    event Action<string> GlobalRemoved;
}

/// <summary>
///     Internal interface for notifying data changes.
/// </summary>
internal interface IDataChangeNotifierInternal
{
    /// <summary>
    ///     Notifies that a global key-value pair has been added.
    /// </summary>
    /// <param name="key">Key.</param>
    /// <param name="value">Value.</param>
    void NotifyGlobalAdded(string key, string value);

    /// <summary>
    ///     Notifies that a global key-value pair has been modified.
    /// </summary>
    /// <param name="key">Key.</param>
    /// <param name="value">Value.</param>
    void NotifyGlobalModified(string key, string value);

    /// <summary>
    ///     Notifies that a global key-value pair has been removed.
    /// </summary>
    /// <param name="key">Key.</param>
    void NotifyGlobalRemoved(string key);
}

/// <summary>
///     Implementation of IDataChangeNotifier.
/// </summary>
internal class DataChangeNotifier : IDataChangeNotifier, IDataChangeNotifierInternal
{
    public event Action<string, string>? GlobalAdded;
    public event Action<string, string>? GlobalModified;
    public event Action<string>? GlobalRemoved;

    public void NotifyGlobalAdded(string key, string value)
    {
        GlobalAdded?.Invoke(key, value);
    }

    public void NotifyGlobalModified(string key, string value)
    {
        GlobalModified?.Invoke(key, value);
    }

    public void NotifyGlobalRemoved(string key)
    {
        GlobalRemoved?.Invoke(key);
    }
}