// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.CodeAnalysis.Sarif.WorkItemFiling
{
    /// <summary>
    /// An interface that defines a facade around the GitHub services needed by
    /// the work item filer.
    /// </summary>
    /// <remarks>
    /// This interface exists to make it possible to mock GitHub in unit tests.
    /// </remarks>
    public interface IGitHubClient
    {
    }
}
