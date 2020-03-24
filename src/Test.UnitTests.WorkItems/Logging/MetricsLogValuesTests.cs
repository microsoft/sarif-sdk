// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using FluentAssertions;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.Extensions.Logging;
using Microsoft.TeamFoundation;
using Xunit;

namespace Microsoft.WorkItems.Logging
{
    public class MetricsLogValuesTests
    {
        [Fact]
        public void MetricsLogValues_ThrowsIfNullOrEmptyDictionary()
        {
            string message = "message";
            EventId eventId = new EventId(1);

            Action action = () => new MetricsLogValues(message, eventId, null);
            action.Should().Throw<ArgumentOutOfRangeException>();

            action = () => new MetricsLogValues(message, eventId, new Dictionary<string, object>());
            action.Should().Throw<ArgumentOutOfRangeException>();
        }

        [Fact]
        public void MetricsLogValues_ToStringPreference()
        {
            string message = "Original";
            EventId eventId = new EventId(1);
            Dictionary<string, object> customDimensions = new Dictionary<string, object>();
            customDimensions.Add("EventId", eventId.Id);

            // Prefer original message if it's provided
            MetricsLogValues values = new MetricsLogValues(message, eventId, customDimensions);
            values.ToString().Should().Be(message);

            // Next prefer the eventId if it's provided
            message = null;
            values = new MetricsLogValues(message, eventId, customDimensions);
            values.ToString().Should().Contain(eventId.Id.ToString());
        }

        [Fact]
        public void MetricsLogValues_ContainsAllCustomDimensions()
        {
            string message = "Original";
            EventId eventId = new EventId(1);
            Dictionary<string, object> customDimensions = new Dictionary<string, object>();
            customDimensions.Add("One", 1);
            customDimensions.Add("Two", 2);
            customDimensions.Add("Three", 3);

            MetricsLogValues values = new MetricsLogValues(message, eventId, customDimensions);

            values.Count.Should().Be(customDimensions.Count);

            foreach (KeyValuePair<string, object> kvp in values)
            {
                kvp.Value.Should().Be(customDimensions[kvp.Key]);
            }
        }
    }
}
