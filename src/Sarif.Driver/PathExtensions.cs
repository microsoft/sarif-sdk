// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.IO;
using System.Linq;
using System.Reflection;
// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

// /********************************************************
// *                                                       *
// *   Copyright (C) Microsoft. All rights reserved.       *
// *                                                       *
// ********************************************************/

namespace Microsoft.CodeAnalysis.Sarif.Driver
{
    /// <summary>Path extension functions.</summary>
    public static class PathExtensions
    {
        /// <summary>Gets a path relative to the SdlCommon library.</summary>
        /// <param name="relativePath">The relative path to obtain to SdlCommon.dll.</param>
        /// <returns>The path relative to the SdlCommon library.</returns>
        public static string GetPathRelativeToSdlCommon(string relativePath)
        {
            string dllPath = Assembly.GetExecutingAssembly().Location;
            string dllDirectory = Path.GetDirectoryName(dllPath);
            return Path.Combine(dllDirectory, relativePath);
        }

        /// <summary>
        /// Try to get the directory containing a file or the directory itself if path is a directory.
        /// </summary>
        /// <param name="fileOrDirectory">file or directory</param>
        /// <returns>directory if possible, otherwise null</returns>
        public static string TryGetDirectory(string fileOrDirectory)
        {
            if (File.Exists(fileOrDirectory))
            {
                return Path.GetDirectoryName(fileOrDirectory);
            }

            if (Directory.Exists(fileOrDirectory))
            {
                return fileOrDirectory;
            }

            try
            {
                string result = Path.GetDirectoryName(fileOrDirectory);
                if (!string.IsNullOrEmpty(result))
                {
                    return result;
                }
            }
            catch (ArgumentException) { }
            // GetDirectoryName path parameter contains invalid characters, is empty, or contains only white spaces.
            // We can't determine a directory so fall through.

            catch (PathTooLongException) { }
            // GetDirectoryName path parameter is longer than the system-defined maximum length.
            // We can't determine a directory so fall through.

            catch (IOException) { }
            // GetDirectoryName (or File.Exists, Directory.Exists) failed for some other reason.  There aren't any others documented in .Net 4.0 but 4.5 has 
            // IOException.
            // We can't determine a directory so fall through.

            // If there are any other unexpected exceptions, like NullReferenceException or AccessViolationException or StackOverflowException, we want to fail.

            return null;
        }

        /// <summary>Unescapes a potentially quoted path.</summary>
        /// <exception cref="IOException">Thrown when an invalid path is supplied.</exception>
        /// <param name="sourcePath">The path to unescape.</param>
        /// <returns>An unescaped version of <paramref name="sourcePath"/>.</returns>
        public static string PathUnescapePath(string sourcePath)
        {
            if (sourcePath == null)
            {
                return string.Empty;
            }

            char[] invalidPathChars = Path.GetInvalidPathChars();
            if (sourcePath.StartsWith("\"", StringComparison.Ordinal))
            {
                // Path is a quoted path
                // http://msdn.microsoft.com/en-us/library/windows/desktop/bb776391.aspx
                // CommandLineToArgvW has a special interpretation of backslash characters when they are followed by a quotation mark character ("), as follows:
                // * 2n backslashes followed by a quotation mark produce n backslashes followed by a quotation mark.
                // * (2n) + 1 backslashes followed by a quotation mark again produce n backslashes followed by a quotation mark.
                // * n backslashes not followed by a quotation mark simply produce n backslashes.
                int sourceLength = sourcePath.Length;
                char[] resultTemp = new char[sourceLength - 1]; // would be - 2 because of 2 quote characters, but the end quote might not be there.
                int resultIdx = 0;
                int backslashN = 0;
                for (int idx = 1; idx != sourceLength; ++idx)
                {
                    char c = sourcePath[idx];

                    if (c != '"' && invalidPathChars.Contains(c))
                    {
                        throw new IOException("Invalid path detected: " + sourcePath + " had invalid characters.");
                    }

                    if (c == '\\')
                    {
                        backslashN++;
                        continue;
                    }
                    else if (backslashN != 0 && c == '"')
                    {
                        AddSlashes(resultTemp, ref resultIdx, backslashN / 2);
                        if (backslashN % 2 == 0)
                        {
                            backslashN = 0;
                            break;
                        }
                        else
                        {
                            backslashN = 0;
                        }
                    }
                    else if (backslashN != 0 && c != '"')
                    {
                        AddSlashes(resultTemp, ref resultIdx, backslashN);
                        backslashN = 0;
                    }
                    else if (c == '"')
                    {
                        break;
                    }

                    resultTemp[resultIdx++] = c;
                }

                AddSlashes(resultTemp, ref resultIdx, backslashN);
                return new string(resultTemp, 0, resultIdx);
            }
            else
            {
                // Path is an unquoted path. Make sure it contains all valid characters.
                foreach (char c in sourcePath)
                {
                    if (invalidPathChars.Contains(c))
                    {
                        throw new IOException("Invalid path detected: " + sourcePath + " had invalid characters.");
                    }
                }

                return sourcePath;
            }
        }

        /// <summary>Adds slashes to the temporary array.</summary>
        /// <param name="resultTemp">The result temporary array.</param>
        /// <param name="resultIdx">[in,out] Zero-based index of <paramref name="resultTemp"/> where slashes are being added. On return, set to the next insertion index in <paramref name="resultTemp"/>.</param>
        /// <param name="slashesToAdd">The number of slashes to add.</param>
        private static void AddSlashes(char[] resultTemp, ref int resultIdx, int slashesToAdd)
        {
            for (int slashCount = slashesToAdd; slashCount != 0; --slashCount)
            {
                resultTemp[resultIdx++] = '\\';
            }
        }
    }
}