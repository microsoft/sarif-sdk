// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using System;

namespace Microsoft.CodeAnalysis.Sarif.Visitors
{
    public class RemoveOptionalDataVisitor : SarifRewritingVisitor
    {
        public RemoveOptionalDataVisitor(OptionallyEmittedData optionallyEmittedData)
        {
            _dataToRemove = optionallyEmittedData;
        }

        readonly OptionallyEmittedData _dataToRemove;

        public override Invocation VisitInvocation(Invocation node)
        {
            if (_dataToRemove.HasFlag(OptionallyEmittedData.NondeterministicProperties))
            {
                node.StartTimeUtc = new DateTime();
                node.EndTimeUtc = new DateTime();
            }

            return base.VisitInvocation(node);
        }

        public override Notification VisitNotification(Notification node)
        {
            if (_dataToRemove.HasFlag(OptionallyEmittedData.NondeterministicProperties))
            {
                node.TimeUtc = new DateTime();
            }

            return base.VisitNotification(node);
        }

        public override PhysicalLocation VisitPhysicalLocation(PhysicalLocation node)
        {
            if (_dataToRemove.HasFlag(OptionallyEmittedData.ContextRegionSnippets) && node.ContextRegion != null)
            {
                node.ContextRegion.Snippet = null;
            }

            if (_dataToRemove.HasFlag(OptionallyEmittedData.RegionSnippets) && node.Region != null)
            {
                node.Region.Snippet = null;
            }

            return base.VisitPhysicalLocation(node);
        }

        public override Artifact VisitArtifact(Artifact node)
        {
            if (_dataToRemove.HasFlag(OptionallyEmittedData.BinaryFiles) && node.Contents?.Binary != null)
            {
                node.Contents.Binary = null;
            }

            if (_dataToRemove.HasFlag(OptionallyEmittedData.TextFiles) && node.Contents?.Text != null)
            {
                node.Contents.Text = null;
            }

            return base.VisitArtifact(node);
        }

        public override Result VisitResult(Result node)
        {
            if (_dataToRemove.HasFlag(OptionallyEmittedData.Guids))
            {
                node.Guid = null;
            }

            return base.VisitResult(node);
        }
    }
}
