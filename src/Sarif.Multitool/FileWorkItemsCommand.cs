// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Globalization;
using System.Linq;

namespace Microsoft.CodeAnalysis.Sarif.Multitool
{
    public class FileWorkItemsCommand : CommandBase
    {
        private static readonly string[] s_knownFilingTargetTypes = new[] { "github", "azuredevops" };
        private static readonly string[] s_knownFilteringStrategies = new[] { "new", "all" };
        private static readonly string[] s_knownGroupingStrategies = new[] { "perresult" };

        private FileWorkItemsOptions _options;

        public int Run(FileWorkItemsOptions options)
        {
            if (!ValidateOptions(options)) { return 1; }

            _options = options;

            return 0;
        }

        private static bool ValidateOptions(FileWorkItemsOptions options)
        {
            bool valid = true;

            options.FilingTargetType = options.FilingTargetType.ToLowerInvariant();
            if (!s_knownFilingTargetTypes.Contains(options.FilingTargetType))
            {
                string optionDescription = CommandUtilities.GetOptionDescription<FileWorkItemsOptions>(nameof(options.FilingTargetType));
                Console.Error.WriteLine(
                    string.Format(
                        CultureInfo.CurrentCulture,
                        MultitoolResources.ErrorUnknownFilingTargetType,
                        optionDescription,
                        $"'{string.Join("', '", s_knownFilingTargetTypes)}'"));
                valid = false;
            }

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
                        MultitoolResources.ErrorUriIsNotAbsolute,
                        optionDescription));
                valid = false;
            }

            options.FilteringStrategy = options.FilteringStrategy.ToLowerInvariant();
            if (!s_knownFilteringStrategies.Contains(options.FilteringStrategy))
            {
                string optionDescription = CommandUtilities.GetOptionDescription<FileWorkItemsOptions>(nameof(options.FilteringStrategy));
                Console.Error.WriteLine(
                    string.Format(
                        CultureInfo.CurrentCulture,
                        MultitoolResources.ErrorUnknownFilteringStrategy,
                        optionDescription,
                        $"'{string.Join("', '", s_knownFilteringStrategies)}'"));
                valid = false;
            }

            options.GroupingStrategy = options.GroupingStrategy.ToLowerInvariant();
            if (!s_knownGroupingStrategies.Contains(options.GroupingStrategy))
            {
                string optionDescription = CommandUtilities.GetOptionDescription<FileWorkItemsOptions>(nameof(options.GroupingStrategy));
                Console.Error.WriteLine(
                    string.Format(
                        CultureInfo.CurrentCulture,
                        MultitoolResources.ErrorUnknownGroupingStrategy,
                        optionDescription,
                        $"'{string.Join("', '", s_knownGroupingStrategies)}'"));
                valid = false;
            }

            if (valid) { Console.WriteLine("Command line argumengs are valid."); }

            return valid;
        }
    }
}
