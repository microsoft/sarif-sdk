// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using Microsoft.CodeAnalysis.Sarif.VersionOne;

namespace Microsoft.CodeAnalysis.Sarif.Visitors
{
    public class SarifCurrentToVersionOneVisitor : SarifRewritingVisitor
    {
        public override SarifLog VisitSarifLog(SarifLog node)
        {
            SarifLogVersionOne = new SarifLogVersionOne();

            return base.VisitSarifLog(node);
        }

        public SarifLogVersionOne SarifLogVersionOne { get; private set; }
    }
}
