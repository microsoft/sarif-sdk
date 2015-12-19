// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Text.RegularExpressions;

namespace Microsoft.CodeAnalysis.Sarif.Driver
{
    /// <summary>
    /// The SemanticVersion class implements Semantic Versioning based on the 
    /// http://semver.org/ summary 2.0.0
    /// </summary>
    public class SemanticVersion : IComparable
    {
        /// <summary>
        /// Major version.
        /// </summary>
        public int Major { get; set; }

        /// <summary>
        /// Metadata which is not used for version comparison.
        /// This contains the value after the plus sign as described below
        /// Build metadata MAY be denoted by appending a plus sign and a series of dot separated 
        /// identifiers immediately following the patch or pre-release version. 
        /// Identifiers MUST comprise only ASCII alphanumerics and hyphen [0-9A-Za-z-]. 
        /// Identifiers MUST NOT be empty. 
        /// Build metadata SHOULD be ignored when determining version precedence. 
        /// Thus two versions that differ only in the build metadata, have the same precedence. 
        /// Examples: 1.0.0-alpha+001, 1.0.0+20130313144700, 1.0.0-beta+exp.sha.5114f85.
        /// </summary>
        public string Metadata { get; set; }

        /// <summary>
        /// Minor version.
        /// </summary>
        public int Minor { get; set; }

        /// <summary>
        /// Patch version.
        /// </summary>
        public int Patch { get; set; }

        /// <summary>
        /// This is used in cases where the version had 4 numbers. This is a violation of Semantic Version spec, but
        /// the retire data has cases of this so we will allow it, but make is read only.
        /// </summary>
        public int PatchMinor { get; private set; }

        /// <summary>
        /// This contains the value after the hyphen as described below.
        /// A pre-release version MAY be denoted by appending a hyphen and a series of dot separated
        /// identifiers immediately following the patch version. Identifiers MUST comprise only 
        /// ASCII alphanumerics and hyphen [0-9A-Za-z-]. Identifiers MUST NOT be empty. 
        /// Numeric identifiers MUST NOT include leading zeroes. 
        /// Pre-release versions have a lower precedence than the associated normal version. 
        /// A pre-release version indicates that the version is unstable and might not satisfy 
        /// the intended compatibility requirements as denoted by its associated normal version. 
        /// Examples: 1.0.0-alpha, 1.0.0-alpha.1, 1.0.0-0.3.7, 1.0.0-x.7.z.92.
        /// </summary>
        public string Prerelease { get; set; }

        /// <summary>
        /// Construct Semantic version.
        /// </summary>
        public SemanticVersion()
        {
            this.Major = -1;
            this.Minor = -1;
            this.Patch = -1;
            this.PatchMinor = -1;
            this.Prerelease = String.Empty;
            this.Metadata = String.Empty;
        }

        /// <summary>
        /// Construct version from a string.
        /// </summary>
        /// <param name="version">Version string</param>
        public SemanticVersion(string version)
        {
            SemanticVersion parsed = Parse(version);
            this.Major = parsed.Major;
            this.Minor = parsed.Minor;
            this.Patch = parsed.Patch;
            this.PatchMinor = -1;
            this.Prerelease = parsed.Prerelease;
            this.Metadata = parsed.Metadata;
        }

        /// <summary>
        /// Compares two prerelease values based on the SemanticVersion specification (identifier at a time).
        /// </summary>
        /// <param name="left">A Series of dot separated identifiers to be compared.</param>
        /// <param name="right">A Series of dot separated identifiers to be compared.</param>
        /// <returns>
        /// returns 1 if left is greater than  right, -1 if left is less than right, 0 if they are equal.
        /// </returns>
        private static int ComparePreRelease(string left, string right)
        {
            // compare the preRelease to see if there is a difference
            if (string.IsNullOrEmpty(left))
            {
                if (string.IsNullOrEmpty(right))
                {
                    // same version.
                    return 0;
                }

                // Pre-releases precede normal versions
                // 1.0.0-alpha < 1.0.0 < 1.0.1-alpha
                // current instance 1.0.0 follows obj instance 1.0.0-*
                return 1;
            }

            if (string.IsNullOrEmpty(right))
            {
                // current instance 1.0.0-* precedes obj instance 1.0.0
                return -1;
            }

            string[] preReleaseIdentifiers = left.Split(new[] { '.' });
            string[] otherIdentifiers = right.Split(new[] { '.' });

            return preReleaseIdentifiers.LexicographicalCompare(otherIdentifiers);
        }

        /// <summary>
        /// Compares Semantic Version according to the specification 2.0.0 at http://semver.org/
        /// </summary>
        /// <param name="obj">Object to compare this to.</param>
        /// <returns>
        /// returns 1 if this is greater than obj, -1 if this is less than obj, 0 if they are equal.
        /// </returns>
        public int CompareTo(object obj)
        {
            var semanticVersion = obj as SemanticVersion;
            if (semanticVersion == null)
            {
                return 1;
            }

            int[] left = new[] { this.Major, this.Minor, this.Patch, this.PatchMinor };
            int[] right = new[] { semanticVersion.Major, semanticVersion.Minor, semanticVersion.Patch, semanticVersion.PatchMinor };
            int result = left.LexicographicalCompare(right);
            if (result == 0)
            {
                return ComparePreRelease(this.Prerelease, semanticVersion.Prerelease);
            }

            return result;
        }

        /// <summary>
        /// Operator > for version comparison.
        /// </summary>
        /// <param name="left">Version on left side of expression.</param>
        /// <param name="right">Version on right side of expression.</param>
        /// <returns></returns>
        public static bool operator <(SemanticVersion left, SemanticVersion right)
        {
            if (left != null)
            {
                return left.CompareTo(right) < 0;
            }
            return right != null;  /* null < anything (except null) is false */
        }

        /// <summary>
        /// Operator > for version comparison.
        /// </summary>
        /// <param name="left">Version on left side of expression.</param>
        /// <param name="right">Version on right side of expression.</param>
        /// <returns></returns>
        public static bool operator >(SemanticVersion left, SemanticVersion right)
        {
            if (left != null)
            {
                return left.CompareTo(right) > 0;
            }
            return false; /* null > anything (including null) is false */
        }

        /// <summary>
        /// Operator == for version comparison.
        /// </summary>
        /// <param name="left">Version on left side of expression.</param>
        /// <param name="right">Version on right side of expression.</param>
        /// <returns></returns>
        public static bool operator ==(SemanticVersion left, SemanticVersion right)
        {
            if (Object.Equals(left, null) && Object.Equals(right, null)) return true;
            if (Object.Equals(left, null) || Object.Equals(right, null)) return false;
            return left.Equals(right);
        }

        /// <summary>
        /// Operator != for version comparison.
        /// </summary>
        /// <param name="left">Version on left side of expression.</param>
        /// <param name="right">Version on right side of expression.</param>
        /// <returns></returns>
        public static bool operator !=(SemanticVersion left, SemanticVersion right)
        {
            return !(left == right);
        }

        /// <summary>
        /// Override Equals for version comparison.
        /// </summary>
        /// <param name="obj">Version to compare.</param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            return this.CompareTo(obj as SemanticVersion) == 0;
        }

        /// <summary>
        /// Get the hash code for the object.
        /// </summary>
        /// <returns>The hash code of the Semantic Version</returns>
        public override int GetHashCode()
        {
            var hash = new MultiplyByPrimesHash();
            hash.Add(this.Major);
            hash.Add(this.Minor);
            hash.Add(this.Patch);
            hash.Add(this.PatchMinor);
            hash.Add(this.Prerelease);
            hash.Add(this.Metadata);
            return hash.GetHashCode();
        }

        /// <summary>
        /// Convert to a string.
        /// </summary>
        /// <returns>
        /// Returns a string representation of this object.
        /// </returns>
        public override string ToString()
        {
            string version = this.Major + "." +
                this.Minor + "." + this.Patch + ((this.PatchMinor > 0) ? "." + this.PatchMinor : String.Empty);
            string preRelease = string.IsNullOrEmpty(this.Prerelease) ? string.Empty : "-" + this.Prerelease;
            string metaData = string.IsNullOrEmpty(this.Metadata) ? string.Empty : "+" + this.Metadata;
            return version + preRelease + metaData;
        }

        /// <summary>
        /// Parse the version from a given string.
        /// </summary>
        /// <param name="version">Version in Semantic Version format http://semver.org/ </param>
        /// <returns>A SemanticVersion object.</returns>
        public static SemanticVersion Parse(string version)
        {
            SemanticVersion result;
            if (!TryParse(version, out result))
            {
                throw new FormatException("Invalid semantic version.");
            }

            return result;
        }

        /// <summary>
        /// Handle explicit conversion so the Json de-serializer will function.
        /// </summary>
        /// <param name="version"></param>
        /// <returns></returns>
        public static explicit operator SemanticVersion(string version)
        {
            SemanticVersion semanticVersion;
            if (!TryParse(version, out semanticVersion))
            {
                semanticVersion = MakeLegalVersion(version);
            }
            return semanticVersion;
        }

        /// <summary>
        /// Try to make a legal version to fix RetireData errors.
        /// </summary>
        /// <param name="version"></param>
        /// <returns></returns>
        internal static SemanticVersion MakeLegalVersion(string version)
        {
            // split off first NonNumeric or dot character
            Regex regex = new Regex(@"[^0-9\\.]");
            Match match = regex.Match(version);
            string versionPart1 = version;
            string versionPart2 = String.Empty;

            if (match.Success)
            {
                versionPart1 = version.Substring(0, match.Index);
                versionPart2 = "-" + version.Substring(match.Index);
            }

            // ensure version looks like X.Y.Z[.Q]
            string[] splits = versionPart1.Split(new char[] { '.' });

            if (splits.Length < 3)
            {
                for (int index = splits.Length; index < 3; index++)
                {
                    versionPart1 += ".0";
                }
            }

            return Parse(versionPart1 + versionPart2);
        }

        /// <summary>
        /// Parse the version from a given string.
        /// </summary>
        /// <param name="version">Input version string in Semantic Version format.</param>
        /// <param name="result">SemanticVersion object.</param>
        /// <returns>Returns true if successful, false otherwise.</returns>
        public static bool TryParse(string version, out SemanticVersion result)
        {
            result = null;

            if (!String.IsNullOrEmpty(version))
            {
                // 1.2.3[-Pre1.2.3][+meta]
                int indexOfPre = version.IndexOf('-');
                int indexOfMeta = version.IndexOf('+');

                string majorMinorPatch = (indexOfPre > 0)
                                             ? version.Substring(0, indexOfPre)
                                             : ((indexOfMeta > 0) ? version.Substring(0, indexOfMeta) : version);

                string[] versionSplits = majorMinorPatch.Split(new[] { '.' });

                if (versionSplits.Length >= 3)
                {
                    int major;
                    int minor;
                    int patch;
                    int patchMinor = -1;

                    if ((int.TryParse(versionSplits[0], out major) &&
                         int.TryParse(versionSplits[1], out minor) &&
                         int.TryParse(versionSplits[2], out patch)))
                    {
                        if (versionSplits.Length == 4)
                        {
                            if (!int.TryParse(versionSplits[3], out patchMinor))
                            {
                                return false;
                            }
                        }

                        var semanticVersion = new SemanticVersion();
                        semanticVersion.Major = major;
                        semanticVersion.Minor = minor;
                        semanticVersion.Patch = patch;
                        semanticVersion.PatchMinor = patchMinor;

                        semanticVersion.Prerelease = (indexOfPre > 0)
                                              ? ((indexOfMeta > 0)
                                                     ? version.Substring(indexOfPre + 1, indexOfMeta - indexOfPre - 1)
                                                     : version.Substring(indexOfPre + 1))
                                              : string.Empty;

                        semanticVersion.Metadata = (indexOfMeta > 0) ? version.Substring(indexOfMeta + 1) : string.Empty;

                        result = semanticVersion;
                    }
                }
            }
            return result != null;
        }
    }
}