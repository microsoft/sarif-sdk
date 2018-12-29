﻿// Copyright (c) Microsoft. All rights reserved. Licensed under the MIT        
// license. See LICENSE file in the project root for full license information. 

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Newtonsoft.Json;

namespace Microsoft.CodeAnalysis.Sarif.Visitors
{
    /// <summary>
    /// A class that, given a variable name (e.x. "%SRCROOT%") and a value (e.x. "C:\src\root\"), rebases the URIs in a SARIF log 
    /// in order to make a SARIF log not depend on absolute paths/be machine independent.
    /// </summary>
    public class RebaseUriVisitor : SarifRewritingVisitor
    {
        internal const string BaseUriDictionaryName = "originalUriBaseIds";
        internal const string IncorrectlyFormattedDictionarySuffix = ".Old";

        private Uri _baseUri;
        private string _baseName;
        private bool _rebaseRelativeUris;

        /// <summary>
        /// Create a RebaseUriVisitor, with a given name for the Base URI and a value for the base URI.
        /// </summary>
        public RebaseUriVisitor(string baseName, Uri baseUri)
        {
            _baseUri = baseUri;
            _baseName = baseName;
            _rebaseRelativeUris = false;

            Debug.Assert(_baseUri.IsAbsoluteUri);
        }

        /// <summary>
        /// Create a RebaseUriVisitor, with a given name for the Base URI and a value for the base URI.
        /// </summary>
        public RebaseUriVisitor(string baseName, bool rebaseRelativeUris, Uri baseUri)
        {
            _baseUri = baseUri;
            _baseName = baseName;
            Debug.Assert(_baseUri.IsAbsoluteUri);

            _rebaseRelativeUris = rebaseRelativeUris;
        }

        public override FileLocation VisitFileLocation(FileLocation node)
        {
            FileLocation newNode = base.VisitFileLocation(node);

            if (newNode.Uri.IsAbsoluteUri && _baseUri.IsBaseOf(newNode.Uri))
            {
                newNode.UriBaseId = _baseName;
                newNode.Uri = _baseUri.MakeRelativeUri(node.Uri);
            }
            else if (_rebaseRelativeUris && !newNode.Uri.IsAbsoluteUri)
            {
                newNode.UriBaseId = _baseName;
            }

            return newNode;
        }

        public override Run VisitRun(Run node)
        {
            Run newRun = base.VisitRun(node);

            // If the dictionary doesn't exist, we should add it to the properties.  If it does, we should add/update the existing dictionary.
            IDictionary<string, FileLocation> baseUriDictionary = new Dictionary<string, FileLocation>();
            if (node.OriginalUriBaseIds != null)
            {
                baseUriDictionary = node.OriginalUriBaseIds;
            }
            
            // Note--this is an add or update, so if this is run twice with the same base variable, we'll replace the path.
            baseUriDictionary[_baseName] = new FileLocation { Uri =_baseUri };
            newRun.OriginalUriBaseIds = baseUriDictionary;

            return newRun;
        }

        /// <summary>
        /// If we are changing the URIs in Results to be relative, we need to also change the URI keys in the files dictionary
        /// to be relative.
        /// </summary>
        /// <param name="node">File location being changed to relative.</param>
        public override FileData VisitFileDataDictionaryEntry(FileData node, ref string key)
        {
            string originalKey = key;
            
            // Force a visit of the file data object, which may rewrite its file location
            node = base.VisitFileDataDictionaryEntry(node, ref key);

            FileLocation fileLocation = node.FileLocation;

            if (fileLocation != null && !string.IsNullOrEmpty(fileLocation.UriBaseId))
            {
                string uriText = Uri.EscapeUriString(fileLocation.Uri.ToString());
                key = "#" + fileLocation.UriBaseId + "#" + uriText;
            }
            else
            {
                // In the event that FileData.FileLocation.UriBaseId is not populated, 
                // we'll simply transform the key on the basis of visitor configuration
                if (key.StartsWith(_baseUri.OriginalString, StringComparison.Ordinal))
                {
                    key = "#" + _baseName + "#" + key.Substring(_baseUri.OriginalString.Length);
                }
            }

            if (node.ParentKey != null &&
                node.ParentKey.StartsWith(_baseUri.OriginalString, StringComparison.Ordinal))
            {
                Debug.Assert(key != originalKey);
                node.ParentKey = "#" + _baseName + "#" + node.ParentKey.Substring(_baseUri.OriginalString.Length);
            }

            return node;
        }

        internal static bool TryDeserializePropertyDictionary(SerializedPropertyInfo serializedProperty, out Dictionary<string, Uri> dictionary)
        {
            try
            {
                dictionary = JsonConvert.DeserializeObject<Dictionary<string, Uri>>(serializedProperty.SerializedValue);

                return true;
            }
            // Didn't deserialize correctly
            catch (Exception ex)
            {
                if(ex is JsonSerializationException || ex is ArgumentNullException)
                {
                    dictionary = null;
                    return false;
                }
                throw;
            }
        }
        
        /// <summary>
        /// Internal as used in testing as a helper.
        /// </summary>
        internal static Dictionary<string, Uri> DeserializePropertyDictionary(SerializedPropertyInfo info)
        {
            return JsonConvert.DeserializeObject<Dictionary<string, Uri>>(info.SerializedValue);
        }

        /// <summary>
        /// Internal as used in testing as a helper.
        /// </summary>
        internal static SerializedPropertyInfo ReserializePropertyDictionary(Dictionary<string, Uri> dictionary)
        {
            return new SerializedPropertyInfo(JsonConvert.SerializeObject(dictionary), false);
        }
    }
}
