// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.CodeAnalysis.Test.Utilities.Sarif;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
using Microsoft.VisualStudio.Services.WebApi;
using Microsoft.VisualStudio.Services.WebApi.Patch.Json;
using Microsoft.WorkItems;
using Moq;
using Newtonsoft.Json;
using Xunit;

namespace Microsoft.CodeAnalysis.Sarif.WorkItems
{
    public class SarifWorkItemFilerTests
    {
        [Fact]
        public void WorkItemFiler_AzureDevOpsClientReceivesExpectedCalls()
        {
            // IMPORTANT. This is a single end-to-end test that demonstrates we have
            // the capability of mocking/injecting mocks into the end-to-end work
            // item filing scenario. This example works for the ADO filer only. Once
            // reviewed/finalized, we would look to provide an equivalent implementation
            // for the GitHub filer. The code below isn't intended to reflect the 
            // actual factoring we can expect when we use this machinery to cover
            // all expected future cases. i.e., after completing this first review,
            // we should look at more thoughtful factoring/helpers to capture 
            // testing for specific scenarios.

            // ONE. Create test data that the low-level ADO client mocks
            //      will flow back to the SARIF work item filer. 
            var attachmentReference = new AttachmentReference()
            {
                Id = Guid.NewGuid(),
                Url = Guid.NewGuid().ToString()
            };

            var workItem = new WorkItem
            {
                Id = DateTime.UtcNow.Millisecond,
                Links = new ReferenceLinks()
            };

            // The fake URI to the filed item that we'll expect the filer to receive
            string htmlUri = "https://example.com/" + Guid.NewGuid().ToString();
            workItem.Links.AddLink("html", htmlUri);

            // TWO. Define variables to capture whether we enter all expected ADO client methods.
            bool connectCalled = false,
                 createWorkItemCalled = false,
                 createAttachmenCalled = false;                 

            // THREE. Create a default mock SARIF filer, configured by an AzureDevOps context
            //        (which creates a default ADO filing client underneath).
            SarifWorkItemFiler filer = CreateMockSarifWorkItemFiler(
                out Mock<FilingClient> mockClient, 
                AzureDevOpsTestContext).Object;

            var workItemTrackingHttpClientMock = new Mock<IWorkItemTrackingHttpClient>();
            workItemTrackingHttpClientMock
                .Setup(x => x.CreateAttachmentAsync(It.IsAny<MemoryStream>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<object>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(attachmentReference) // Return our test attachment, defined above.
                .Callback<MemoryStream, string, string, string, object, CancellationToken>(
                    (stream, fileName, uploadType, areaPath, userState, cancellationToken) =>
                    {
                        // Verify that the ADO client receives the request to create an attachment
                        createAttachmenCalled = true;
                    });

            workItemTrackingHttpClientMock
                .Setup(x => x.CreateWorkItemAsync(It.IsAny<JsonPatchDocument>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool?>(), It.IsAny<bool?>(), It.IsAny<bool?>(), It.IsAny<object>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(workItem) // Return our test work item, defined above
                .Callback<JsonPatchDocument, string, string, bool?, bool?, bool?, object, CancellationToken>(
                    (document, project, type, validateOnly, bypassRules, suppressNotifications, userState, cancellationToken) =>
                    {
                        // Verify that the ADO client receives the request to file the bug
                        createWorkItemCalled = true;
                    });

            // FOUR. Create a mock VssConnection instance, which handles the auth connection
            //       and retrieval of the ADO filing client. We are required to put this
            //       mock behind an interface due to an inability 

            var vssConnectionMock = new Mock<IVssConnection>();
            vssConnectionMock
                .Setup(x => x.ConnectAsync(It.IsAny<Uri>(), It.IsAny<string>()))
                .Returns(Task.CompletedTask)
                .Callback<Uri, string>(
                    (uri, pat) =>
                    {
                        // The following data values flow to us via test constants which are
                        // captured in the default context object that initialized the filer.
                        pat.Should().Be(TestData.NotActuallyASecret);

                        // We configure the filer with a full ADO URI that contains the account and
                        // project, e.g., https://dev.azure.com/myaccount/myproject. By the time the
                        // ADO client receives it, however, the project has been stripped off 
                        // (because it is not required for making the connection).
                        AzureDevOpsFilingUri.OriginalString.StartsWith(uri.ToString()).Should().BeTrue();                        

                        // Verify that we received the connection request.
                        connectCalled = true;
                    });

            // Our GetClientAsync is overridden to provide our low-level ADO mock.
            vssConnectionMock
                .Setup(x => x.GetClientAsync())
                .ReturnsAsync(workItemTrackingHttpClientMock.Object);

            // FIVE. We are required to inject the low level ADO connection wrapper. We are 
            //       required to do so due to the complexity of creating and initializing
            //       required objects. MOQ cannot override constructors and (related)
            //       in general cannot instantiate a type without a parameterless ctor.
            //       Even when a concrete class can be instantiated, its various properties
            //       might not be easily stubbed (as for a non-virtual property accessor).
            //       For these cases, we need to insert an interface in our system, and 
            //       create wrappers around those concrete types, where the interface is
            //       directly mapped or otherwise adapted to the concrete type's methods.
            //       Moq can then simply manufacture a class that implements that interface
            //       in order to control behaviors.
            //
            //       Rather than introducing a type factory or more sophisticated pattern
            //       of injection, we use the very simple expedient of declaring an internal
            //       property to hold a mock instance. In production, if that property is null,
            //       the system instantiates a wrapper around the standard types for use.
            
            var adoFilingClient = (AzureDevOpsFilingClient)filer.FilingClient;
            adoFilingClient.vssConection = vssConnectionMock.Object;

            filer.FileWorkItems(sarifLog: TestData.SimpleLog);

            // Did we see all the execution we expected?
            connectCalled.Should().BeTrue();
            createAttachmenCalled.Should().BeTrue();
            createWorkItemCalled.Should().BeTrue();

            // This property is a naive mechanism to ensure that the code
            // executed comprehensively (i.e., that execution was not limited
            // due to unhandled exceptions). This is required because we have
            // not really implemented a proper async API with appropriate handling
            // for exceptions and other negative conditions. I wouldn't expect this
            // little helper to survive but it closes the loop for the current
            // rudimentary in-flight implementation.
            filer.FilingSucceeded.Should().BeTrue();

            filer.FiledWorkItems.Count.Should().Be(1);

            WorkItemModel filedWorkItem = filer.FiledWorkItems[0];

            // Finally, make sure that our test data flows back properly through the filer.

            filedWorkItem.Attachment.Should().NotBeNull();
            filedWorkItem.Attachment.Text.Should().Be(JsonConvert.SerializeObject(TestData.SimpleLog));

            filedWorkItem.HtmlUri.Should().Be(new Uri(htmlUri, UriKind.Absolute));
        }

        [Fact]
        public void WorkItemFiler_ValidatesSarifLogFileContentsArgument()
        {
            SarifWorkItemFiler filer = CreateWorkItemFiler();

            Action action = () => filer.FileWorkItems(sarifLogFileContents: null);

            action.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void WorkItemFiler_ValidatesSarifLogFileArgument()
        {
            SarifWorkItemFiler filer = CreateWorkItemFiler();

            Action action = () => filer.FileWorkItems(sarifLog: null);

            action.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void WorkItemFiler_ValidatesSarifLogFileLocationArgument()
        {
            SarifWorkItemFiler filer = CreateWorkItemFiler();

            Action action = () => filer.FileWorkItems(sarifLogFileLocation: null);

            action.Should().Throw<ArgumentNullException>();
        }

        private static SarifWorkItemFiler CreateWorkItemFiler(SarifWorkItemContext context = null)
            => CreateMockSarifWorkItemFiler(out _, context).Object;

        private static Mock<SarifWorkItemFiler> CreateMockSarifWorkItemFiler(out Mock<FilingClient> mockClient, SarifWorkItemContext context = null)
        {
            context = context ?? GitHubTestContext;

            mockClient = new Mock<FilingClient>();

            mockClient
                .Setup(x => x.Connect(It.IsAny<string>()))
                .CallBase();

            mockClient
                .Setup(x => x.FileWorkItems(It.IsAny<IEnumerable<WorkItemModel>>()))
                .CallBase();

            var mockFiler = new Mock<SarifWorkItemFiler>(context);

            mockFiler
                .Setup(x => x.FileWorkItems(It.IsAny<string>()))
                .CallBase();

            mockFiler
                .Setup(x => x.FileWorkItems(It.IsAny<SarifLog>()))
                .CallBase();

            mockFiler
                .Setup(x => x.FileWorkItems(It.IsAny<Uri>()))
                .CallBase();
            
            return mockFiler;
        }

        private readonly static Uri GitHubFilingUri = new Uri("https://github.com/nonexistentorg/nonexistentrepo");
        private readonly static Uri AzureDevOpsFilingUri = new Uri("https://dev.azure.com/nonexistentaccount/nonexistentproject");

        private static readonly SarifWorkItemContext GitHubTestContext = new SarifWorkItemContext()
        {
            HostUri = GitHubFilingUri,
            PersonalAccessToken = TestData.NotActuallyASecret
        };

        private static readonly SarifWorkItemContext AzureDevOpsTestContext = new SarifWorkItemContext()
        {
            HostUri = AzureDevOpsFilingUri,
            PersonalAccessToken = TestData.NotActuallyASecret
        };
    }
}