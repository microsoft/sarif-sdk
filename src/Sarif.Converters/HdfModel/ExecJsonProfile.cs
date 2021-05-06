// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Globalization;

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Microsoft.CodeAnalysis.Sarif.Converters.HdfModel
{
    public partial class ExecJsonProfile
    {
        [JsonProperty("attributes", Required = Required.Always)]
        public List<Dictionary<string, object>> Attributes { get; set; }

        [JsonProperty("controls", Required = Required.Always)]
        public List<ExecJsonControl> Controls { get; set; }

        [JsonProperty("copyright", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public string Copyright { get; set; }

        [JsonProperty("copyright_email", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public string CopyrightEmail { get; set; }

        [JsonProperty("depends", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public List<Dependency> Depends { get; set; }

        [JsonProperty("description", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public string Description { get; set; }

        [JsonProperty("groups", Required = Required.Default)]
        public List<ControlGroup> Groups { get; set; }

        [JsonProperty("inspec_version", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public string InspecVersion { get; set; }

        [JsonProperty("license", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public string License { get; set; }

        [JsonProperty("maintainer", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public string Maintainer { get; set; }

        [JsonProperty("name", Required = Required.Always)]
        public string Name { get; set; }

        [JsonProperty("parent_profile", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public string ParentProfile { get; set; }

        [JsonProperty("sha256", Required = Required.Always)]
        public string Sha256 { get; set; }

        [JsonProperty("skip_message", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public string SkipMessage { get; set; }

        [JsonProperty("status", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public string Status { get; set; }

        [JsonProperty("summary", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public string Summary { get; set; }

        [JsonProperty("supports", Required = Required.Always)]
        public List<SupportedPlatform> Supports { get; set; }

        [JsonProperty("title", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public string Title { get; set; }

        [JsonProperty("version", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public string Version { get; set; }
    }
}
