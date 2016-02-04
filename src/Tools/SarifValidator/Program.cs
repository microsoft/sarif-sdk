using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CommandLine;

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
            ShowOptions(options);

            try
            {
                List<JSONError> errors =
                    Validator.ValidateFile(options.InstanceFilePath, options.SchemaFilePath)
                    .ToList();
                if (errors.Count == 0)
                {
                    ConsoleUtil.WriteSuccess(Resources.Success);
                    rc = 0;
                }
                else
                {
                    ConsoleUtil.WriteError(Resources.FileContainsErrors, errors.Count);
                    errors.ForEach(e => Console.WriteLine(e));
                }
            }
            catch (Exception ex)
            {
                ReportException(ex);
            }

            return rc;
        }

        private static void Banner()
        {
            Version version = typeof(Program).Assembly.GetName().Version;

            Console.WriteLine(string.Format(CultureInfo.CurrentCulture, Resources.Banner, version));
            Console.WriteLine(Resources.Copyright);
            Console.WriteLine();
        }

        private static void ShowOptions(Options options)
        {
            Console.WriteLine(
                string.Format(
                    CultureInfo.CurrentCulture,
                    Resources.Options,
                    options.InstanceFilePath,
                    options.SchemaFilePath));
            Console.WriteLine();
        }

        private static void ReportException(Exception ex)
        {
            ConsoleUtil.WriteError(
                string.Format(
                    CultureInfo.CurrentCulture,
                    Resources.Exception,
                    ex.Message));
        }
    }
}
