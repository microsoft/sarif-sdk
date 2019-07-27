// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;

namespace Microsoft.CodeAnalysis.Sarif.WorkItemFiling
{
    public static class Extensions
    {
        public static string GetProjectName(this Uri projectUri)
        {
            string projectUriString = projectUri.OriginalString;
            int lastSlashIndex = projectUriString.LastIndexOf('/');

            string projectName = lastSlashIndex > 0 && lastSlashIndex < projectUriString.Length - 1
                ? projectUriString.Substring(lastSlashIndex + 1)
                : throw new ArgumentException($"Cannot parse project name from URI {projectUriString}");

            return projectName;
        }

        public static string GetAccountUriString(this Uri projectUri)
        {
            string projectUriString = projectUri.OriginalString;
            int lastSlashIndex = projectUriString.LastIndexOf('/');

            string accountUriString = projectUriString.Substring(0, lastSlashIndex);

            return accountUriString;
        }
    }
}
