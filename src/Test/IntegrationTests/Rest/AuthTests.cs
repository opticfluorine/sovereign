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
using LiteNetLib;
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

    /// <summary>
    ///     Verifies that a non-admin user cannot access an admin-only endpoint.
    /// </summary>
    [Fact]
    public async Task AdminEndpoint_WithNonAdminToken_ReturnsUnauthorized()
    {
        var username = "nonadminuser";
        var password = "nonadminpass";
        await RegisterTestAccount(username, password);
        var loginRequest = new LoginRequest { Username = username, Password = password };
        var loginResponse = await (await client.PostAsJsonAsync(RestEndpoints.Authentication, loginRequest))
            .Content.ReadFromJsonAsync<LoginResponse>();
        Assert.False(string.IsNullOrEmpty(loginResponse?.RestApiKey));
        Assert.False(string.IsNullOrEmpty(loginResponse?.UserId));

        // Attempt to access admin-only endpoint
        var basicToken =
            Convert.ToBase64String(Encoding.UTF8.GetBytes($"{loginResponse.UserId}:{loginResponse.RestApiKey}"));
        var request = new HttpRequestMessage(HttpMethod.Get, $"{RestEndpoints.TemplateEntities}/1");
        request.Headers.Add("Authorization", $"Basic {basicToken}");
        var response = await client.SendAsync(request);
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    /// <summary>
    ///     Tests that the connection handoff to the event server after a successful login works.
    /// </summary>
    [Fact]
    public async Task EventServerHandoff_ValidLogin_Successful()
    {
        // Create account and log in.
        var username = "testuser_evh";
        var password = "testpass_evh";
        await RegisterTestAccount(username, password);
        var loginRequest = new LoginRequest { Username = username, Password = password };
        var loginResponse = await (await client.PostAsJsonAsync(RestEndpoints.Authentication, loginRequest))
            .Content.ReadFromJsonAsync<LoginResponse>();
        Assert.False(string.IsNullOrEmpty(loginResponse?.RestApiKey));
        Assert.False(string.IsNullOrEmpty(loginResponse?.UserId));
        var basicToken =
            Convert.ToBase64String(Encoding.UTF8.GetBytes($"{loginResponse.UserId}:{loginResponse.RestApiKey}"));
        var request = new HttpRequestMessage(HttpMethod.Get, RestEndpoints.Player);
        request.Headers.Add("Authorization", $"Basic {basicToken}");
        var response = await client.SendAsync(request);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        // Now attempt connection to the event server using the handoff info.
        var listener = new EventBasedNetListener();
        var evClient = new NetManager(listener);
        evClient.Start();
        var peer = evClient.Connect("127.0.0.1", 12820, loginResponse.UserId);
        await Task.Delay(500);
        Assert.Equal(ConnectionState.Connected, peer.ConnectionState);
    }

    /// <summary>
    ///     Tests that event server connections are rejected if a bad account ID is used while the server is
    ///     tracking a pending handoff.
    /// </summary>
    [Fact]
    public async Task EventServerHandoff_ValidLogin_BadKey_Rejected()
    {
        // Create account and log in.
        var username = "testuser_evh2";
        var password = "testpass_evh2";
        await RegisterTestAccount(username, password);
        var loginRequest = new LoginRequest { Username = username, Password = password };
        var loginResponse = await (await client.PostAsJsonAsync(RestEndpoints.Authentication, loginRequest))
            .Content.ReadFromJsonAsync<LoginResponse>();
        Assert.False(string.IsNullOrEmpty(loginResponse?.RestApiKey));
        Assert.False(string.IsNullOrEmpty(loginResponse?.UserId));
        var basicToken =
            Convert.ToBase64String(Encoding.UTF8.GetBytes($"{loginResponse.UserId}:{loginResponse.RestApiKey}"));
        var request = new HttpRequestMessage(HttpMethod.Get, RestEndpoints.Player);
        request.Headers.Add("Authorization", $"Basic {basicToken}");
        var response = await client.SendAsync(request);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        // Now attempt connection to the event server using the handoff info.
        var listener = new EventBasedNetListener();
        var evClient = new NetManager(listener);
        evClient.Start();
        var peer = evClient.Connect("127.0.0.1", 12820, Guid.NewGuid().ToString());
        await Task.Delay(500);
        Assert.Equal(ConnectionState.Disconnected, peer.ConnectionState);
    }
}