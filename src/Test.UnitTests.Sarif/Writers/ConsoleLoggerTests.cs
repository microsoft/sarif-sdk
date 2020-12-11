// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using System;

using FluentAssertions;

using Xunit;

namespace Microsoft.CodeAnalysis.Sarif.Writers
{
    public class ConsoleLoggerTests
    {
        [Fact]
        public void ConsoleLogger_EmitsMessageWithCharOffsetRegion()
        {
            string tool = Guid.NewGuid().ToString();
            string message = Guid.NewGuid().ToString();
            string uriText = Guid.NewGuid().ToString();

            Uri uri = new Uri(uriText, UriKind.RelativeOrAbsolute);

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

            var logger = new ConsoleLogger(verbose: false, toolName: tool)
            {
                CaptureOutput = true
            };

            logger.Log(null, result);

            string expected = $"{tool} error {uriText}";
        }

        [Fact]
        public void ConsoleLogger_EmitsNotificationLocationMessageOrToolName()
        {
            string messageGuid = Guid.NewGuid().ToString();
            string toolName = Guid.NewGuid().ToString();
            string uriGuid = Guid.NewGuid().ToString();
            Uri uri = new Uri(uriGuid, UriKind.RelativeOrAbsolute);

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
                }
            };

            // If a notification has one or more locations, the first one should be 
            // present as part of the console out message.
            string output = ConsoleLogger.FormatNotificationMessage(notification, toolName);
            output.Should().Contain(uriGuid);
            output.Should().NotContain(toolName);
            output.Should().Contain(messageGuid);

            // In the absence of notification locations, the tool name should be 
            // present as part of the console out message.
            notification.Locations = null;
            output = ConsoleLogger.FormatNotificationMessage(notification, toolName);
            output.Should().NotContain(uriGuid);
            output.Should().Contain(toolName);
            output.Should().Contain(messageGuid);
        }
    }
}
