// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
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

            if (System.IO.Directory.Exists(path))
            {
                return MimeType.Directory;
            }

            foreach (ImmutableArray<string> tableEntry in s_extensionTable)
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
        public static readonly string Sarif = "text/x-sarif";

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


        private static readonly ImmutableArray<ImmutableArray<string>> s_extensionTable = ImmutableArray.Create(
            ImmutableArray.Create("text/x-bat", "bat", "cmd"),
            ImmutableArray.Create(MimeType.Cpp, "c", "cpp", "h", "hpp", "cxx", /*SDV rule file*/ "slic"),
            ImmutableArray.Create(MimeType.CSharp, "cs"),
            ImmutableArray.Create("text/coffeescript", "coffee"),
            ImmutableArray.Create("text/css", "css"),
            ImmutableArray.Create("text/x-fsharp", "fs"),
            ImmutableArray.Create("text/x-handlebars-template", "handlebars"),
            ImmutableArray.Create("text/html", "htm", "html"),
            ImmutableArray.Create("text/x-ini", "ini", "gitconfig", "yml"),
            ImmutableArray.Create("text/x-jade", "jade"),
            ImmutableArray.Create(MimeType.Java, "java", "jav"),
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
            ImmutableArray.Create("text/typescript", "ts"),
            ImmutableArray.Create("text/x-vb", "vb"),
            ImmutableArray.Create("text/xml", "xml", "ascx", "aspx", "csproj", "xaml", "dtd", "xsd", "vcxproj", "vbproj", "wixproj", "jsproj", "proj", "targets", "props", "config"),
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
