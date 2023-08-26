/*
 * Sovereign Engine
 * Copyright (c) 2023 opticfluorine
 *
 * Permission is hereby granted, free of charge, to any person obtaining a 
 * copy of this software and associated documentation files (the "Software"), 
 * to deal in the Software without restriction, including without limitation 
 * the rights to use, copy, modify, merge, publish, distribute, sublicense, 
 * and/or sell copies of the Software, and to permit persons to whom the 
 * Software is furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER 
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING 
 * FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
 * DEALINGS IN THE SOFTWARE.
 */

using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using Sovereign.NetworkCore.Network;

namespace Sovereign.ClientCore.Network.Rest;

/// <summary>
///     Provides a common interface for interacting with the REST server from the client.
/// </summary>
public sealed class RestClient
{
    /// <summary>
    ///     HTTP client instance.
    /// </summary>
    private readonly HttpClient httpClient = new();

    /// <summary>
    ///     Base URI for the REST server.
    /// </summary>
    private Uri baseUri;

    /// <summary>
    ///     Flag indicating whether the REST client should be considered "connected".
    /// </summary>
    public bool Connected { get; private set; }

    /// <summary>
    ///     Selects the REST server to use for all future requests. This additionally sets
    ///     the REST client state to "connected".
    /// </summary>
    /// <param name="connectionParameters">Updated connection parameters to use.</param>
    public void SelectServer(ClientConnectionParameters connectionParameters)
    {
        var builder = new UriBuilder();
        builder.Scheme = connectionParameters.RestTls ? "https" : "http";
        builder.Host = connectionParameters.RestHost;
        builder.Port = connectionParameters.RestPort;
        baseUri = builder.Uri;

        Connected = true;
    }

    /// <summary>
    ///     Sets the credentials to use for future requests.
    /// </summary>
    /// <param name="accountId">Account ID.</param>
    /// <param name="apiKey">API key.</param>
    public void SetCredentials(Guid accountId, string apiKey)
    {
        // Pack the account ID and API key into an HTTP Basic authentication string.
        var sb = new StringBuilder().Append(accountId.ToString()).Append(":").Append(apiKey);
        var token = Convert.ToBase64String(Encoding.UTF8.GetBytes(sb.ToString()));

        // Set the authorization header for future requests.
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", token);
    }

    /// <summary>
    ///     Sets the REST client to the disconnected state to prevent accidental requests
    ///     to the server after the session has ended.
    /// </summary>
    /// If the REST client is not in the connected state, this method does nothing.
    public void Disconnect()
    {
        httpClient.DefaultRequestHeaders.Authorization = null;
        Connected = false;
    }

    /// <summary>
    ///     Asynchronously makes a GET request to the REST server.
    /// </summary>
    /// <param name="url">Relative URL of the REST endpoint.</param>
    /// <returns>Task awaiting the response.</returns>
    /// <exception cref="NetworkException">Thrown if the REST client is not in the connected state.</exception>
    public Task<HttpResponseMessage> Get(string url)
    {
        if (!Connected) throw new NetworkException("REST client is not connected.");
        var uri = new Uri(baseUri, url);
        return httpClient.GetAsync(uri);
    }

    /// <summary>
    ///     Asynchronously makes a POST request to the REST server with a JSON payload.
    /// </summary>
    /// <param name="url">Relative URL of the REST endpoint.</param>
    /// <param name="content">Request content.</param>
    /// <returns>Task awaiting the response.</returns>
    /// <exception cref="NetworkException">Thrown if the REST client is not in the connected state.</exception>
    public Task<HttpResponseMessage> PostJson<T>(string url, T content)
    {
        if (!Connected) throw new NetworkException("REST client is not connected.");
        var uri = new Uri(baseUri, url);
        var jsonContent = JsonContent.Create(content);
        jsonContent.Headers.ContentLength = jsonContent.ReadAsStream().Length;
        return httpClient.PostAsync(uri, jsonContent);
    }
}