// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using Microsoft.CodeAnalysis.Driver.Configuration;

namespace Microsoft.CodeAnalysis.DataModelGenerator
{
    /// <summary>Configuration settings for DataModelGenerator. This class cannot be inherited.</summary>
    /// <seealso cref="T:Microsoft.MSEC.ConfigurationParser.ConfigurationBase"/>
    /// <seealso cref="T:System.IDisposable"/>
    internal sealed class DataModelGeneratorConfiguration : ConfigurationBase, IDisposable
    {
        [Field(Type = FieldTypes.Required | FieldTypes.Default, Name = "Input", Usage = "Grammar input file")]
        public string InputFilePath { get; private set; }

        [Field(Name = "Output", Usage = "Output C# path. If this is not specified, defaults to the input file with the extension changed to cs.")]
        public string OutputFilePath { get; private set; }

        [Field(Name = "OverwriteOutput", ShortName = "F", Usage = "Overwrite the output even when it already exists.")]
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public bool Overwrite { get; private set; }

        [Field(Name = "ToConsole", Usage = "If specified, writes the generated C# code to the console in addition to the supplied output file.")]
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public bool ToConsole { get; private set; }

        public FileStream InputStream { get; private set; }
        public FileStream OutputStream { get; private set; }

        public void Dispose()
        {
            if (this.InputStream != null)
            {
                this.InputStream.Dispose();
                this.InputStream = null;
            }

            if (this.OutputStream != null)
            {
                this.OutputStream.Dispose();
                this.OutputStream = null;
            }
        }

        public override bool Validate(Dictionary<string, string> errors)
        {
            bool answer = true;

            IOCatch(() =>
            {
                this.InputFilePath = Path.GetFullPath(this.InputFilePath);
                this.InputStream = File.OpenRead(this.InputFilePath);
            }, "Input", errors, ref answer);

            IOCatch(() =>
            {
                if (this.OutputFilePath == null)
                {
                    this.OutputFilePath = Path.ChangeExtension(this.InputFilePath, ".cs");
                }

                FileMode mode;
                if (this.Overwrite)
                {
                    mode = FileMode.Create;
                }
                else
                {
                    mode = FileMode.CreateNew;
                }

                this.OutputStream = new FileStream(this.OutputFilePath, mode, FileAccess.Write, FileShare.None);
            }, "Output", errors, ref answer);

            return answer & base.Validate(errors);
        }

        private static void IOCatch(Action act, string arg, Dictionary<string, string> errors, ref bool answer)
        {
            try
            {
                act();
            }
            catch (IOException ex)
            {
                errors.Add(arg, ex.Message);
                answer = false;
            }
            catch (ArgumentException ex)
            {
                errors.Add(arg, ex.Message);
                answer = false;
            }
            catch (UnauthorizedAccessException ex)
            {
                errors.Add(arg, ex.Message);
                answer = false;
            }
        }
    }
}
