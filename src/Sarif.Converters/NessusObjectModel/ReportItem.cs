// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Xml.Serialization;

namespace Microsoft.CodeAnalysis.Sarif.Converters.NessusObjectModel
{
    public class ReportItem
    {
        [XmlAttribute("severity")]
        public string Severity { get; set; } = string.Empty;

        [XmlAttribute("port")]
        public string Port { get; set; } = string.Empty;

        [XmlAttribute("pluginFamily")]
        public string PluginFamily { get; set; } = string.Empty;

        [XmlAttribute("pluginName")]
        public string PluginName { get; set; } = string.Empty;

        [XmlAttribute("pluginID")]
        public string PluginId { get; set; } = string.Empty;

        [XmlAttribute("protocol")]
        public string Protocol { get; set; } = string.Empty;

        [XmlAttribute("svc_name")]
        public string ServiceName { get; set; } = string.Empty;

        [XmlElement("plugin_modification_date")]
        public string PluginModificationDate { get; set; } = string.Empty;

        [XmlElement("plugin_publication_date")]
        public string PluginPublicationDate { get; set; } = string.Empty;

        [XmlElement("plugin_type")]
        public string PluginType { get; set; } = string.Empty;

        [XmlElement("solution")]
        public string Solution { get; set; } = string.Empty;

        [XmlElement("description")]
        public string Description { get; set; } = string.Empty;

        [XmlElement("synopsis")]
        public string Synopsis { get; set; } = string.Empty;

        [XmlElement("risk_factor")]
        public string RiskFactor { get; set; } = string.Empty;

        [XmlElement("script_version")]
        public string ScriptVersion { get; set; } = string.Empty;

        [XmlElement("plugin_output")]
        public string PluginOutput { get; set; } = string.Empty;

        [XmlElement("see_also")]
        public string SeeAlso { get; set; } = string.Empty;

        [XmlElement("cvss_base_score")]
        public string CvssBaseScore { get; set; } = string.Empty;

        [XmlElement("cvss_temporal_score")]
        public string CvssTemporalScore { get; set; } = string.Empty;

        [XmlElement("cvss3_base_score")]
        public string Cvss3BaseScore { get; set; } = string.Empty;

        [XmlElement("cvss3_temporal_score")]
        public string Cvss3TemporalScore { get; set; } = string.Empty;

        [XmlElement("exploit_available")]
        public string ExploitAvailable { get; set; } = string.Empty;

        [XmlElement("patch_publication_date")]
        public string PatchPublicationDate { get; set; } = string.Empty;

        [XmlElement("vuln_publication_date")]
        public string VulnPublicationDate { get; set; } = string.Empty;

        [XmlElement("cvss3_temporal_vector")]
        public string Cvss3TemporalVector { get; set; } = string.Empty;

        [XmlElement("cvss3_vector")]
        public string Cvss3Vector { get; set; } = string.Empty;

        [XmlElement("cvss_temporal_vector")]
        public string CvssTemporalVector { get; set; } = string.Empty;

        [XmlElement("cvss_vector")]
        public string CvssVector { get; set; } = string.Empty;

        [XmlElement("cve")]
        public List<string> Cves { get; set; } = new List<string>();

        [XmlElement("xref")]
        public List<string> Xrefs { get; set; } = new List<string>();
    }
}
