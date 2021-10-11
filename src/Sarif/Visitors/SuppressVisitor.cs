// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Globalization;

namespace Microsoft.CodeAnalysis.Sarif.Visitors
{
    public class SuppressVisitor : SarifRewritingVisitor
    {
        private readonly bool guids;
        private readonly string alias;
        private readonly bool timestamps;
        private readonly DateTime timeUtc;
        private readonly DateTime expiryUtc;
        private readonly int expiryInDays;
        private readonly string justification;
        private readonly SuppressionStatus suppressionStatus;

        public SuppressVisitor(string justification,
                               string alias,
                               bool guids,
                               bool timestamps,
                               int expiryInDays,
                               SuppressionStatus suppressionStatus)
        {
            this.alias = alias;
            this.guids = guids;
            this.timestamps = timestamps;
            this.timeUtc = DateTime.UtcNow;
            this.expiryInDays = expiryInDays;
            this.justification = justification;
            this.suppressionStatus = suppressionStatus;
            this.expiryUtc = this.timeUtc.AddDays(expiryInDays);
        }

        public override Result VisitResult(Result node)
        {
            if (node.Suppressions == null)
            {
                node.Suppressions = new List<Suppression>();
            }

            var suppression = new Suppression
            {
                Status = suppressionStatus,
                Justification = justification
            };

            if (!string.IsNullOrWhiteSpace(alias))
            {
                suppression.SetProperty(nameof(alias), alias);
            }

            if (guids)
            {
                suppression.SetProperty("guid", Guid.NewGuid());
            }

            if (timestamps)
            {
                suppression.SetProperty(nameof(timeUtc), timeUtc);
            }

            if (expiryInDays > 0)
            {
                suppression.SetProperty(nameof(expiryUtc), expiryUtc);
            }

            node.Suppressions.Add(suppression);
            return base.VisitResult(node);
        }
    }
}
