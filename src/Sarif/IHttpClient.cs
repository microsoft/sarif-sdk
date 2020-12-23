// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.CodeAnalysis.Sarif
{
    /// <summary>
    /// An interface for accessing the http client.
    /// </summary>
    /// <remarks>
    /// Clients wishing to send requests and retrieve responses using http protocol should use
    /// wrapper object rather than directly using the .NET HttpClient and other classes,
    /// so they can mock the IHttpClient interface in unit tests.
    /// </remarks>
    public interface IHttpClient
    {
        /// <summary>
        /// Send a GET request to the specified Uri as an asynchronous operation.
        /// </summary>
        /// <param name="requestUri">The Uri the request is sent to.</param>
        /// <returns>The task object representing the asynchronous operation.</returns>
        Task<HttpResponseMessage> GetAsync(string requestUri);
    }
}
