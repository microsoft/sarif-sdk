// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using FluentAssertions;
using Xunit;

namespace Microsoft.WorkItems
{
    public class ServiceProviderFactoryTests
    {
        [Fact]
        public void GetAppSettingsFilePath_DefaultFile()
        {
            ServiceProviderFactory.GetAppSettingsFilePath().Should().BeEquivalentTo("appsettings.json");
        }

        [Fact]
        public void CreateFilingTarget_OverrideConfigFile()
        {
            try
            {
                string customFileName = "customFile.json";
                Environment.SetEnvironmentVariable(ServiceProviderFactory.AppSettingsEnvironmentVariableName, customFileName);

                ServiceProviderFactory.GetAppSettingsFilePath().Should().BeEquivalentTo(customFileName);
            }
            finally
            {
                Environment.SetEnvironmentVariable(ServiceProviderFactory.AppSettingsEnvironmentVariableName, null);
            }
        }
    }
}
