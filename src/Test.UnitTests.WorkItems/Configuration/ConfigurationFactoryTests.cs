// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using FluentAssertions;
using Microsoft.CodeAnalysis.WorkItems.Configuration;
using Xunit;

namespace Microsoft.WorkItems.Configuration
{
    public class ConfigurationFactoryTests
    {
        [Fact]
        public void Load()
        {
            SurffConfiguration config = ConfigurationFactory.Load(null, null, "binskim");
        }
    }
}
