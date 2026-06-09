// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;

using FluentAssertions;

using Microsoft.CodeAnalysis.Sarif.Multitool.Rules;

using Xunit;

namespace Microsoft.CodeAnalysis.Sarif.Multitool
{
    public class UrisShouldBeReachableTests
    {
        private static bool IsReserved(string uri)
            => UrisShouldBeReachable.IsReservedDocumentationHost(new Uri(uri, UriKind.Absolute));

        [Fact]
        public void IsReservedDocumentationHost_ExemptsExampleOrgAzureDevOpsRepository()
        {
            IsReserved("https://dev.azure.com/example-org/example-project/_git/sarif-sdk").Should().BeTrue();
        }

        [Fact]
        public void IsReservedDocumentationHost_ExemptsExampleAzureDevOpsRepository()
        {
            IsReserved("https://dev.azure.com/example/example-project/_git/sarif-sdk").Should().BeTrue();
        }

        [Fact]
        public void IsReservedDocumentationHost_DoesNotExemptRealAzureDevOpsRepository()
        {
            IsReserved("https://dev.azure.com/contoso/widgets/_git/widgets").Should().BeFalse();
        }

        [Fact]
        public void IsReservedDocumentationHost_DoesNotExemptOrgMerelyPrefixedWithExample()
        {
            // The carve-out is an exact org match, not a prefix: a real org named "example-corp"
            // must still be probed.
            IsReserved("https://dev.azure.com/example-corp/widgets/_git/widgets").Should().BeFalse();
        }

        [Fact]
        public void IsReservedDocumentationHost_DoesNotExemptExampleOrgOnGitHubHost()
        {
            // The Azure DevOps org carve-out is host-scoped to dev.azure.com; a github.com owner
            // named "example-org" is a real reachable repository.
            IsReserved("https://github.com/example-org/sarif-sdk").Should().BeFalse();
        }

        [Fact]
        public void IsReservedDocumentationHost_ExemptsRfc2606DocumentationHost()
        {
            IsReserved("https://www.example.com/help").Should().BeTrue();
        }

        [Fact]
        public void IsReservedDocumentationHost_DoesNotExemptRealHost()
        {
            IsReserved("https://github.com/microsoft/sarif-sdk").Should().BeFalse();
        }
    }
}
