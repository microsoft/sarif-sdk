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

        OptionallyEmittedData _dataToRemove;

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
    }
}
