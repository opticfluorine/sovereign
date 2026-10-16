// Sovereign Engine
// Copyright (c) 2026 opticfluorine
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

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Logging.Abstractions;
using Sovereign.EngineCore.Network;
using Sovereign.EngineCore.Systems.Data;
using Sovereign.NetworkCore.Network.Rest.Data;
using Xunit;

namespace Sovereign.ServerNetwork.Network.Rest.TemplateEntities;

/// <summary>
///     Tests for the entity data REST endpoint's typed response payload.
/// </summary>
public class TestEntityDataRestService
{
    private readonly FakeDataServices dataServices = new();

    private readonly EntityDataRestService service;

    public TestEntityDataRestService()
    {
        service = new EntityDataRestService(NullLogger<EntityDataRestService>.Instance, dataServices);
    }

    [Fact]
    public async Task EntityDataGet_ReturnsTypedPayload()
    {
        dataServices.Values[5] = new Dictionary<string, DataValue>
        {
            ["s"] = DataValue.FromString("hello"),
            ["i"] = DataValue.FromInt(3),
            ["v"] = DataValue.FromVector3(new Vector3(1.0f, 2.0f, 3.0f)),
            ["e"] = DataValue.FromEntityId(0x10)
        };

        var result = await service.EntityDataGet(5, new DefaultHttpContext());

        var content = Assert.IsType<ContentHttpResult>(result);
        Assert.Equal(200, content.StatusCode);

        var response = JsonSerializer.Deserialize<EntityDataResponse>(content.ResponseContent!, MessageConfig.JsonOptions);
        Assert.NotNull(response);

        Assert.True(response!.EntityData["s"].TryToDataValue(out var s));
        Assert.Equal(DataValueType.String, s.Type);

        Assert.True(response.EntityData["i"].TryToDataValue(out var i));
        Assert.Equal(DataValue.FromInt(3), i);

        Assert.True(response.EntityData["v"].TryToDataValue(out var v));
        Assert.Equal(new Vector3(1.0f, 2.0f, 3.0f), v.AsVector3());

        Assert.True(response.EntityData["e"].TryToDataValue(out var e));
        Assert.Equal(0x10UL, e.AsEntityId());
    }

    /// <summary>
    ///     Minimal fake data services that serves a fixed set of entity values.
    /// </summary>
    private sealed class FakeDataServices : IDataServices
    {
        /// <summary>
        ///     Entity values keyed by entity ID.
        /// </summary>
        public Dictionary<ulong, Dictionary<string, DataValue>> Values { get; } = new();

        public bool HasGlobal(string key)
        {
            return false;
        }

        public bool TryGetGlobal(string key, [NotNullWhen(true)] out string? value)
        {
            value = null;
            return false;
        }

        public bool TryGetGlobal(string key, out DataValue value)
        {
            value = default;
            return false;
        }

        public bool TryGetGlobal(string key, DataValueType type, out DataValue value)
        {
            value = default;
            return false;
        }

        public bool TryGetGlobalInt(string key, out int value)
        {
            value = 0;
            return false;
        }

        public bool TryGetGlobalBool(string key, out bool value)
        {
            value = false;
            return false;
        }

        public bool TryGetGlobalFloat(string key, out float value)
        {
            value = 0f;
            return false;
        }

        public bool TryGetGlobalVector3(string key, out Vector3 value)
        {
            value = Vector3.Zero;
            return false;
        }

        public bool TryGetGlobalEntityId(string key, out ulong value)
        {
            value = 0;
            return false;
        }

        public bool TryGetEntityKeyValue(ulong entityId, string key, [NotNullWhen(true)] out string? value)
        {
            value = null;
            return false;
        }

        public bool TryGetEntityKeyValueLocal(ulong entityId, string key, [NotNullWhen(true)] out string? value)
        {
            value = null;
            return false;
        }

        public bool TryGetEntityKeyValue(ulong entityId, string key, out DataValue value)
        {
            value = default;
            return false;
        }

        public bool TryGetEntityKeyValueLocal(ulong entityId, string key, out DataValue value)
        {
            value = default;
            return false;
        }

        public bool TryGetEntityKeyValue(ulong entityId, string key, DataValueType type, out DataValue value)
        {
            value = default;
            return false;
        }

        public bool TryGetEntityKeyValueInt(ulong entityId, string key, out int value)
        {
            value = 0;
            return false;
        }

        public bool TryGetEntityKeyValueBool(ulong entityId, string key, out bool value)
        {
            value = false;
            return false;
        }

        public bool TryGetEntityKeyValueFloat(ulong entityId, string key, out float value)
        {
            value = 0f;
            return false;
        }

        public bool TryGetEntityKeyValueVector3(ulong entityId, string key, out Vector3 value)
        {
            value = Vector3.Zero;
            return false;
        }

        public bool TryGetEntityKeyValueEntityId(ulong entityId, string key, out ulong value)
        {
            value = 0;
            return false;
        }

        public void GetEntityData(ulong entityId, Dictionary<string, DataValue> data)
        {
            data.Clear();
            if (Values.TryGetValue(entityId, out var values))
                foreach (var (key, value) in values)
                    data[key] = value;
        }

        public bool IsKeyReadOnly(string key)
        {
            return key.StartsWith("__");
        }
    }
}
