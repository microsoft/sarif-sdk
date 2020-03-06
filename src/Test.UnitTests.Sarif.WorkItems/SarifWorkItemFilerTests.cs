// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.TeamFoundation.Build.WebApi;
using Microsoft.TeamFoundation.Work.WebApi;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
using Microsoft.VisualStudio.Services.Directories;
using Microsoft.VisualStudio.Services.WebApi.Patch.Json;
using Microsoft.WorkItems;
using Moq;
using Xunit;

namespace Microsoft.CodeAnalysis.Sarif.WorkItems
{
    public class SarifWorkItemFilerTests
    {
        private static readonly Uri s_testUri = new Uri("https://github.com/Microsoft/sarif-sdk");

        [Fact]
        public void WorkItemFiler_AzureDevOpsClientReceivesExpectedCalls()
        {
            var attachmentReference = new AttachmentReference()
            {
                Id = Guid.NewGuid(),
                Url = Guid.NewGuid().ToString()
            };

            var workItem = new WorkItem
            {
                Id = DateTime.UtcNow.Millisecond
            };

            bool connectCalled = false,
                 createWorkItemCalled = false,
                 createAttachmenCalled = false;                 

            // Create a default mock SARIF filer, configured by an 
            // AzureDevOps context (which creates a default ADO
            // filing client underneat).
            SarifWorkItemFiler filer = CreateMockSarifWorkItemFiler(
                out Mock<FilingClient> mockClient, 
                AzureDevOpsTestContext).Object;

            var workItemTrackingHttpClientMock = new Mock<IWorkItemTrackingHttpClient>();
            workItemTrackingHttpClientMock
                .Setup(x => x.CreateAttachmentAsync(It.IsAny<MemoryStream>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<object>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(attachmentReference)
                .Callback<MemoryStream, string, string, string, object, CancellationToken>(
                    (stream, fileName, uploadType, areaPath, userState, cancellationToken) =>
                    {
                        createAttachmenCalled = true;
                    });

            workItemTrackingHttpClientMock
                .Setup(x => x.CreateWorkItemAsync(It.IsAny<JsonPatchDocument>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool?>(), It.IsAny<bool?>(), It.IsAny<bool?>(), It.IsAny<object>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(workItem)
                .Callback<JsonPatchDocument, string, string, bool?, bool?, bool?, object, CancellationToken>(
                    (document, project, type, validateOnly, bypassRules, suppressNotifications, userState, cancellationToken) =>
                    {
                        createAttachmenCalled = true;
                    });


            var vssConnectionMock = new Mock<IVssConnection>();
            vssConnectionMock
                .Setup(x => x.ConnectAsync(It.IsAny<Uri>(), It.IsAny<string>()))
                .Returns(Task.CompletedTask)
                .Callback<Uri, string>(
                    (uri, pat) =>
                    {
                        uri.Should().NotBeNull();
                        pat.Should().Be(NotActuallyASecret);
                        connectCalled = true;
                    });

            vssConnectionMock
                .Setup(x => x.GetClientAsync())
                .ReturnsAsync(workItemTrackingHttpClientMock.Object);

            // Inject an object to receive lower-level ADO client interactions
            // This call will throw an exception if the client type isn't right.
            var adoFilingClient = (AzureDevOpsFilingClient)filer.FilingClient;
            adoFilingClient.vssConection = vssConnectionMock.Object;

            Action action = () => filer.FileWorkItems(sarifLog: SimpleLog);

            action();

            connectCalled.Should().BeTrue();
            createAttachmenCalled.Should().BeTrue();
            createWorkItemCalled.Should().BeTrue();
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
            => CreateMockSarifWorkItemFiler(out Mock<FilingClient> client, context).Object;

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
            
            // Moq magic: you can return whatever was passed to a method by providing
            // a lambda (rather than a fixed value) to Returns or ReturnsAsync.
            // https://stackoverflow.com/questions/996602/returning-value-that-was-passed-into-a-method
            mockFiler
                .Setup(x => x.FileWorkItems(
                    It.IsAny<Uri>(), 
                    It.IsAny<IList<WorkItemModel<SarifWorkItemContext>>>(),
                    It.IsAny<string>()))
                .ReturnsAsync((Uri uri, IList<WorkItemModel<SarifWorkItemContext>> resultGroups, string personalAccessToken) => resultGroups);

            return mockFiler;
        }

        private const string TestRuleId = nameof(TestRuleId);
        private const string TestMessageText = nameof(TestMessageText);
        private const string NotActuallyASecret = nameof(NotActuallyASecret);

        private readonly static string AttachmentUrl = Guid.NewGuid().ToString();

        private readonly static Uri GitHubFilingUri = new Uri("https://github.com/nonexistentorg/nonexistentrepo");
        private readonly static Uri AzureDevOpsFilingUri = new Uri("https://dev.azure.com/nonexistentaccount/nonexistentproject");

        private static SarifWorkItemContext GitHubTestContext = new SarifWorkItemContext()
        {
            HostUri = GitHubFilingUri,
            PersonalAccessToken = NotActuallyASecret
        };

        private static SarifWorkItemContext AzureDevOpsTestContext = new SarifWorkItemContext()
        {
            HostUri = AzureDevOpsFilingUri,
            PersonalAccessToken = NotActuallyASecret
        };


        private static SarifLog SimpleLog = new SarifLog
        {
                Runs = new Run[]
                {
                    new Run
                    {
                        Results = new []
                        {
                            new Result
                            {
                                Rule = new ReportingDescriptorReference
                                {
                                    Id = TestRuleId
                                },
                                Message = new Message
                                {
                                    Text = TestMessageText
                                }
                            }
                        }
                    }
                }
        };
    }
}