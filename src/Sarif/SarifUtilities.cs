// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Resources;
using System.Text;
using System.Text.RegularExpressions;

namespace Microsoft.CodeAnalysis.Sarif
{
    public static class SarifUtilities
    {
        private static readonly Regex s_semVer200 =
            new Regex(@"^(?<major>\d+)\.(?<minor>\d+)\.(?<patch>\d+)(-(?<prerelease>[A-Za-z0-9\-\.]+))?(\+(?<build>[A-Za-z0-9\-\.]+))?$", RegexOptions.Compiled);

        public static bool IsSemanticVersioningCompatible(this string versionText)
        {
            return s_semVer200.IsMatch(versionText);
        }

        public const string V1_0_0 = "1.0.0";
        public const string SarifSchemaUriBase = "https://schemastore.azurewebsites.net/schemas/json/sarif-";

        public static readonly string SarifSchemaUri = ConvertToSchemaUri(SarifVersion.Current).OriginalString;

        /// <summary>
        /// Returns an ISO 8601 compatible universal date time format string with
        /// seconds precision, used to produce times such as "2016-03-02T01:44:50Z"
        /// </summary>
        public static readonly string SarifDateTimeFormatSecondsPrecision = "yyyy'-'MM'-'dd'T'HH':'mm':'ss'Z'";

        /// <summary>
        /// Returns an ISO 8601 compatible universal date time format string with
        /// milliseconds precision, used to produce times such as "2016-03-02T01:44:50.123Z"
        public static readonly string SarifDateTimeFormatMillisecondsPrecision = "yyyy'-'MM'-'dd'T'HH':'mm':'ss.fff'Z'";

        private static string s_stableSarifVersion = null;

        public static string StableSarifVersion
        {
            get
            {
                if (s_stableSarifVersion == null)
                {
                    s_stableSarifVersion = VersionConstants.StableSarifVersion;
                }

                return s_stableSarifVersion;
            }
        }

        public static SarifVersion ConvertToSarifVersion(this string sarifVersionText)
        {
            if (sarifVersionText.Equals(StableSarifVersion, StringComparison.Ordinal))
            {
                return SarifVersion.Current;
            }
            else if (sarifVersionText.Equals(V1_0_0, StringComparison.Ordinal))
            {
                return SarifVersion.OneZeroZero;
            }

            return SarifVersion.Unknown;
        }

        public static string ConvertToText(this SarifVersion sarifVersion)
        {
            switch (sarifVersion)
            {
                case SarifVersion.OneZeroZero: { return V1_0_0; }
                case SarifVersion.Current: { return StableSarifVersion; }
            }
            return "unknown";
        }

        public static Uri ConvertToSchemaUri(this SarifVersion sarifVersion)
        {
            return new Uri(
                    SarifSchemaUriBase +
                    (sarifVersion == SarifVersion.Current ? VersionConstants.SchemaVersionAsPublishedToSchemaStoreOrg : sarifVersion.ConvertToText()) + ".json", UriKind.Absolute);
        }

        public static Dictionary<string, string> BuildMessageFormats(IEnumerable<string> resourceNames, ResourceManager resourceManager)
        {
            if (resourceNames == null)
            {
                throw new ArgumentNullException(nameof(resourceNames));
            }

            if (resourceManager == null)
            {
                throw new ArgumentNullException(nameof(resourceManager));
            }


            // Note this dictionary provides for case-insensitive keys
            var dictionary = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            foreach (string resourceName in resourceNames)
            {
                string resourceValue = resourceManager.GetString(resourceName);
                dictionary[resourceName] = resourceValue;
            }

            return dictionary;
        }

        public static string FormatMessage(this Exception exception)
        {
            if (exception == null)
            {
                throw new ArgumentNullException(nameof(exception));
            }

            // Retrieves a formatted message that includes exception type details, e.g.
            // System.InvalidOperationException: Operation is not valid due to the current state of the object.
            return exception.ToString().Split(new string[] { Environment.NewLine }, StringSplitOptions.None)[0];
        }

        public static void AddOrUpdateDictionaryEntry<TKey, TValue>(IDictionary<TKey, TValue> dictionary, TKey key, TValue val)
        {
            if (dictionary.ContainsKey(key))
            {
                dictionary[key] = val;
            }
            else
            {
                dictionary.Add(key, val);
            }
        }

        public static CodeFlow CreateSingleThreadedCodeFlow(IEnumerable<ThreadFlowLocation> locations = null)
        {
            return new CodeFlow
            {
                ThreadFlows = new List<ThreadFlow>()
                {
                    new ThreadFlow
                    {
                        Locations = new List<ThreadFlowLocation>(locations ?? new ThreadFlowLocation[]{ })
                    }
                }
            };
        }

        public static string GetUtf8Base64String(string s)
        {
            return GetBase64String(s, Encoding.UTF8);
        }

        public static string GetBase64String(string s, Encoding encoding)
        {
            byte[] bytes = encoding.GetBytes(s);
            return Convert.ToBase64String(bytes);
        }

        public static string DecodeBase64String(string s, Encoding encoding = null)
        {
            encoding = encoding ?? Encoding.UTF8;
            byte[] bytes = Convert.FromBase64String(s);
            return encoding.GetString(bytes);
        }

        public static int GetByteLength(char[] chars, Encoding encoding)
        {
            chars = chars ?? throw new ArgumentNullException(nameof(chars));
            encoding = encoding ?? throw new ArgumentNullException(nameof(encoding));

            string s = new string(chars);
            return GetByteLength(s, encoding);
        }

        public static int GetByteLength(string s, Encoding encoding)
        {
            s = s ?? throw new ArgumentNullException(nameof(s));
            encoding = encoding ?? throw new ArgumentNullException(nameof(encoding));

            byte[] bytes = encoding.GetBytes(s);
            return bytes.Length;
        }

        public static Regex RegexFromPattern(string pattern) =>
            new Regex(
                pattern,
                RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture | RegexOptions.IgnorePatternWhitespace);

        internal static bool UnitTesting = false;

        internal static void DebugAssert(bool conditional)
        {
            if (UnitTesting)
            {
                if (!conditional)
                {
                    throw new InvalidOperationException("Assert condition failed during unit test execution.");
                }
            }
            else
            {
                Debug.Assert(conditional);
            }
        }

        internal static Encoding GetEncodingFromName(string encodingName)
        {
            if (string.IsNullOrWhiteSpace(encodingName)) { return null; }

            try
            {
                return Encoding.GetEncoding(encodingName);
            }
            catch (ArgumentException)
            {
                return null;
            }
        }
    }
}
