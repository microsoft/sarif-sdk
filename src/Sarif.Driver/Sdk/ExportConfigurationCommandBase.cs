// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;

namespace Microsoft.CodeAnalysis.Sarif.Driver
{
    public abstract class ExportConfigurationCommandBase : PlugInDriverCommand<ExportConfigurationOptions>
    {
        public override int Run(ExportConfigurationOptions exportOptions)
        {
            int result = FAILURE;

            try
            {
                PropertiesDictionary allOptions = new PropertiesDictionary();

                // The export command could be updated in the future to accept an arbitrary set
                // of analyzers for which to build an options XML file suitable for configuring them.
                ImmutableArray<IOptionsProvider> providers = CompositionUtilities.GetExports<IOptionsProvider>(DefaultPlugInAssemblies);
                foreach (IOptionsProvider provider in providers)
                {
                    IOption sampleOption = null;

                    // Every analysis options provider has access to the following default configuration knobs
                    foreach (IOption option in provider.GetOptions())
                    {
                        sampleOption = sampleOption ?? option;
                        allOptions.SetProperty(option, option.DefaultValue, cacheDescription: true);
                    }
                }

                IEnumerable<ReportingDescriptor> rules;
                rules = CompositionUtilities.GetExports<ReportingDescriptor>(DefaultPlugInAssemblies);

                // This code injects properties that are provided for every rule instance.
                foreach (ReportingDescriptor rule in rules)
                {
                    object objectResult;
                    PropertiesDictionary properties;

                    string ruleOptionsKey = rule.Id + "." + rule.Name + ".Options";

                    if (!allOptions.TryGetValue(ruleOptionsKey, out objectResult))
                    {
                        objectResult = allOptions[ruleOptionsKey] = new PropertiesDictionary();
                    }
                    properties = (PropertiesDictionary)objectResult;

                    foreach (IOption option in DefaultDriverOptions.Instance.GetOptions())
                    {
                        properties.SetProperty(option, option.DefaultValue, cacheDescription: true, persistToSettingsContainer: false);
                    }
                }

                string extension = Path.GetExtension(exportOptions.OutputFilePath);

                if (extension.Equals(".xml", StringComparison.OrdinalIgnoreCase))
                {
                    allOptions.SaveToXml(exportOptions.OutputFilePath);
                }
                else if (extension.Equals(".json", StringComparison.OrdinalIgnoreCase))
                {
                    allOptions.SaveToJson(exportOptions.OutputFilePath);
                }
                else if (exportOptions.FileFormat == FileFormat.Xml)
                {
                    allOptions.SaveToXml(exportOptions.OutputFilePath);
                }
                else
                {
                    allOptions.SaveToJson(exportOptions.OutputFilePath);
                }

                Console.WriteLine("Configuration file saved to: " + Path.GetFullPath(exportOptions.OutputFilePath));

                result = SUCCESS;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.ToString());
            }

            return result;
        }
    }
}
