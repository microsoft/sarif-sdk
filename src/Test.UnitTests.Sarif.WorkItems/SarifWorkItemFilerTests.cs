// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.CodeAnalysis.Test.Utilities.Sarif;
using Microsoft.CodeAnalysis.WorkItems;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
using Microsoft.VisualStudio.Services.WebApi;
using Microsoft.VisualStudio.Services.WebApi.Patch.Json;
using Microsoft.WorkItems;
using Moq;
using Newtonsoft.Json;
using Octokit;
using Xunit;

namespace Microsoft.CodeAnalysis.Sarif.WorkItems
{
    public class SarifWorkItemFilerTests
    {
        [Fact]
        public void WorkItemFiler_PerRunSplitStrategyPartitionsProperly()
        {
            SarifWorkItemContext context = CreateAzureDevOpsTestContext();

            SarifLog sarifLog = TestData.CreateSimpleLog();

            // Our default splitting strategy is PerRun, that is, one
            // work item (and corresponding attachment) should be filed 
            // for each run in the log file.
            int numberOfRuns = sarifLog.Runs.Count;
            context.SetProperty(ExpectedWorkItemsCount, numberOfRuns);

            TestWorkItemFiler(sarifLog, context, true);
        }

        [Fact]
        public void WorkItemFiler_PerRunSplitStrategyPartitionsProperlyGithub()
        {
            SarifWorkItemContext context = GitHubTestContext;

            SarifLog sarifLog = TestData.CreateSimpleLog();

            // Our default splitting strategy is PerRun, that is, one
            // work item (and corresponding attachment) should be filed 
            // for each run in the log file.
            int numberOfRuns = sarifLog.Runs.Count;
            context.SetProperty(ExpectedWorkItemsCount, numberOfRuns);

            TestWorkItemFiler(sarifLog, context, false);
        }


        [Fact]
        public void WorkItemFiler_PerResultSplitStrategyPartitionsProperly()
        {
            SarifLog sarifLog = TestData.CreateSimpleLog();
            SarifWorkItemContext context = CreateAzureDevOpsTestContext();

            context.SplittingStrategy = SplittingStrategy.PerResult;

            int numberOfResults = sarifLog.Runs.Sum(run => run.Results.Count);
            context.SetProperty(ExpectedWorkItemsCount, numberOfResults);

            TestWorkItemFiler(sarifLog, context, true);
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

        private void TestWorkItemFiler(SarifLog sarifLog, SarifWorkItemContext context, bool adoClient)
        {
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
            string bugUriText = "https://example.com/" + Guid.NewGuid().ToString();
            string bugHtmlUriText = "https://example.com/" + Guid.NewGuid().ToString();

            Uri bugUri = new Uri(bugUriText, UriKind.RelativeOrAbsolute);
            Uri bugHtmlUri = new Uri(bugHtmlUriText, UriKind.RelativeOrAbsolute);

            workItem.Url = bugUriText;
            workItem.Links.AddLink("html", bugHtmlUriText);

            // TWO. Reset variables to capture whether we enter all expected client methods.
            ConnectCalled = false;
            CreateWorkItemCalled = CreateAttachmentCount = UpdateIssueCount = 0;

            // THREE. Create a default mock SARIF filer and client.
            SarifWorkItemFiler filer = CreateMockSarifWorkItemFiler(context).Object;

            // FOUR. Based on which client we are using (ADO or GitHub), create the correct context.
            //       This implies created both the connection mocks and the mocks for filing, updating, and attaching work items.
            //       We are required to put this mock behind an interface due to an inability to mock these types directly.

            FilingClient filingClient;
            if (adoClient == true)
            {
                filingClient = CreateAdoMocksAndFilingClient(attachmentReference, workItem, filer);
            }
            else
            {
                filingClient = CreateGitHubMocksAndFilingClient(bugUriText, bugHtmlUriText, filer);
            }

            string sarifLogText = JsonConvert.SerializeObject(sarifLog);
            SarifLog updatedSarifLog = filer.FileWorkItems(sarifLogText);

            // Did we see all the execution we expected?
            ConnectCalled.Should().BeTrue();

            int expectedWorkItemsCount = context.GetProperty(ExpectedWorkItemsCount);

            CreateWorkItemCalled.Should().Be(expectedWorkItemsCount);
            CreateAttachmentCount.Should().Be(adoClient ? expectedWorkItemsCount : 0);

            // This property is a naive mechanism to ensure that the code
            // executed comprehensively (i.e., that execution was not limited
            // due to unhandled exceptions). This is required because we have
            // not really implemented a proper async API with appropriate handling
            // for exceptions and other negative conditions. I wouldn't expect this
            // little helper to survive but it closes the loop for the current
            // rudimentary in-flight implementation.
            filer.FilingResult.Should().Be(FilingResult.Succeeded);

            filer.FiledWorkItems.Count.Should().Be(expectedWorkItemsCount);

            foreach (WorkItemModel filedWorkItem in filer.FiledWorkItems)
            {
                // Finally, make sure that our test data flows back properly through the filer.

                filedWorkItem.Attachment.Should().NotBeNull();
                JsonConvert.SerializeObject(filedWorkItem.Attachment.Text).Should().NotBeNull();

                filedWorkItem.Uri.Should().Be(bugUri);
                filedWorkItem.HtmlUri.Should().Be(bugHtmlUri);
            }

            // Validate that we updated the SARIF log with work itme URIs.
            // 
            updatedSarifLog.Should().NotBeEquivalentTo(sarifLog);
            
            foreach(Run run in updatedSarifLog.Runs)
            {
                foreach(Result result in run.Results)
                {
                    result.WorkItemUris.Should().NotBeNull();
                    result.WorkItemUris.Count.Should().Be(1);
                    result.WorkItemUris[0].Should().Be(bugHtmlUri);

                    result.TryGetProperty(SarifWorkItemFiler.PROGRAMMABLE_URIS_PROPERTY_NAME, out List<Uri> programmableUris)
                        .Should().BeTrue();

                    programmableUris.Should().NotBeNull();
                    programmableUris.Count.Should().Be(1);
                    programmableUris[0].Should().Be(bugUri);
                }
            }              
        }

        private static FilingClient CreateAdoMocksAndFilingClient(AttachmentReference attachmentReference, WorkItem workItem, SarifWorkItemFiler filer)
        {
            FilingClient filingClient;
            var workItemTrackingHttpClientMock = new Mock<IWorkItemTrackingHttpClient>();
            workItemTrackingHttpClientMock
                .Setup(x => x.CreateAttachmentAsync(It.IsAny<MemoryStream>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<object>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(attachmentReference) // Return our test attachment, defined above.
                .Callback<MemoryStream, string, string, string, object, CancellationToken>(
                    (stream, fileName, uploadType, areaPath, userState, cancellationToken) =>
                    {
                        // Verify that the ADO client receives the request to create an attachment
                        CreateAttachmentCount++;
                    });

            workItemTrackingHttpClientMock
                .Setup(x => x.CreateWorkItemAsync(It.IsAny<JsonPatchDocument>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool?>(), It.IsAny<bool?>(), It.IsAny<bool?>(), It.IsAny<object>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(workItem) // Return our test work item, defined above
                .Callback<JsonPatchDocument, string, string, bool?, bool?, bool?, object, CancellationToken>(
                    (document, project, type, validateOnly, bypassRules, suppressNotifications, userState, cancellationToken) =>
                    {
                        // Verify that the ADO client receives the request to file the bug
                        CreateWorkItemCalled++;
                    });

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
                        ConnectCalled = true;
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

            filingClient = (AzureDevOpsFilingClient)filer.FilingClient;
            ((AzureDevOpsFilingClient)filingClient)._vssConection = vssConnectionMock.Object;
            return filingClient;
        }

        private static FilingClient CreateGitHubMocksAndFilingClient(string bugUriText, string bugHtmlUriText, SarifWorkItemFiler filer)
        {
            FilingClient filingClient;
            Issue testGithubIssue = new Issue(bugUriText, bugHtmlUriText, bugUriText + "comments", bugUriText + "events",
                                              111111, ItemState.Open, "TestTitle", "TestBody", new User(), new User(), null, new User(), null, new Milestone(1), 0, new PullRequest(), null,
                                              new DateTimeOffset(DateTime.Now), null, 111111, null, false, null, null);

            var gitHubClientWrapperMock = new Mock<IGitHubClientWrapper>();
            gitHubClientWrapperMock
                .Setup(x => x.CreateWorkItemAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<NewIssue>()))
                .ReturnsAsync(testGithubIssue)
                .Callback<string, string, NewIssue>(
                    (org, repository, issue) =>
                    {
                        CreateWorkItemCalled++;
                    });

            gitHubClientWrapperMock
                .Setup(x => x.UpdateWorkItemAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<IssueUpdate>()))
                .ReturnsAsync(testGithubIssue)
                .Callback<string, string, int, IssueUpdate>(
                    (org, repository, issueNumber, issue) =>
                    {
                        UpdateIssueCount++;
                    });

            var gitHubConnectionMock = new Mock<IGitHubConnection>();
            gitHubConnectionMock
                .Setup(x => x.ConnectAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(gitHubClientWrapperMock.Object)
                .Callback<string, string>(
                    (org, pat) =>
                    {
                        ConnectCalled = true;
                        pat.Should().Be(TestData.NotActuallyASecret);
                        GitHubFilingUri.OriginalString.Contains(org.ToString()).Should().BeTrue();
                    });

            filingClient = (GitHubFilingClient)filer.FilingClient;
            ((GitHubFilingClient)filingClient)._gitHubConnection = gitHubConnectionMock.Object;
            return filingClient;
        }

        private static SarifWorkItemFiler CreateWorkItemFiler(SarifWorkItemContext context = null)
            => CreateMockSarifWorkItemFiler(context).Object;

        private static Mock<SarifWorkItemFiler> CreateMockSarifWorkItemFiler(SarifWorkItemContext context = null)
        {
            context = context ?? GitHubTestContext;

            var mockFiler = new Mock<SarifWorkItemFiler>(context.HostUri, context);

            mockFiler
                .Setup(x => x.FileWorkItems(It.IsAny<string>()))
                .CallBase();

            mockFiler
                .Setup(x => x.FileWorkItems(It.IsAny<SarifLog>()))
                .CallBase();

            mockFiler
                .Setup(x => x.FileWorkItems(It.IsAny<Uri>()))
                .CallBase();

            mockFiler
                .Setup(x => x.SplitLogFile(It.IsAny<SarifLog>()))
                .CallBase();

            return mockFiler;
        }

        private readonly static Uri GitHubFilingUri = new Uri("https://github.com/nonexistentorg/nonexistentrepo");
        private readonly static Uri AzureDevOpsFilingUri = new Uri("https://dev.azure.com/nonexistentaccount/nonexistentproject");

        public static int CreateWorkItemCalled = 0, CreateAttachmentCount = 0, UpdateIssueCount = 0;
        public static bool ConnectCalled = false;

        private static readonly SarifWorkItemContext GitHubTestContext = new SarifWorkItemContext()
        {
            HostUri = GitHubFilingUri,
            PersonalAccessToken = TestData.NotActuallyASecret
        };

        private static SarifWorkItemContext CreateAzureDevOpsTestContext()
        {
            return new SarifWorkItemContext()
            {
                HostUri = AzureDevOpsFilingUri,
                PersonalAccessToken = TestData.NotActuallyASecret
            };
        }

        internal static PerLanguageOption<int> ExpectedWorkItemsCount { get; } =
            new PerLanguageOption<int>(
                "Extensibility", nameof(ExpectedWorkItemsCount),
                defaultValue: () => { return 1; });
    }
}