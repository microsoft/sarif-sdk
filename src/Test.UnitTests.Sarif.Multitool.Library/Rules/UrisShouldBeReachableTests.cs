// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using FluentAssertions;

using Microsoft.CodeAnalysis.Sarif.Multitool.Rules;

using Xunit;

namespace Microsoft.CodeAnalysis.Sarif.Multitool
{
    public class UrisShouldBeReachableTests
    {
        private static bool IsReserved(string uri)
            => UrisShouldBeReachable.IsReservedDocumentationHost(new Uri(uri, UriKind.Absolute));

        private static bool IsHttpScheme(string uri)
            => UrisShouldBeReachable.IsHttpScheme(new Uri(uri, UriKind.Absolute));

        [Fact]
        public void IsHttpScheme_AcceptsHttp()
        {
            IsHttpScheme("http://example-corp.com/help").Should().BeTrue();
        }

        [Fact]
        public void IsHttpScheme_AcceptsHttps()
        {
            IsHttpScheme("https://example-corp.com/help").Should().BeTrue();
        }

        [Fact]
        public void IsHttpScheme_RejectsFtp()
        {
            IsHttpScheme("ftp://files.contoso.com/pkg.zip").Should().BeFalse();
        }

        [Fact]
        public void IsHttpScheme_RejectsFile()
        {
            IsHttpScheme("file:///C:/temp/pkg.zip").Should().BeFalse();
        }

        [Fact]
        public void IsHttpScheme_RejectsMailto()
        {
            IsHttpScheme("mailto:dev@contoso.com").Should().BeFalse();
        }

        [Fact]
        public void SARIF2006_DoesNotRaiseSkimmerExceptionForNonHttpScheme()
        {
            // A well-formed absolute URI whose scheme is not HTTP(S) must not be passed to
            // HttpClient.GetAsync (which throws NotSupportedException), which would otherwise
            // surface as an ERR998.ExceptionInAnalyze skimmer fault and abort the rule.
            var log = new SarifLog
            {
                Runs = new[]
                {
                    new Run
                    {
                        Tool = new Tool
                        {
                            Driver = new ToolComponent
                            {
                                Name = "CodeScanner",
                                Version = "1.0",
                                DownloadUri = new Uri("ftp://files.contoso.com/pkg.zip")
                            }
                        },
                        Results = new List<Result>()
                    }
                }
            };

            SarifLog output = RunValidation(log);

            ToolExecutionNotifications(output)
                .Any(n => n.Descriptor?.Id == "ERR998.ExceptionInAnalyze")
                .Should().BeFalse();

            output.Runs[0].Results
                .Any(r => r.RuleId == "SARIF2006")
                .Should().BeFalse();
        }

        private static IEnumerable<Notification> ToolExecutionNotifications(SarifLog output)
            => output.Runs[0].Invocations?.SelectMany(
                   i => i.ToolExecutionNotifications ?? Enumerable.Empty<Notification>())
               ?? Enumerable.Empty<Notification>();

        private static SarifLog RunValidation(SarifLog inputLog)
        {
            string inputPath = Path.GetTempFileName() + ".sarif";
            string outputPath = Path.GetTempFileName() + ".sarif";

            try
            {
                inputLog.Save(inputPath);

                var options = new ValidateOptions
                {
                    TargetFileSpecifiers = new[] { inputPath },
                    OutputFilePath = outputPath,
                    OutputFileOptions = new[] { FilePersistenceOptions.ForceOverwrite },
                    Kind = new List<ResultKind> { ResultKind.Fail },
                    Level = new List<FailureLevel> { FailureLevel.Note, FailureLevel.Warning, FailureLevel.Error }
                };

                var context = new SarifValidationContext { FileSystem = FileSystem.Instance };
                new ValidateCommand().Run(options, ref context);

                return SarifLog.Load(outputPath);
            }
            finally
            {
                if (File.Exists(inputPath)) { File.Delete(inputPath); }
                if (File.Exists(outputPath)) { File.Delete(outputPath); }
            }
        }

        [Fact]
        public void IsReservedDocumentationHost_ExemptsExampleOrgAzureDevOpsRepository()
        {
            IsReserved("https://dev.azure.com/example-org/example-project/_git/sarif-sdk").Should().BeTrue();
        }

        [Fact]
        public void IsReservedDocumentationHost_ExemptsExampleAzureDevOpsRepository()
        {
            IsReserved("https://dev.azure.com/example/example-project/_git/sarif-sdk").Should().BeTrue();
        }

        [Fact]
        public void IsReservedDocumentationHost_DoesNotExemptRealAzureDevOpsRepository()
        {
            IsReserved("https://dev.azure.com/contoso/widgets/_git/widgets").Should().BeFalse();
        }

        [Fact]
        public void IsReservedDocumentationHost_DoesNotExemptOrgMerelyPrefixedWithExample()
        {
            // The carve-out is an exact org match, not a prefix: a real org named "example-corp"
            // must still be probed.
            IsReserved("https://dev.azure.com/example-corp/widgets/_git/widgets").Should().BeFalse();
        }

        [Fact]
        public void IsReservedDocumentationHost_DoesNotExemptExampleOrgOnGitHubHost()
        {
            // The Azure DevOps org carve-out is host-scoped to dev.azure.com; a github.com owner
            // named "example-org" is a real reachable repository.
            IsReserved("https://github.com/example-org/sarif-sdk").Should().BeFalse();
        }

        [Fact]
        public void IsReservedDocumentationHost_ExemptsRfc2606DocumentationHost()
        {
            IsReserved("https://www.example.com/help").Should().BeTrue();
        }

        [Fact]
        public void IsReservedDocumentationHost_DoesNotExemptRealHost()
        {
            IsReserved("https://github.com/microsoft/sarif-sdk").Should().BeFalse();
        }
    }
}
