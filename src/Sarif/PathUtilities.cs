// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.CodeAnalysis.Sarif
{
    /// <summary>
    /// Provides path utility methods that are safe to use with paths containing
    /// characters that would be illegal in file system paths but valid in URIs.
    /// </summary>
    public static class PathUtilities
    {
        /// <summary>
        /// Returns the extension of the specified path string, similar to Path.GetExtension,
        /// but without throwing on illegal path characters. This is useful when working with
        /// URIs that may contain characters like '&lt;', '&gt;', '|', '"', etc.
        /// </summary>
        /// <param name="path">The path string from which to obtain the extension.</param>
        /// <returns>
        /// The extension of the specified path (including the period "."), or null, or String.Empty.
        /// If path is null, this method returns null. If path does not have extension information,
        /// this method returns String.Empty.
        /// </returns>
        /// <remarks>
        /// This implementation matches the .NET 8.0 Path.GetExtension logic but removes 
        /// the validation for illegal path characters, making it safe to use with URIs.
        /// The behavior is identical to Path.GetExtension in .NET Core 3.1+ where invalid
        /// path characters no longer throw exceptions.
        /// </remarks>
        public static string GetExtension(string? path)
        {
            if (path == null)
            {
                return null;
            }

            int length = path.Length;
            for (int i = length - 1; i >= 0; i--)
            {
                char ch = path[i];
                if (ch == '.')
                {
                    if (i != length - 1)
                    {
                        return path.Substring(i, length - i);
                    }
                    
                    return string.Empty;
                }
                
                if (ch == System.IO.Path.DirectorySeparatorChar || 
                    ch == System.IO.Path.AltDirectorySeparatorChar || 
                    ch == System.IO.Path.VolumeSeparatorChar)
                {
                    break;
                }
            }
            
            return string.Empty;
        }
    }
}
