// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using CommandLine;
using Microsoft.CodeAnalysis.Sarif.Driver;

namespace Microsoft.CodeAnalysis.Sarif.Cli
{
    internal class Program
    {
        private static int Main(string[] args)
        {
            Banner();

            return Parser.Default.ParseArguments<
                AnalyzeOptions,
                ExportRulesMetadataOptions
                >(args)
              .MapResult(
                (AnalyzeOptions analyzeOptions) => new AnalyzeCommand().Run(analyzeOptions),
                (ExportRulesMetadataOptions exportRulesMetadataOptions) => new ExportRulesMetadataCommand().Run(exportRulesMetadataOptions),
                errs => 1);
        }

        private static void Banner()
        {
            Assembly entryAssembly = Assembly.GetEntryAssembly();
            IEnumerable<Attribute> attributes = entryAssembly.GetCustomAttributes();

            var titleAttribute = attributes.Single(a => a is AssemblyTitleAttribute) as AssemblyTitleAttribute;
            string programName = titleAttribute.Title;

            string version = entryAssembly.GetName().Version.ToString();

            var copyrightAttribute = attributes.Single(a => a is AssemblyCopyrightAttribute) as AssemblyCopyrightAttribute;
            string copyright = copyrightAttribute.Copyright;

            Console.WriteLine(string.Format(CultureInfo.CurrentCulture, Resources.Banner, programName, version));
            Console.WriteLine(copyright);
            Console.WriteLine();
        }
    }
}
