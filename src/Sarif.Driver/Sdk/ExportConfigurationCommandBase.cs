// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Immutable;
using System.IO;
using System.Reflection;

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
                // Currently, we perform discovery against the built-in CodeFormatter rules
                // and analyzers only.
                ImmutableArray<IOptionsProvider> providers = DriverUtilities.GetExports<IOptionsProvider>(DefaultPlugInAssemblies);
                foreach (IOptionsProvider provider in providers)
                {
                    foreach (IOption option in provider.GetOptions())
                    {
                        allOptions.SetProperty(option, option.DefaultValue, cacheDescription: true);
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
