// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;

namespace Microsoft.CodeAnalysis.Sarif.Visitors
{
    /// <summary>
    /// Utility class that helps provide current SARIF v2 rule, file and logical location
    /// index data, when transform SARIF v1 or SARIF v2 prerelease data. 
    /// </summary>
    public class UpdateIndicesFromLegacyDataVisitor : SarifRewritingVisitor
    {
        private readonly IDictionary<string, int> _fullyQualifiedLogicalNameToIndexMap;
        private readonly IDictionary<string, int> _fileLocationKeyToIndexMap;
        private readonly IDictionary<string, int> _ruleKeyToIndexMap;

        private Tool _tool;

        public UpdateIndicesFromLegacyDataVisitor(
            IDictionary<string, int> fullyQualifiedLogicalNameToIndexMap, 
            IDictionary<string, int> fileLocationKeyToIndexMap,
            IDictionary<string, int> ruleKeyToIndexMap)
        {
            _fullyQualifiedLogicalNameToIndexMap = fullyQualifiedLogicalNameToIndexMap;
            _fileLocationKeyToIndexMap = fileLocationKeyToIndexMap;
            _ruleKeyToIndexMap = ruleKeyToIndexMap;
        }

        public override Result VisitResult(Result node)
        {
            if (_ruleKeyToIndexMap != null)
            {
                if (node.RuleId != null &&_ruleKeyToIndexMap.TryGetValue(node.RuleId, out int ruleIndex))
                {
                    node.RuleIndex = ruleIndex;

                    // We need to update the rule id, as it previously referred to a synthesized 
                    // key that resolved some collision in the resources.rules collection.
                    node.RuleId = _tool.RulesMetadata[ruleIndex].Id;
                }
            }

            return base.VisitResult(node);
        }

        public override Tool VisitTool(Tool node)
        {
            _tool = node;
            return base.VisitTool(node);
        }

        public override Location VisitLocation(Location node)
        {
            if (_fullyQualifiedLogicalNameToIndexMap != null && !string.IsNullOrEmpty(node.FullyQualifiedLogicalName))
            {
                if (_fullyQualifiedLogicalNameToIndexMap.TryGetValue(node.FullyQualifiedLogicalName, out int index))
                {
                    node.LogicalLocationIndex = index;
                }
            }

            return base.VisitLocation(node);
        }

        public override FileLocation VisitFileLocation(FileLocation node)
        {

            if (_fileLocationKeyToIndexMap != null)
            {
                string key = node.Uri.OriginalString;

                string uriBaseId = node.UriBaseId;
                if (!string.IsNullOrEmpty(uriBaseId))
                {
                    key = "#" + uriBaseId + "#" + key;
                }

                if (_fileLocationKeyToIndexMap.TryGetValue(key, out int index))
                {
                    var fileLocation = FileLocation.CreateFromFilesDictionaryKey(key);
                    node.Uri = new Uri(UriHelper.MakeValidUri(fileLocation.Uri.OriginalString), UriKind.RelativeOrAbsolute);
                    node.UriBaseId = fileLocation.UriBaseId;
                    node.FileIndex = index;
                }
            }

            return base.VisitFileLocation(node);
        }
    }
}