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

        public static string FileUri(this ArtifactLocation fileLocation, Run run)
        {
            if (fileLocation == null)
            {
                return null;
            }
            else if (fileLocation.Uri != null)
            {
                return fileLocation.Uri.ToString();
            }
            else if (fileLocation.FileIndex >= 0 && fileLocation.FileIndex < run.Files.Count)
            {
                return run.Files[fileLocation.FileIndex].ArtifactLocation?.Uri?.ToString();
            }
            else
            {
                return null;
            }
        }
    }
}