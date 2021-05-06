﻿// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Concurrent;
using System.Net.Http;
using System.Threading.Tasks;

namespace Microsoft.CodeAnalysis.Sarif
{
    /// <summary>
    /// A wrapper class for accessing the .NET http client.
    /// </summary>
    /// <remarks>
    /// Clients should use this class rather directly using the .NET http client classes, so they
    /// can mock the IHttpClient interface in unit tests.
    /// </remarks>
    public class HttpClientWrapper : IHttpClient
    {
        // .NET http client 
        private readonly HttpClient httpClient;

        /// <summary>
        /// Allow HttpClient to be injected, can accepted httpclient with mocked HttpMessageHandler for easier unit testing
        /// </summary>
        /// <param name="httpClient"></param>
        public HttpClientWrapper(HttpClient httpClient = null)
        {
            this.httpClient = httpClient ?? new HttpClient();
        }

        /// <summary>
        /// Send a GET request to the specified Uri as an asynchronous operation
        /// if cache doesn't have this Uri entry, otherwize return it from cache
        /// </summary>
        /// <param name="requestUri">The Uri the request is sent to.</param>
        /// <returns>The task object representing the asynchronous operation.</returns>
        public Task<HttpResponseMessage> GetAsync(string requestUri)
        {
            return httpClient.GetAsync(requestUri);
        }

        /// <summary>
        /// Send a POST request to the specified Uri as an asynchronous operation.
        /// </summary>
        /// <param name="requestUri">The Uri the request is sent to.</param>
        /// <param name="content">The HTTP request content sent to the server.</param>
        /// <returns>The task object representing the asynchronous operation</returns>
        public Task<HttpResponseMessage> PostAsync(string requestUri, HttpContent content)
        {
            return httpClient.PostAsync(requestUri, content);
        }

        /// <summary>
        /// Send a PUT request to the specified Uri as an asynchronous operation.
        /// </summary>
        /// <param name="requestUri">The Uri the request is sent to.</param>
        /// <param name="content">The HTTP request content sent to the server.</param>
        /// <returns>The task object representing the asynchronous operation.</returns>
        public Task<HttpResponseMessage> PutAsync(string requestUri, HttpContent content)
        {
            return httpClient.PutAsync(requestUri, content);
        }
    }
}
