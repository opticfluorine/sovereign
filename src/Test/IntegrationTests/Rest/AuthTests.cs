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

using System.Net;
using System.Net.Http.Json;
using System.Text;
using Sovereign.EngineCore.Network.Rest;
using Sovereign.IntegrationTests.Fixtures;
using Sovereign.NetworkCore.Network.Rest.Data;

namespace Sovereign.IntegrationTests.Rest;

public class AuthTests : IClassFixture<ServerFixture>
{
    private readonly HttpClient client;
    private readonly ServerFixture fixture;

    public AuthTests(ServerFixture fixture)
    {
        this.fixture = fixture;
        // Assume SovereignServer runs on localhost:5000 for tests
        client = new HttpClient { BaseAddress = new Uri("http://localhost:5000") };
    }

    /// <summary>
    ///     Registers a new test account using the REST API.
    /// </summary>
    /// <param name="username">Username for the new account.</param>
    /// <param name="password">Password for the new account.</param>
    private async Task RegisterTestAccount(string username, string password)
    {
        var registration = new RegistrationRequest { Username = username, Password = password };
        var response = await client.PostAsJsonAsync(RestEndpoints.AccountRegistration, registration);
        response.EnsureSuccessStatusCode();
    }

    /// <summary>
    ///     Verifies that logging in with valid credentials returns an API token.
    /// </summary>
    [Fact]
    public async Task Login_WithValidCredentials_ReturnsApiToken()
    {
        var username = "testuser";
        var password = "testpass";
        await RegisterTestAccount(username, password);
        var request = new LoginRequest { Username = username, Password = password };
        var response = await client.PostAsJsonAsync(RestEndpoints.Authentication, request);
        response.EnsureSuccessStatusCode();
        var loginResponse = await response.Content.ReadFromJsonAsync<LoginResponse>();
        Assert.False(string.IsNullOrEmpty(loginResponse?.RestApiKey));
    }

    /// <summary>
    ///     Verifies that logging in with invalid credentials returns Unauthorized.
    /// </summary>
    [Fact]
    public async Task Login_WithInvalidCredentials_ReturnsUnauthorized()
    {
        var request = new LoginRequest { Username = "baduser", Password = "badpass" };
        var response = await client.PostAsJsonAsync(RestEndpoints.Authentication, request);
        Assert.False(response.IsSuccessStatusCode);
    }

    /// <summary>
    ///     Verifies that accessing an authorized endpoint without a token returns Unauthorized.
    /// </summary>
    [Fact]
    public async Task AuthorizedEndpoint_RequiresToken()
    {
        var response = await client.GetAsync(RestEndpoints.Player);
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    /// <summary>
    ///     Verifies that accessing an authorized endpoint with a valid Basic auth token succeeds.
    /// </summary>
    [Fact]
    public async Task AuthorizedEndpoint_WithToken_Succeeds()
    {
        var username = "testuser2";
        var password = "testpass2";
        await RegisterTestAccount(username, password);
        var loginRequest = new LoginRequest { Username = username, Password = password };
        var loginResponse = await (await client.PostAsJsonAsync(RestEndpoints.Authentication, loginRequest))
            .Content.ReadFromJsonAsync<LoginResponse>();
        Assert.False(string.IsNullOrEmpty(loginResponse?.RestApiKey));
        Assert.False(string.IsNullOrEmpty(loginResponse?.UserId));
        // Create Basic auth header
        var basicToken =
            Convert.ToBase64String(Encoding.UTF8.GetBytes($"{loginResponse.UserId}:{loginResponse.RestApiKey}"));
        var request = new HttpRequestMessage(HttpMethod.Get, RestEndpoints.Player);
        request.Headers.Add("Authorization", $"Basic {basicToken}");
        var response = await client.SendAsync(request);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    /// <summary>
    ///     Verifies that accessing an authorized endpoint with a garbage Basic auth token returns Unauthorized.
    /// </summary>
    [Fact]
    public async Task AuthorizedEndpoint_WithGarbageToken_ReturnsUnauthorized()
    {
        var guid = Guid.NewGuid().ToString();
        var garbageToken = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{guid}:garbage"));
        var request = new HttpRequestMessage(HttpMethod.Get, RestEndpoints.Player);
        request.Headers.Add("Authorization", $"Basic {garbageToken}");
        var response = await client.SendAsync(request);
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}