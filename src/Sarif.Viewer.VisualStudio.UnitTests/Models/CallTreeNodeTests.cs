// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Globalization;
using FluentAssertions;
using Microsoft.CodeAnalysis.Sarif;
using Microsoft.Sarif.Viewer.Converters;
using Microsoft.Sarif.Viewer.Models;
using Xunit;

namespace Microsoft.Sarif.Viewer.VisualStudio.Converters.UnitTests
{
    public class CallTreeNodeTests
    {
        [Fact]
        public void CallTreeNode_DefaultHighlightColor()
        {
            var callTreeNode = new CallTreeNode
            {
                Location = new AnnotatedCodeLocation(),
            };

            callTreeNode.Location.Importance = AnnotatedCodeLocationImportance.Essential;
            callTreeNode.DefaultSourceHighlightColor.Should().Be("CodeAnalysisKeyEventSelection");

            callTreeNode.Location.Importance = AnnotatedCodeLocationImportance.Important;
            callTreeNode.DefaultSourceHighlightColor.Should().Be("CodeAnalysisLineTraceSelection");

            callTreeNode.Location.Importance = AnnotatedCodeLocationImportance.Unimportant;
            callTreeNode.DefaultSourceHighlightColor.Should().Be("CodeAnalysisLineTraceSelection");
        }

        [Fact]
        public void CallTreeNode_SelectedHighlightColor()
        {
            var callTreeNode = new CallTreeNode
            {
                Location = new AnnotatedCodeLocation(),
            };

            callTreeNode.Location.Importance = AnnotatedCodeLocationImportance.Essential;
            callTreeNode.SelectedSourceHighlightColor.Should().Be("CodeAnalysisCurrentStatementSelection");
        }
    }
}
