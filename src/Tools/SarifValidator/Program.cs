// Copyright (c) Microsoft. All rights reserved. Licensed under the MIT        
// license. See LICENSE file in the project root for full license information. 

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using CommandLine;
using Microsoft.CodeAnalysis.Sarif.Driver;
using Microsoft.CodeAnalysis.Sarif.Validation;

namespace Microsoft.CodeAnalysis.Sarif.SarifValidator
{
    internal class Program
    {
        private static int Main(string[] args)
        {
            return Parser.Default.ParseArguments<Options>(args)
                .MapResult(Run, errs => 1);
        }

        private static int Run(Options options)
        {
            int rc = 1;

            Banner();

            if (string.IsNullOrWhiteSpace(options.OutputFilePath))
            {
                options.OutputFilePath = MakeDefaultOutputFilePath(options.InstanceFilePath);
            }

            try
            {
                List<JsonError> errors =
                    Validator.ValidateFile(options.InstanceFilePath, options.SchemaFilePath)
                    .ToList();

                IEnumerable<string> messages = Enumerable.Empty<string>();
                using (var logBuilder = new ResultLogBuilder(
                    options.InstanceFilePath,
                    options.SchemaFilePath,
                    options.OutputFilePath,
                    new FileSystem()))
                {
                    messages = logBuilder.BuildLog(errors);
                }

                if (errors.Count == 0)
                {
                    Console.WriteLine(Resources.Success);
                    rc = 0;
                }
                else
                {
                    Console.WriteLine(Resources.FileContainsErrors, errors.Count);
                    foreach (var message in messages)
                    {
                        Console.WriteLine(message);
                    }
                }

            }
            catch (Exception ex)
            {
                ReportException(ex);
            }

            return rc;
        }

        private static string MakeDefaultOutputFilePath(string instanceFilePath)
        {
            return Path.GetFileNameWithoutExtension(instanceFilePath) + ".sarif";
        }

        private static void Banner()
        {
            Version version = typeof(Program).Assembly.GetName().Version;

            Console.WriteLine(string.Format(CultureInfo.CurrentCulture, Resources.Banner, version));
            Console.WriteLine(Resources.Copyright);
            Console.WriteLine();
        }

        private static void ReportException(Exception ex)
        {
            Console.WriteLine(
                string.Format(
                    CultureInfo.CurrentCulture,
                    Resources.Exception,
                    ex.Message));
        }
    }
}
