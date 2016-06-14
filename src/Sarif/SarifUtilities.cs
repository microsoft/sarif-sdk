﻿// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Resources;
using System.Text.RegularExpressions;

namespace Microsoft.CodeAnalysis.Sarif
{
    public static class SarifUtilities
    {
        private static Regex s_semVer200 = new Regex(@"^(?<major>\d+)\.(?<minor>\d+)\.(?<patch>\d+)(-(?<prerelease>[A-Za-z0-9\-\.]+))?(\+(?<build>[A-Za-z0-9\-\.]+))?$", RegexOptions.Compiled);
        public static bool IsSemanticVersioningCompatible(this string versionText)
        {
            return s_semVer200.IsMatch(versionText);
        }

        private const string V1_0_0 = "1.0.0";
        private const string V1_0_0_BETA_5 = "1.0.0-beta.5";

        /// <summary>
        /// Returns an ISO 8601 compatible universal date time format string with
        /// seconds precision, used to produce times such as "2016-03-02T01:44:50Z"
        /// </summary>
        public static readonly string SarifDateTimeFormatSecondsPrecision = "yyyy'-'MM'-'dd'T'HH':'mm':'ss'Z'";

        /// <summary>
        /// Returns an ISO 8601 compatible universal date time format string with
        /// milliseconds precision, used to produce times such as "2016-03-02T01:44:50.123Z"
        public static readonly string SarifDateTimeFormatMillisecondsPrecision = "yyyy'-'MM'-'dd'T'HH':'mm':'ss.fff'Z'";


        public static SarifVersion ConvertToSarifVersion(this string sarifVersionText)
        {
            switch (sarifVersionText)
            {
                case V1_0_0_BETA_5: return SarifVersion.OneZeroZeroBetaFive;
                case V1_0_0: return SarifVersion.OneZeroZero;
            }

            return SarifVersion.Unknown;
        }

        public static string ConvertToText(this SarifVersion sarifVersion)
        {
            switch (sarifVersion)
            {
                case SarifVersion.OneZeroZeroBetaFive: { return V1_0_0_BETA_5; }
                case SarifVersion.OneZeroZero: { return V1_0_0; }
            }
            return "unknown";
        }

        public static Uri ConvertToSchemaUri(this SarifVersion sarifVersion)
        {
            return new Uri("http://json.schemastore.org/sarif-" + sarifVersion.ConvertToText(), UriKind.Absolute);
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

        public static void InitializeFromAssembly(this Tool tool, Assembly assembly)
        {
            InitializeFromAssembly(tool, assembly, prereleaseInfo: null);
        }

        public static void InitializeFromAssembly(this Tool tool, Assembly assembly, string prereleaseInfo)
        {
            if (assembly == null)
            {
                throw new ArgumentNullException(nameof(assembly));
            }

            if (tool == null)
            {
                throw new ArgumentNullException(nameof(tool));
            }

            string name = Path.GetFileNameWithoutExtension(assembly.Location);
            Version version = assembly.GetName().Version;

            tool.Name = name;
            tool.Version = version.Major.ToString(CultureInfo.InvariantCulture) + "." + version.Minor.ToString(CultureInfo.InvariantCulture) + "." + version.Build.ToString(CultureInfo.InvariantCulture);
            tool.FullName = name + " " + tool.Version + (prereleaseInfo ?? "");
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
    }
}
