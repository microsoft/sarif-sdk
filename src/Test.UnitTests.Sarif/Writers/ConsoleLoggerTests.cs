﻿// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;

using FluentAssertions;

using Xunit;

namespace Microsoft.CodeAnalysis.Sarif.Writers
{
    public class ConsoleLoggerTests
    {

        [Fact]
        public void ConsoleLogger_EmitsFailureNoneMessagesProperly()
        {
            // A failure level of none indicates a debug message.
            // The console logger will emit the tool name in cases 
            // where a scan target URI is not available (as uppercase).
            string tool = Guid.NewGuid().ToString().ToUpperInvariant();

            string message = Guid.NewGuid().ToString();

            var result = new Result
            {
                Level = FailureLevel.None,
                Message = new Message { Text = message },
            };


            var levels = new FailureLevelSet(new[] { FailureLevel.None });

            var logger = new ConsoleLogger(quietConsole: false,
                                           toolName: tool,
                                           levels: levels,
                                           kinds: BaseLogger.Fail)
            {
                CaptureOutput = true
            };

            logger.Log(null, result);

            string expected = $"{tool}: info {message}{Environment.NewLine}";
            logger.CapturedOutput.Should().Be(expected);
        }

        [Fact]
        public void ConsoleLogger_EmitsMessageWithCharOffsetRegion()
        {
            string tool = Guid.NewGuid().ToString();
            string message = Guid.NewGuid().ToString();
            string uriText = Guid.NewGuid().ToString();

            var uri = new Uri(uriText, UriKind.RelativeOrAbsolute);

            var result = new Result
            {
                Level = FailureLevel.Error,
                Message = new Message { Text = message },
                Locations = new[]
                {
                    new Location
                    {
                         PhysicalLocation = new PhysicalLocation
                         {
                             ArtifactLocation = new ArtifactLocation
                             {
                                 Uri = uri
                             },
                             Region = new Region
                             {
                                CharOffset = 1,
                                CharLength = 10
                             }
                         }
                    }
                }
            };

            var logger = new ConsoleLogger(quietConsole: false,
                                            toolName: tool,
                                            levels: BaseLogger.ErrorWarning,
                                            kinds: BaseLogger.Fail)
            {
                CaptureOutput = true
            };

            logger.Log(null, result);

            // TODO: we need to get rid of the (0) literal that indicates we
            // couldn't generate a location
            string expected = $"{uriText}(0): error {message}{Environment.NewLine}";
            logger.CapturedOutput.Should().Be(expected);
        }

        [Fact]
        public void ConsoleLogger_EmitsNotificationLocationMessageOrToolName()
        {
            string messageGuid = Guid.NewGuid().ToString();
            string exceptionMessage = "Exception message";
            string toolName = Guid.NewGuid().ToString();
            string uriGuid = Guid.NewGuid().ToString();
            var uri = new Uri(uriGuid, UriKind.RelativeOrAbsolute);

            var notification = new Notification
            {
                Message = new Message
                {
                    Text = messageGuid
                },

                Locations = new[]
                {
                    new Location
                    {
                         PhysicalLocation = new PhysicalLocation
                         {
                             ArtifactLocation = new ArtifactLocation
                             {
                                 Uri = uri
                             }
                         }
                    }
                },

                Exception = new ExceptionData
                {
                    Message = exceptionMessage
                }
            };

            // If a notification has one or more locations, the first one should be 
            // present as part of the console out message.
            string output = ConsoleLogger.FormatNotificationMessage(notification, toolName);
            output.Should().Contain(uriGuid);
            output.Should().NotContain(toolName);
            output.Should().Contain(messageGuid);
            output.Should().Contain(exceptionMessage);

            // In the absence of notification locations, the tool name should be 
            // present as part of the console out message.
            notification.Locations = null;
            output = ConsoleLogger.FormatNotificationMessage(notification, toolName);
            output.Should().NotContain(uriGuid);
            output.Should().Contain(toolName);
            output.Should().Contain(messageGuid);
            output.Should().Contain(exceptionMessage);
        }
    }
}
