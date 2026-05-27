// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.Json.Pointer;

namespace Microsoft.CodeAnalysis.Sarif.Multitool.Rules
{
    public class ProvideCheckoutPath : SarifValidationSkimmerBase
    {
        public ProvideCheckoutPath()
        {
            this.DefaultConfiguration.Level = FailureLevel.Error;
        }

        /// <summary>
        /// GH1006
        /// </summary>
        public override string Id => RuleId.ProvideCheckoutPath;

        public override HashSet<RuleKind> RuleKinds => new HashSet<RuleKind>(new[] { RuleKind.Gh });

        // GitHub Advanced Security code scanning will reject a SARIF file that expresses
        // result locations as absolute 'file' scheme URIs unless GitHub can determine the URI
        // of the repository root (which GitHub refers to as the "checkout path"). There are
        // three ways to address this issue.
        //
        // 1. Recommended: Express all result locations as relative URI references with respect to
        // the checkout path.
        //
        // 2. Place the checkout path in 'invocations[].workingDirectory'. The SARIF specification
        // defines that property to be the working directory of the process that executed the
        // analysis tool, so if the tool was not invoked from the repository root directory, it
        // isn't strictly legal to place the checkout path there.
        //
        // 3. Place the checkout path in a configuration file at the root of the repository.This
        // requires the analysis tool always to be invoked from that same directory.
        public override MultiformatMessageString FullDescription => new MultiformatMessageString { Text = RuleResources.GH1006_ProvideCheckoutPath_FullDescription_Text };

        protected override ICollection<string> MessageResourceNames => new List<string> {
            nameof(RuleResources.GH1006_ProvideCheckoutPath_Error_Default_Text)
        };

        public override bool EnabledByDefault => false;

        protected override void Analyze(Result result, string resultPointer)
        {
            if (result.Locations?.Any() == true)
            {
                string locationsPointer = resultPointer.AtProperty(SarifPropertyName.Locations);
                for (int i = 0; i < result.Locations.Count; i++)
                {
                    ValidateLocation(result.Locations[i], locationsPointer.AtIndex(i));
                }
            }

            if (result.RelatedLocations?.Any() == true)
            {
                string relatedLocationsPointer = resultPointer.AtProperty(SarifPropertyName.RelatedLocations);
                for (int i = 0; i < result.RelatedLocations.Count; i++)
                {
                    ValidateLocation(result.RelatedLocations[i], relatedLocationsPointer.AtIndex(i));
                }
            }
        }

        private string EnsureTrailingSlash(string uri)
            => uri.EndsWith("/") ? uri : uri + "/";

        private void ValidateLocation(Location location, string locationPointer)
        {
            Uri uri = location?.PhysicalLocation?.ArtifactLocation?.Uri;
            if (uri?.IsAbsoluteUri == true && uri.Scheme == "file")
            {
                if (!IsKnownCheckoutPath(uri.AbsoluteUri))
                {
                    // {0}: This result location is expressed as an absolute 'file' URI.
                    // GitHub Advanced Security code scanning will reject this file because
                    // it cannot determine the location of the repository root (which it
                    // refers to as the "checkout path"). Either express result locations
                    // as relative URI references with respect to the checkout path, place
                    // the checkout path in 'invocations[].workingDirectory', or place the
                    // checkout path in a configuration file at the root of the repository.
                    LogResult(
                        locationPointer
                            .AtProperty(SarifPropertyName.PhysicalLocation)
                            .AtProperty(SarifPropertyName.ArtifactLocation)
                            .AtProperty(SarifPropertyName.Uri),
                        nameof(RuleResources.GH1006_ProvideCheckoutPath_Error_Default_Text));
                }
            }
        }

        private bool IsKnownCheckoutPath(string absoluteUri)
        {
            Run run = Context.CurrentRun;
            if (run.Invocations?.Any() != true)
            {
                return false;
            }

            return run.Invocations.Any(invocation =>
                invocation.WorkingDirectory?.Uri?.IsAbsoluteUri == true &&
                absoluteUri.StartsWith(EnsureTrailingSlash(invocation.WorkingDirectory.Uri.AbsoluteUri)));
        }
    }
}
