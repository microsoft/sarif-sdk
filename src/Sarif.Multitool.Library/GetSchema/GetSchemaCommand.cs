// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;

using Microsoft.CodeAnalysis.Sarif.Driver;

namespace Microsoft.CodeAnalysis.Sarif.Multitool
{
    /// <summary>
    /// Implements <c>multitool get-schema</c>: emits the embedded JSON Schema that validates the
    /// input to a named emit verb.
    /// </summary>
    /// <remarks>
    /// The served bytes are the assembly's embedded resources, byte-identical to the schema files
    /// under <c>GetSchema/</c>.
    /// </remarks>
    public class GetSchemaCommand : CommandBase
    {
        internal const string ResourcePrefix = "Microsoft.CodeAnalysis.Sarif.Multitool.GetSchema.";

        /// <summary>
        /// Maps each emit verb to the embedded schema file that validates its input. A null value
        /// marks a verb whose schema is reserved but not yet available.
        /// </summary>
        internal static readonly IReadOnlyDictionary<string, string> SchemaByVerb =
            new SortedDictionary<string, string>(StringComparer.Ordinal)
            {
                ["emit-run"] = "ai-run.schema.json",
                ["emit-finalize"] = null,
                ["add-result"] = "ai-result.schema.json",
                ["add-invocation"] = "ai-invocation.schema.json",
                ["add-notification-reporting-descriptor"] = "ai-notification-reporting-descriptor.schema.json",
                ["add-rule-reporting-descriptor"] = "ai-rule-reporting-descriptor.schema.json",
            };

        public int Run(GetSchemaOptions options, IFileSystem fileSystem = null)
        {
            fileSystem ??= Sarif.FileSystem.Instance;

            try
            {
                if (options.List)
                {
                    Console.Out.WriteLine(BuildVerbList());
                    return SUCCESS;
                }

                if (string.IsNullOrWhiteSpace(options.Verb))
                {
                    Console.Error.WriteLine("error: specify a verb whose schema to emit, or pass --list.");
                    Console.Error.WriteLine(BuildVerbList());
                    return FAILURE;
                }

                string verb = options.Verb.Trim();
                if (!SchemaByVerb.TryGetValue(verb, out string schemaFile))
                {
                    Console.Error.WriteLine(
                        string.Format(
                            CultureInfo.CurrentCulture,
                            "error: '{0}' is not a verb with a schema.",
                            verb));
                    Console.Error.WriteLine(BuildVerbList());
                    return FAILURE;
                }

                if (schemaFile == null)
                {
                    Console.Error.WriteLine(
                        string.Format(
                            CultureInfo.CurrentCulture,
                            "error: the schema for '{0}' is not yet available (tracking: https://github.com/microsoft/sarif-sdk/issues/2970).",
                            verb));
                    return FAILURE;
                }

                byte[] schemaBytes = ReadEmbeddedSchema(schemaFile);
                if (schemaBytes == null)
                {
                    Console.Error.WriteLine(
                        string.Format(
                            CultureInfo.CurrentCulture,
                            "error: embedded schema resource '{0}{1}' was not found.",
                            ResourcePrefix,
                            schemaFile));
                    return FAILURE;
                }

                if (!string.IsNullOrEmpty(options.OutputFilePath))
                {
                    if (fileSystem.FileExists(options.OutputFilePath) && !options.ForceOverwrite)
                    {
                        Console.Error.WriteLine(
                            string.Format(
                                CultureInfo.CurrentCulture,
                                "error: '{0}' already exists. Pass --force-overwrite to replace it.",
                                options.OutputFilePath));
                        return FAILURE;
                    }

                    fileSystem.FileWriteAllBytes(options.OutputFilePath, schemaBytes);
                    Console.Out.WriteLine(
                        string.Format(
                            CultureInfo.CurrentCulture,
                            "Wrote schema for '{0}' to '{1}'.",
                            verb,
                            options.OutputFilePath));
                    return SUCCESS;
                }

                using (Stream stdout = Console.OpenStandardOutput())
                {
                    stdout.Write(schemaBytes, 0, schemaBytes.Length);
                }

                return SUCCESS;
            }
            catch (Exception ex) when (!Debugger.IsAttached)
            {
                Console.Error.WriteLine(ex);
                return FAILURE;
            }
        }

        internal static byte[] ReadEmbeddedSchema(string schemaFile)
        {
            Assembly assembly = typeof(GetSchemaCommand).Assembly;
            using (Stream stream = assembly.GetManifestResourceStream(ResourcePrefix + schemaFile))
            {
                if (stream == null) { return null; }

                using (var memory = new MemoryStream())
                {
                    stream.CopyTo(memory);
                    return memory.ToArray();
                }
            }
        }

        private static string BuildVerbList()
        {
            IEnumerable<string> servable = SchemaByVerb
                .Where(kvp => kvp.Value != null)
                .Select(kvp => kvp.Key);

            return "Verbs with a schema:" + Environment.NewLine
                + string.Concat(servable.Select(v => "  " + v + Environment.NewLine)).TrimEnd();
        }
    }
}
