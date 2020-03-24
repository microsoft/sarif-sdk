// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using FluentAssertions;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.TeamFoundation;
using Xunit;

namespace Microsoft.WorkItems.Logging
{
    public class ApplicationInsightsTelemetryInitializerTests
    {
        [Fact]
        public void CreateFilingTarget_ThrowsIfTelemetryIsNull()
        {
            ApplicationInsightsTelemetryInitializer initializer = new ApplicationInsightsTelemetryInitializer();

            Action action = () => initializer.Initialize(null);
            action.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void CreateFilingTarget_OperationIdIsSet()
        {
            TraceTelemetry telemetry = new TraceTelemetry();
            ApplicationInsightsTelemetryInitializer initializer = new ApplicationInsightsTelemetryInitializer();

            telemetry.Context.Operation.Id.Should().BeNull();

            // After initialization, the operation_Id should be set to a GUID.
            initializer.Initialize(telemetry);
            telemetry.Context.Operation.Id.Should().NotBeNull();
            Guid.TryParse(telemetry.Context.Operation.Id, out Guid temp).Should().BeTrue();

            // A second telemetry item should be assigned the same operation_Id
            TraceTelemetry telemetry2 = new TraceTelemetry();
            initializer.Initialize(telemetry2);
            telemetry.Context.Operation.Id.Should().Be(telemetry2.Context.Operation.Id);
        }

        [Fact]
        public void CreateFilingTarget_OperationIdIsDifferent()
        {
            // After initialization, the operation_Id should be set to a GUID.
            TraceTelemetry telemetry = new TraceTelemetry();
            ApplicationInsightsTelemetryInitializer initializer = new ApplicationInsightsTelemetryInitializer();

            telemetry.Context.Operation.Id.Should().BeNull();
            initializer.Initialize(telemetry);
            telemetry.Context.Operation.Id.Should().NotBeNull();
            Guid.TryParse(telemetry.Context.Operation.Id, out Guid temp).Should().BeTrue();

            // A second telemetry item using a different Initializer should be assigned a different operation_Id
            TraceTelemetry telemetry2 = new TraceTelemetry();
            ApplicationInsightsTelemetryInitializer initializer2 = new ApplicationInsightsTelemetryInitializer();

            telemetry2.Context.Operation.Id.Should().BeNull();
            initializer2.Initialize(telemetry2);
            telemetry2.Context.Operation.Id.Should().NotBeNull();
            Guid.TryParse(telemetry2.Context.Operation.Id, out Guid temp2).Should().BeTrue();

            telemetry.Context.Operation.Id.Should().NotBe(telemetry2.Context.Operation.Id);
        }

        [Fact]
        public void CreateFilingTarget_OperationIdShouldNotBeOverwritten()
        {
            TraceTelemetry telemetry = new TraceTelemetry();
            ApplicationInsightsTelemetryInitializer initializer = new ApplicationInsightsTelemetryInitializer();

            const string operationId = "abcd";
            telemetry.Context.Operation.Id = operationId;

            initializer.Initialize(telemetry);
            telemetry.Context.Operation.Id.Should().NotBeNull();
            telemetry.Context.Operation.Id.Should().Be(operationId);
        }
    }
}
