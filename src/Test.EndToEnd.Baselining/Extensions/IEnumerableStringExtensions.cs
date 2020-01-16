// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SarifBaseline.Extensions
{
    public static class IEnumerableStringExtensions
    {
        /// <summary>
        ///  Filter a set to elements which contain 'requiredPart'. If 'requiredPart' is null or empty,
        ///  this function returns the full set as-is.
        /// </summary>
        /// <param name="set">IEnumerable of string to filter</param>
        /// <param name="requiredPart">Value which must be within items to return, or null/empty for no filtering</param>
        /// <returns>Strings in set which contain requiredPart, or full set if no requiredPart provided</returns>
        public static IEnumerable<string> WhereContainsPart(this IEnumerable<string> set, string requiredPart)
        {
            if (set == null) { throw new ArgumentNullException("set"); }

            if (string.IsNullOrEmpty(requiredPart))
            {
                return set;
            }
            else
            {
                return set.Where(path => (path?.IndexOf(requiredPart, StringComparison.OrdinalIgnoreCase) ?? -1) >= 0);
            }
        }

        /// <summary>
        ///  Filter an IEnumerable of string by the file extensions found on the files.
        /// </summary>
        /// <param name="set">Set of paths to filter</param>
        /// <param name="fileExtension">File extension to match</param>
        /// <returns>Items in the set which have the desired file extension</returns>
        public static IEnumerable<string> WhereExtensionIs(this IEnumerable<string> set, string fileExtension)
        {
            fileExtension = "." + fileExtension.TrimStart('.');

            return set.Where(path => Path.GetExtension(path).Equals(fileExtension, StringComparison.OrdinalIgnoreCase));
        }
    }
}
