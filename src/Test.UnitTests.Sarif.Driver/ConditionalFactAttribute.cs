// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Xunit;

namespace Test.UnitTests.Sarif.Driver
{
    /// <summary>
    /// A Fact attribute that conditionally skips tests based on the specified conditions.
    /// </summary>
    internal class ConditionalFactAttribute : FactAttribute
    {
        /// <summary>
        /// Conditionally skip a test fact based on the specified target frameworks.
        /// </summary>
        /// <param name="skipOnFrameworks">Array of framework monikers (e.g., "net472", "net48") on which to skip the test.</param>
        /// <param name="reason">Optional reason for skipping the test. If not provided, a default message will be used.</param>
        public ConditionalFactAttribute(string[] skipOnFrameworks, string reason = null)
            : base()
        {
            if (skipOnFrameworks == null || skipOnFrameworks.Length == 0)
            {
                return;
            }

#if NET48
            if (System.Linq.Enumerable.Contains(skipOnFrameworks, "net48", System.StringComparer.OrdinalIgnoreCase))
            {
                Skip = reason ?? "Test is skipped on NET48";
            }
#endif
        }
    }
}
