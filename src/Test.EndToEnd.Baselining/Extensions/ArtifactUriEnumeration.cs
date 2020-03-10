// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;

using Microsoft.CodeAnalysis.Sarif;

namespace SarifBaseline.Extensions
{
    public static class ArtifactUriEnumeration
    {
        public static IEnumerable<Uri> AllResultArtifactUris(this SarifLog log)
        {
            if (log != null)
            {
                foreach (Run run in log.EnumerateRuns())
                {
                    foreach (Uri uri in run.AllResultArtifactUris())
                    {
                        yield return uri;
                    }
                }
            }
        }

        public static IEnumerable<Uri> AllResultArtifactUris(this Run run)
        {
            if (run?.Artifacts != null)
            {
                foreach (Artifact artifact in run.Artifacts)
                {
                    Uri uri = artifact?.Location?.Uri;
                    if (uri != null) { yield return uri; }
                }
            }

            foreach (Result result in run.EnumerateResults())
            {
                foreach (Uri uri in AllArtifactUris(result))
                {
                    yield return uri;
                }
            }
        }

        public static IEnumerable<Uri> AllArtifactUris(this Result result)
        {
            if (result?.Locations != null)
            {
                foreach (Location location in result.Locations)
                {
                    Uri uri = location?.PhysicalLocation?.ArtifactLocation?.FileUri(result.Run);
                    if (uri != null) { yield return uri; }
                }
            }

            if (result?.Attachments != null)
            {
                foreach (Attachment attachment in result.Attachments)
                {
                    Uri uri = attachment.ArtifactLocation?.FileUri(result.Run);
                    if (uri != null) { yield return uri; }
                }
            }
        }

        public static IEnumerable<PhysicalLocation> EnumeratePhysicalLocations(this Result result)
        {
            if (result.Locations != null)
            {
                foreach (Location location in result.Locations)
                {
                    PhysicalLocation pLoc = location.PhysicalLocation;
                    if (pLoc != null) { yield return pLoc; }
                }
            }
        }

        public static Uri FileUri(this PhysicalLocation physicalLocation, Run run)
        {
            return FileUri(physicalLocation?.ArtifactLocation, run);
        }

        public static Uri FileUri(this ArtifactLocation artifactLocation, Run run)
        {
            if (artifactLocation == null)
            {
                return null;
            }
            else if (artifactLocation.Uri != null)
            {
                return artifactLocation.Uri;
            }
            else if (artifactLocation.Index >= 0 && artifactLocation.Index < run.Artifacts.Count)
            {
                return run.Artifacts[artifactLocation.Index].Location?.Uri;
            }
            else
            {
                return null;
            }
        }
    }
}
