// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using Moq;
using Xunit;

namespace Microsoft.Sarif.Viewer.VisualStudio.UnitTests
{
    public class TelemetryProviderTests
    {
        private Mock<ITelemetryChannel> InitializeTelemetryProvider()
        {
            var mockChannel = new Mock<ITelemetryChannel>();
            TelemetryConfiguration configuration = new TelemetryConfiguration()
            {
                TelemetryChannel = mockChannel.Object,
                InstrumentationKey = Guid.Empty.ToString()
            };
            TelemetryProvider.Reset();
            TelemetryProvider.Initialize(configuration);

            return mockChannel;
        }

        [Fact]
        public void WriteMenuCommandEvent_ToolFormat_GetsSent()
        {
            // Arrange
            string format = "SARIF";
            EventTelemetry item = new EventTelemetry(TelemetryEvent.LogFileOpenedByMenuCommand.ToString());
            item.Properties.Add("Format", format);
            Mock<ITelemetryChannel> mockChannel = InitializeTelemetryProvider();
            mockChannel.Setup(m => m.Send(item));
            mockChannel.Setup(m => m.Flush());

            // Act
            TelemetryProvider.WriteMenuCommandEvent(format);

            // Assert
            mockChannel.Verify(c => c.Send(It.Is<EventTelemetry>(t => t.Name == item.Name &&
                                                                      t.Properties["Format"] == format)));
            mockChannel.Verify(c => c.Flush());
        }

        [Fact]
        public void WriteEvent_EventName_GetsSent()
        {
            // Arrange
            EventTelemetry item = new EventTelemetry(TelemetryEvent.ViewerExtensionLoaded.ToString());
            Mock<ITelemetryChannel> mockChannel = InitializeTelemetryProvider();
            mockChannel.Setup(m => m.Send(item));
            mockChannel.Setup(m => m.Flush());

            // Act
            TelemetryProvider.WriteEvent(TelemetryEvent.ViewerExtensionLoaded);

            // Assert
            mockChannel.Verify(c => c.Send(It.Is<EventTelemetry>(t => t.Name == item.Name)));
            mockChannel.Verify(c => c.Flush());
        }

        [Fact]
        public void WriteEvent_EventNameAndData_GetsSent()
        {
            // Arrange
            string data = "The quick brown fox";
            EventTelemetry item = new EventTelemetry(TelemetryEvent.ViewerExtensionLoaded.ToString());
            item.Properties.Add("Data", data);
            Mock<ITelemetryChannel> mockChannel = InitializeTelemetryProvider();
            mockChannel.Setup(m => m.Send(item));
            mockChannel.Setup(m => m.Flush());

            // Act
            TelemetryProvider.WriteEvent(TelemetryEvent.ViewerExtensionLoaded, data);

            // Assert
            mockChannel.Verify(c => c.Send(It.Is<EventTelemetry>(t => t.Properties["Data"] == data)));
            mockChannel.Verify(c => c.Flush());
        }

        [Fact]
        public void WriteEvent_EventNameAndKeyValuePairs_GetsSent()
        {
            // Arrange
            List<KeyValuePair<string, string>> pairs = new List<KeyValuePair<string, string>>()
            {
                TelemetryProvider.CreateKeyValuePair("Key1", "Value1"),
                TelemetryProvider.CreateKeyValuePair("Key2", "Value2")
            };
            EventTelemetry item = new EventTelemetry(TelemetryEvent.ViewerExtensionLoaded.ToString());
            pairs.ForEach(p => item.Properties.Add(p));
            Mock<ITelemetryChannel> mockChannel = InitializeTelemetryProvider();
            mockChannel.Setup(m => m.Send(item));
            mockChannel.Setup(m => m.Flush());

            // Act
            TelemetryProvider.WriteEvent(TelemetryEvent.ViewerExtensionLoaded, pairs.ToArray());

            // Assert
            mockChannel.Verify(c => c.Send(It.Is<EventTelemetry>(t => t.Properties["Key1"] == "Value1" &&
                                                                      t.Properties["Key2"] == "Value2")));
            mockChannel.Verify(c => c.Flush());
        }

        [Fact]
        public void WriteEvent_EventNameAndDictionary_GetsSent()
        {
            // Arrange
            List<KeyValuePair<string, string>> pairs = new List<KeyValuePair<string, string>>()
            {
                TelemetryProvider.CreateKeyValuePair("Key1", "Value1"),
                TelemetryProvider.CreateKeyValuePair("Key2", "Value2")
            };
            Dictionary<string, string> dictionary = pairs.ToDictionary(x => x.Key, x => x.Value);
            EventTelemetry item = new EventTelemetry(TelemetryEvent.ViewerExtensionLoaded.ToString());
            pairs.ForEach(p => item.Properties.Add(p));
            Mock<ITelemetryChannel> mockChannel = InitializeTelemetryProvider();
            mockChannel.Setup(m => m.Send(item));
            mockChannel.Setup(m => m.Flush());

            // Act
            TelemetryProvider.WriteEvent(TelemetryEvent.ViewerExtensionLoaded, dictionary);

            // Assert
            mockChannel.Verify(c => c.Send(It.Is<EventTelemetry>(t => t.Properties["Key1"] == "Value1" &&
                                                                      t.Properties["Key2"] == "Value2")));
            mockChannel.Verify(c => c.Flush());
        }

        [Fact]
        public void CreateKeyValuePair_Succeeds()
        {
            // Arrange
            var control = new KeyValuePair<string, string>("Keymaster", "Gatekeeper");

            // Act
            var result = TelemetryProvider.CreateKeyValuePair("Keymaster", "Gatekeeper");

            // Assert
            Assert.Equal(control.Key, result.Key);
            Assert.Equal(control.Value, result.Value);
        }

        [Fact]
        public void WriteMenuCommandEvent_EmptyToolFormatParam_ThrowsArgNullException()
        {
            // Arrange
            // Act
            // Assert
            Assert.Throws<ArgumentNullException>(() => TelemetryProvider.WriteMenuCommandEvent(string.Empty));
        }

        [Fact]
        public void WriteEvent_EmptyDataParam_ThrowsArgNullException()
        {
            // Arrange
            // Act
            // Assert
            Assert.Throws<ArgumentNullException>(() => TelemetryProvider.WriteEvent(TelemetryEvent.ViewerExtensionLoaded, string.Empty));
        }

        [Fact]
        public void CreateKeyValuePair_EmptyKeyParam_ThrowsArgNullException()
        {
            // Arrange
            // Act
            // Assert
            Assert.Throws<ArgumentNullException>(() => TelemetryProvider.CreateKeyValuePair(string.Empty, "Value"));
        }

        [Fact]
        public void CreateKeyValuePair_EmptyValueParam_ThrowsArgNullException()
        {
            // Arrange
            // Act
            // Assert
            Assert.Throws<ArgumentNullException>(() => TelemetryProvider.CreateKeyValuePair("Key", string.Empty));
        }
    }
}
