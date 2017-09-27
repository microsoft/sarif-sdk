// Copyright (c) Microsoft. All rights reserved. 
// Licensed under the MIT license. See LICENSE file in the project root for full license information. 

using Microsoft.CodeAnalysis.Sarif;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Sarif.Viewer.Sarif
{
    static class ResultExtensions
    {
        const string NOTARGETFILEPATH = "NOTARGETFILEPATH";

        public static string FormatForVisualStudio(this Result result, IRule rule)
        {
            var messageLines = new List<string>();
            foreach (var location in result.Locations)
            {
                PhysicalLocation physicalLocation = location.ResultFile ?? location.AnalysisTarget;
                Uri uri = physicalLocation.Uri;
                string path = uri.IsFile ? uri.LocalPath : uri.ToString();
                messageLines.Add(
                    string.Format(
                        CultureInfo.InvariantCulture, "{0}{1}: {2} {3}: {4}",
                        path,
                        physicalLocation.Region.FormatForVisualStudio(),
                        result.Level.FormatForVisualStudio(),
                        result.RuleId,
                        result.GetMessageText(rule)
                        ));
            }

            return string.Join(Environment.NewLine, messageLines);
        }

        public static string GetPrimaryTargetFile(this Result result)
        {
            if (result == null)
            {
                return null;
            }

            if (result.Locations == null || result.Locations.Count == 0)
            {
                return string.Empty;
            }

            Location primaryLocation = result.Locations[0];

            if (primaryLocation.ResultFile != null)
            {
                return primaryLocation.ResultFile.Uri.ToPath();
            }
            else if (primaryLocation.AnalysisTarget != null)
            {
                return primaryLocation.AnalysisTarget.Uri.ToPath();
            }
            else if (primaryLocation.FullyQualifiedLogicalName != null)
            {
                return primaryLocation.FullyQualifiedLogicalName;
            }

            return string.Empty;
        }

        public static Region GetPrimaryTargetRegion(this Result result)
        {
            if (result == null || result.Locations == null || result.Locations.Count == 0)
            {
                return null;
            }

            Location primaryLocation = result.Locations[0];

            if (primaryLocation.ResultFile != null)
            {
                return primaryLocation.ResultFile.Region;
            }
            else if (primaryLocation.AnalysisTarget != null)
            {
                return primaryLocation.AnalysisTarget.Region;
            }
            else
            {
                return null;
            }
        }
    }
}
