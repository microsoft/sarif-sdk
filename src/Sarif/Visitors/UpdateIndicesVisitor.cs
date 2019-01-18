// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;

namespace Microsoft.CodeAnalysis.Sarif.Visitors
{
    public class UpdateIndicesVisitor : SarifRewritingVisitor
    {
        private IDictionary<string, int> _fullyQualifiedLogicalNameToIndexMap;
        private IDictionary<string, int> _fileLocationKeyToIndexMap;
        private IDictionary<string, int> _ruleKeyToIndexMap;
        private Resources _resources;

        public UpdateIndicesVisitor(
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
                if (_ruleKeyToIndexMap.TryGetValue(node.RuleId, out int ruleIndex))
                {
                    node.RuleIndex = ruleIndex;

                    // We need to update the rule id, as it previously referred to a synthesized 
                    // key that resolved some collision in the resources.rules collection.
                    node.RuleId = _resources.Rules[ruleIndex].Id;
                }
            }

            return base.VisitResult(node);
        }

        public override Run VisitRun(Run node)
        {
            _resources = node.Resources;
            return base.VisitRun(node);
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