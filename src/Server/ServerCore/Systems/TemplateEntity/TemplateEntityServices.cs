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

using System.Threading.Tasks;

namespace Sovereign.ServerCore.Systems.TemplateEntity;

/// <summary>
///     Public read API to the TemplateEntity system.
/// </summary>
public class TemplateEntityServices
{
    private readonly TemplateEntityDataGenerator dataGenerator;

    public TemplateEntityServices(TemplateEntityDataGenerator dataGenerator)
    {
        this.dataGenerator = dataGenerator;
    }

    /// <summary>
    ///     Gets an asynchronous task that yields the latest TemplateEntityData object
    ///     in compressed serialized form.
    /// </summary>
    /// <returns>Latest TemplateEntityData object in compressed serialized form.</returns>
    public Task<byte[]> GetLatestTemplateEntityData()
    {
        return dataGenerator.GetLatestTemplateEntityData();
    }
}