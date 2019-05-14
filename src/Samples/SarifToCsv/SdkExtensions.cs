// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;

using Microsoft.CodeAnalysis.Sarif;

namespace SarifToCsv
{
    public static class SdkExtensions
    {
        public static IEnumerable<PhysicalLocation> PhysicalLocations(this Result result)
        {
            if (result.Locations != null)
            {
                foreach (Location location in result.Locations)
                {
                    if (location.PhysicalLocation != null)
                    {
                        yield return location.PhysicalLocation;
                    }
                }
            }
        }

        public static string FileUri(this ArtifactLocation artifactLocation, Run run)
        {
            if (artifactLocation == null)
            {
                return null;
            }
            else if (artifactLocation.Uri != null)
            {
                return artifactLocation.Uri.ToString();
            }
            else if (artifactLocation.Index >= 0 && artifactLocation.Index < run.Artifacts.Count)
            {
                return run.Artifacts[artifactLocation.Index].Location?.Uri?.ToString();
            }
            else
            {
                return null;
            }
        }
    }
}