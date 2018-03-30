// Copyright (c) Microsoft. All rights reserved. 
// Licensed under the MIT license. See LICENSE file in the project root for full license information. 

using System;
using Microsoft.CodeAnalysis.Sarif;
using Microsoft.Sarif.Viewer.ErrorList;
using Moq;

namespace Microsoft.Sarif.Viewer.VisualStudio.UnitTests
{
    public static class TestUtilities
    {
        public static void InitializeTestEnvironment()
        {
            SarifViewerPackage.IsUnitTesting = true;

            // While loading test SARIF objects, the SarifViewerPackage.ServiceProvider object
            // is checked for not being null. This value is not actually used in any tests. In
            // production code, the object is always created before any SARIF files are read.
            Mock<IServiceProvider> mockServiceProvider = new Mock<IServiceProvider>();
            SarifViewerPackage.ServiceProvider = mockServiceProvider.Object;
        }

        public static void InitializeTestEnvironment(SarifLog sarifLog)
        {
            InitializeTestEnvironment();

            ErrorListService.ProcessSarifLog(sarifLog, "", null);
        }
    }
}
