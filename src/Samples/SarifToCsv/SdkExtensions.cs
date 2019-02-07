using Microsoft.CodeAnalysis.Sarif;
using System.Collections.Generic;

namespace SarifToCsv
{
    public static class SdkExtensions
    {
        public static IEnumerable<PhysicalLocation> PhysicalLocations(this Result result)
        {
            foreach (Location location in result.Locations)
            {
                if (location.PhysicalLocation != null)
                {
                    yield return location.PhysicalLocation;
                }
            }
        }

        public static string FileUri(this FileLocation fileLocation, Run run)
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
                return run.Files[fileLocation.FileIndex].FileLocation?.Uri?.ToString();
            }
            else
            {
                return null;
            }
        }
    }
}