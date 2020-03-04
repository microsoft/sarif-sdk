// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis.Sarif.Writers
{
    /// <summary>A class containing utility functions for working with MIME types.</summary>
    public static class MimeType
    {
        /// <summary>Guesses filePath appropriate MIME type given the extension from a file name.</summary>
        /// <param name="path">File path from which MIME type shall be guessed.</param>
        /// <returns>A string corresponding to the likely MIME type of <paramref name="path"/>
        public static string DetermineFromFileExtension(string path)
        {
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }

            foreach (ImmutableArray<ImmutableArray<string>> extensionsTable in GetExtensionsTables())
            {
                foreach (ImmutableArray<string> tableEntry in extensionsTable)
                {
                    // Each entry in the table is of the form [ mimeType, ext1, ext2, ... extN ]
                    for (int idx = 1; idx < tableEntry.Length; ++idx)
                    {
                        if (HasExtension(path, tableEntry[idx]))
                        {
                            return tableEntry[0];
                        }
                    }
                }
            }

            if (System.IO.Directory.Exists(path))
            {
                return MimeType.Directory;
            }

            return MimeType.Binary;
        }

        /// <summary>Guesses an appropriate MIME type given the extension from a file name.</summary>
        /// <param name="fileName">File name from which MIME type shall be guessed.</param>
        /// <returns>A string corresponding to the likely MIME type of <paramref name="fileName"/> given
        /// its extension.</returns>
        public static string DetermineFromFileExtension(Uri fileUri)
        {
            if (fileUri == null)
            {
                throw new ArgumentNullException(nameof(fileUri));
            }

            string fileName = fileUri.ToString();

            if (fileUri.IsAbsoluteUri && fileUri.IsFile)
            {
                fileName = fileUri.LocalPath;
            }

            return DetermineFromFileExtension(fileName);
        }

        public static bool IsTextualMimeType(string mimeType)
        {
            s_textualMimeTypes = s_textualMimeTypes ?? InitializeMimeTypesSet(s_textualExtensionsTable);

            // In order for a mime type to be regarded as textual, we require an explicit 
            // reference to it in this set. All unrecognized mime types are regarded as 
            // binary, in order to provoke the most conservative SDK behaviors around 
            // retrieving code snippets, etc.
            return s_textualMimeTypes.Contains(mimeType);
        }

        public static bool IsBinaryMimeType(string mimeType)
        {
            s_textualMimeTypes = s_textualMimeTypes ?? InitializeMimeTypesSet(s_textualExtensionsTable);

            // In order for a mime type to be regarded as textual, we require an explicit 
            // reference to it in this set. All unrecognized mime types are regarded as 
            // binary, in order to provoke the most conservative SDK behaviors around 
            // retrieving code snippets, etc.
            return !s_textualMimeTypes.Contains(mimeType);
        }

        /// <summary>The MIME type to use when no better MIME type is known.</summary>
        public static readonly string Default = Binary;

        /// <summary>The MIME type for C and C++ files.</summary>
        public static readonly string Cpp = "text/x-cpp";
        /// /// <summary>The MIME type for Java source code files.</summary>
        public static readonly string Java = "text/x-java-source";
        /// <summary>The MIME type for binaries.</summary>
        public static readonly string Binary = "application/octet-stream";
        /// <summary>The MIME type for directories.</summary>
        public static readonly string Directory = "application/x-directory";
        /// <summary>The MIME type used for CSharp files.</summary>
        public static readonly string CSharp = "text/x-csharp";
        /// <summary>The MIME type for SARIF files.</summary>
        public static readonly string Sarif = "application/sarif-json";
        /// <summary>The MIME type for Java properties files (which are xml).</summary>
        public static readonly string JavaProperties = "text/x-java-properties";

        private static bool HasExtension(string fileName, string extension)
        {
            if (extension.Length + 1 > fileName.Length)
            {
                // Not long enough
                return false;
            }

            // Check for '.' without allocating "." + extension
            int shouldBeDotIndex = fileName.Length - extension.Length - 1;
            if (fileName[shouldBeDotIndex] != '.')
            {
                // Period not in the right place.
                return false;
            }

            return fileName.EndsWith(extension, StringComparison.OrdinalIgnoreCase);
        }

        private static IEnumerable<ImmutableArray<ImmutableArray<string>>> GetExtensionsTables()
        {
            yield return s_textualExtensionsTable;
            yield return s_binaryExtensionsTable;
        }

        private static ImmutableHashSet<string> InitializeMimeTypesSet(ImmutableArray<ImmutableArray<string>> extensionsTable)
        {
            ImmutableHashSet<string>.Builder builder = ImmutableHashSet.CreateBuilder<string>();
            foreach (ImmutableArray<string> tableEntry in extensionsTable)
            {
                builder.Add(tableEntry[0]);
            }
            return builder.ToImmutableHashSet<string>();
        }

        private static ImmutableHashSet<string> s_textualMimeTypes;

        private static readonly ImmutableArray<ImmutableArray<string>> s_textualExtensionsTable = ImmutableArray.Create(
            ImmutableArray.Create("text/x-bat", "bat", "cmd"),
            ImmutableArray.Create(MimeType.Cpp, "c", "cpp", "h", "hpp", "cxx", /*SDV rule file*/ "slic"),
            ImmutableArray.Create(MimeType.CSharp, "cs"),
            ImmutableArray.Create("text/coffeescript", "coffee"),
            ImmutableArray.Create("text/css", "css"),
            ImmutableArray.Create("text/x-fsharp", "fs"),
            ImmutableArray.Create("text/x-handlebars-template", "handlebars"),
            ImmutableArray.Create("text/html", "ascx", "aspx", "htm", "html"),
            ImmutableArray.Create("text/x-ini", "ini", "gitconfig", "yml"),
            ImmutableArray.Create("text/x-jade", "jade"),
            ImmutableArray.Create(MimeType.JavaProperties, "properties"),
            ImmutableArray.Create("text/javascript", "js"),
            ImmutableArray.Create("application/json", "json"),
            ImmutableArray.Create("text/less", "less"),
            ImmutableArray.Create("text/x-lua", "lua"),
            ImmutableArray.Create("text/x-web-markdown", "md", "markdown"),
            ImmutableArray.Create("application/x-php", "php"),
            ImmutableArray.Create("text/plain", "txt"),
            ImmutableArray.Create("text/x-powershell", "ps", "ps1"),
            ImmutableArray.Create("text/python", "py"),
            ImmutableArray.Create("text/x-cshtml", "cshtml"),
            ImmutableArray.Create("text/ruby", "ruby", "gemspec"),
            ImmutableArray.Create(MimeType.Sarif, "sarif"),
            ImmutableArray.Create("text/scss", "scss"),
            ImmutableArray.Create("text/x-sql", "sql", "tsql"),
            ImmutableArray.Create("text/typescript", "ts"),
            ImmutableArray.Create("text/x-vb", "vb"),
            ImmutableArray.Create("text/xml", "xml", "csproj", "xaml", "dtd", "xsd", "vcxproj", "vbproj", "wixproj", "jsproj", "proj", "targets", "props", "config")
            );

        private static readonly ImmutableArray<ImmutableArray<string>> s_binaryExtensionsTable = ImmutableArray.Create(
             ImmutableArray.Create(MimeType.Java, "java", "jav"),
             ImmutableArray.Create("application/java-archive", "jar"),
             ImmutableArray.Create("application/zip", "zip"),
             ImmutableArray.Create("application/vns.ms-appx", "appx"),
             ImmutableArray.Create("application/vnd.ms-word.document", "docx"),
             ImmutableArray.Create("application/vnd.ms-word.template", "dotx"),
             ImmutableArray.Create("application/vnd.ms-excel", "xlsx"),
             ImmutableArray.Create("application/vnd.ms-powerpoint", "pptx"),
             ImmutableArray.Create("application/vnd.ms-cab-compressed", "cab"),
             ImmutableArray.Create("application/vnd.ms-xpsdocument", "xps")
             );
    }
}
