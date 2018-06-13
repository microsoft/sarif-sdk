// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using FluentAssertions;
using Microsoft.Sarif.Viewer.ErrorList;
using Xunit;

namespace Microsoft.Sarif.Viewer.VisualStudio.UnitTests
{
    public class SarifSnapshotTests
    {
        [Fact]
        public void SarifSnapshot_TryGetValue_LineNumber()
        {
            int lineNumber = 10;

            SarifErrorListItem errorItem = new SarifErrorListItem();
            errorItem.LineNumber = lineNumber;

            SarifSnapshot snapshot = new SarifSnapshot(String.Empty, new List<SarifErrorListItem>() { errorItem });

            Object value;
            snapshot.TryGetValue(0, "line", out value).Should().Be(true);
            value.Should().Be(lineNumber - 1);
        }

        [Fact]
        public void SarifSnapshot_TryGetValue_NoLineNumber()
        {
            SarifErrorListItem errorItem = new SarifErrorListItem();

            SarifSnapshot snapshot = new SarifSnapshot(String.Empty, new List<SarifErrorListItem>() { errorItem });

            Object value;
            snapshot.TryGetValue(0, "line", out value).Should().Be(true);
            value.Should().Be(-1);
        }

        [Fact]
        public void SarifSnapshot_TryGetValue_NegativeLineNumber()
        {
            int lineNumber = -10;

            SarifErrorListItem errorItem = new SarifErrorListItem();
            errorItem.LineNumber = lineNumber;

            SarifSnapshot snapshot = new SarifSnapshot(String.Empty, new List<SarifErrorListItem>() { errorItem });

            Object value;
            snapshot.TryGetValue(0, "line", out value).Should().Be(true);
            value.Should().Be(lineNumber - 1);
        }
    }
}
