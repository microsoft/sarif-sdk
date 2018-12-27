// Copyright (c) Microsoft. All rights reserved. Licensed under the MIT        
// license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis.Sarif.VersionOne;

namespace Microsoft.CodeAnalysis.Sarif.Visitors
{
    /// <summary>
    /// This class visits all locations in a SARIF v1 log file. On observing use of result.logicalLocationKey,
    /// which is used to disambiguate logical locations in the result.logicalLocations dictionary that have a 
    /// common fully qualified name (but which are different types), the visitor creates a mapping between
    /// the logical location key and its associated fully qualified name. This allows the v2 transformation
    /// in particular to more easily populate its logical location equivalents.
    /// </summary>
    public class VersionOneLogicalLocationKeyToFullyQualifiedNameMappingVisitor : SarifRewritingVisitorVersionOne
    {
        public VersionOneLogicalLocationKeyToFullyQualifiedNameMappingVisitor()
        {
            LogicalLocationKeyToFullyQualifiedNameMap = new Dictionary<string, string>();
        }

        public IDictionary<string, string> LogicalLocationKeyToFullyQualifiedNameMap { get; set; }

        public override LocationVersionOne VisitLocationVersionOne(LocationVersionOne node)
        {
            if (!string.IsNullOrEmpty(node.LogicalLocationKey) &&
                !string.IsNullOrEmpty(node.FullyQualifiedLogicalName) &&
                !node.FullyQualifiedLogicalName.Equals(node.LogicalLocationKey))
            {
                LogicalLocationKeyToFullyQualifiedNameMap[node.LogicalLocationKey] = node.FullyQualifiedLogicalName;
            }

            return base.VisitLocationVersionOne(node);
        }

        public override StackFrameVersionOne VisitStackFrameVersionOne(StackFrameVersionOne node)
        {
            if (!string.IsNullOrEmpty(node.LogicalLocationKey) &&
                !string.IsNullOrEmpty(node.FullyQualifiedLogicalName) &&
                !node.FullyQualifiedLogicalName.Equals(node.LogicalLocationKey))
            {
                LogicalLocationKeyToFullyQualifiedNameMap[node.LogicalLocationKey] = node.FullyQualifiedLogicalName;
            }

            return base.VisitStackFrameVersionOne(node);
        }

        public override AnnotatedCodeLocationVersionOne VisitAnnotatedCodeLocationVersionOne(AnnotatedCodeLocationVersionOne node)
        {
            if (!string.IsNullOrEmpty(node.LogicalLocationKey) &&
                !string.IsNullOrEmpty(node.FullyQualifiedLogicalName) &&
                !node.FullyQualifiedLogicalName.Equals(node.LogicalLocationKey))
            {
                LogicalLocationKeyToFullyQualifiedNameMap[node.LogicalLocationKey] = node.FullyQualifiedLogicalName;
            }

            return base.VisitAnnotatedCodeLocationVersionOne(node);
        }
    }
}
