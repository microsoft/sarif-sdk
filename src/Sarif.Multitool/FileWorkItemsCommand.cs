// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Globalization;
using Microsoft.CodeAnalysis.Sarif.WorkItemFiling;

namespace Microsoft.CodeAnalysis.Sarif.Multitool
{
    public class FileWorkItemsCommand : CommandBase
    {
        public int Run(FileWorkItemsOptions options)
        {
            if (!ValidateOptions(options)) { return 1; }

            // For unit tests: allow us to just validate the options and return.
            if (options.TestOptionValidation) { return 0; }

            FilingTarget filingTarget = FilingTargetFactory.CreateFilingTarget(options.ProjectUriString);
            FilteringStrategy filteringStrategy = FilteringStrategyFactory.CreateFilteringStrategy(options.FilteringStrategy);
            GroupingStrategy groupingStrategy = GroupingStrategyFactory.CreateGroupingStrategy(options.GroupingStrategy);

            var filer = new WorkItemFiler(filingTarget, filteringStrategy, groupingStrategy);

            try
            {
                filer.FileWorkItems(options.InputFilePath).Wait();
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
            }

            return 0;
        }

        private static bool ValidateOptions(FileWorkItemsOptions options)
        {
            bool valid = true;

            // The CommandLine package lets you declare an option of any type with a constructor
            // that accepts a string. We could have declared the property corresponding to the
            // "project-uri" option to be of type Uri, in which case CommandLine would have
            // populated it by invoking the Uri(string) ctor. This overload requires its argument
            // to be an absolute URI. This happens to be what we want; the problem is that if the
            // user supplies a relative URI, CommandLine produces this error:
            //
            //   Option 'p, project-uri' is defined with a bad format.
            //   Required option 'p, project-uri' is missing.
            //
            // That's not as helpful as a message like this:
            //
            //   The value of the 'p, project-uri' option must be an absolute URI.
            //
            // Therefore we declare the property corresponding to "project-uri" as a string.
            // We convert it to a URI with the ctor Uri(string, UriKind.RelativeOrAbsolute).
            // If it succeeds, we can assign the result to a Uri-valued property; if it fails,
            // we can produce a helpful error message.
            options.ProjectUri = new Uri(options.ProjectUriString, UriKind.RelativeOrAbsolute);

            if (!options.ProjectUri.IsAbsoluteUri)
            {
                string optionDescription = CommandUtilities.GetOptionDescription<FileWorkItemsOptions>(nameof(options.ProjectUriString));
                Console.Error.WriteLine(
                    string.Format(
                        CultureInfo.CurrentCulture,
                        MultitoolResources.WorkItemFiling_ErrorUriIsNotAbsolute,
                        options.ProjectUriString,
                        optionDescription));
                valid = false;
            }

            return valid;
        }
    }
}
