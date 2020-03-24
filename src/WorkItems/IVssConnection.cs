// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Threading.Tasks;

namespace Microsoft.WorkItems
{
    /// <summary>
    /// This interface allows for mocking of the low-level VssConnection class.
    /// </summary>
    internal interface IVssConnection: IDisposable
    {
        /// <summary>
        /// Provide for both the instantiation of the connection instance followed by
        /// a call to the VssConnection.ConnectAsync method.
        /// </summary>
        /// <param name="accountUri">The AzureDevOps account URI for the connection.</param>
        /// <param name="personalAccessToken">A personal access token with sufficient permissions to establish the connection.</param>
        /// <returns></returns>
        Task ConnectAsync(Uri accountUri, string personalAccessToken);

        /// <summary>
        ///  Interface abstraction for VssConnection.GetClientAsync
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        Task<IWorkItemTrackingHttpClient> GetClientAsync();
    }
}