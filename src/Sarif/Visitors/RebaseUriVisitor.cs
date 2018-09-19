// Copyright (c) Microsoft. All rights reserved. Licensed under the MIT        
// license. See LICENSE file in the project root for full license information. 

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.CodeAnalysis.Sarif.Readers;
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
        internal const string OriginalFileKey = "OriginalFileKey";

        private Uri _baseUri;
        private string _baseName;
        private bool _rebaseRelativeUris;
        IDictionary<string, FileData> _files;

        private static JsonSerializerSettings _settings;

        internal static JsonSerializerSettings JsonSerializerSettings
        {
            get
            {
                if (_settings == null)
                {
                    _settings = new JsonSerializerSettings();
                    _settings.ContractResolver = SarifContractResolver.Instance;
                }
                return _settings;
            }
        }
        
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

        public override FileData VisitFileData(FileData node)
        {
            FileData newNode = base.VisitFileData(node);

            RebaseFilesDictionary(newNode);

            return newNode;
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
            _files = node.Files;

            // Save the file key in the property bag so we can remap it during rebasing.
            foreach (string key in _files.Keys)
            {
                _files[key].SetProperty(OriginalFileKey, key);
            }

            Run newRun = base.VisitRun(node);

            // Remove the remapped files (they are duplicated during remapping).
            foreach (string key in _files.Keys.ToArray())
            {
                string originalFileKey = _files[key].GetProperty<string>(OriginalFileKey);

                if (originalFileKey != null)
                {
                    if (_files.ContainsKey(originalFileKey))
                    {
                        _files.Remove(originalFileKey);
                    }
                    else
                    {
                        _files[key].RemoveProperty(OriginalFileKey);

                        if (_files[key].Properties.Count == 0)
                        {
                            _files[key].Properties = null;
                        }
                    }
                }
            }

            newRun.Files = _files;

            // If the dictionary doesn't exist, we should add it to the properties.  If it does, we should add/update the existing dictionary.
            IDictionary<string, Uri> baseUriDictionary = new Dictionary<string, Uri>();
            if (node.OriginalUriBaseIds != null)
            {
                baseUriDictionary = node.OriginalUriBaseIds;
            }
            
            // Note--this is an add or update, so if this is run twice with the same base variable, we'll replace the path.
            baseUriDictionary[_baseName] = _baseUri;
            newRun.OriginalUriBaseIds = baseUriDictionary;

            return newRun;
        }

        /// <summary>
        /// If we are changing the URIs in Results to be relative, we need to also change the URI keys in the files dictionary
        /// to be relative.
        /// </summary>
        /// <param name="node">File location being changed to relative.</param>
        internal void RebaseFilesDictionary(FileData node)
        {
            _files = _files ?? new Dictionary<string, FileData>(StringComparer.OrdinalIgnoreCase);

            FileLocation fileLocation = node.FileLocation;

            string uriText = Uri.EscapeUriString(fileLocation.Uri.ToString());
            string uriTextOriginal = node.Properties[OriginalFileKey].SerializedValue;
            string uriTextOriginalWithBase = _baseUri + uriText;

            if (!string.IsNullOrEmpty(fileLocation.UriBaseId))
            {
                uriText = "#" + fileLocation.UriBaseId + "#" + uriText;
            }

            if (!_files.ContainsKey(uriText))
            {
                _files[uriText] = node;

                if (_files.ContainsKey(uriTextOriginal))
                {
                    _files.Remove(uriTextOriginal);
                }
                else if (_files.ContainsKey(uriTextOriginalWithBase))
                {
                    _files.Remove(uriTextOriginalWithBase);
                }
            }
        }

        internal static bool TryDeserializePropertyDictionary(SerializedPropertyInfo serializedProperty, out Dictionary<string, Uri> dictionary)
        {
            try
            {
                dictionary = JsonConvert.DeserializeObject<Dictionary<string, Uri>>(serializedProperty.SerializedValue, _settings);

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
            return JsonConvert.DeserializeObject<Dictionary<string, Uri>>(info.SerializedValue, _settings);
        }

        /// <summary>
        /// Internal as used in testing as a helper.
        /// </summary>
        internal static SerializedPropertyInfo ReserializePropertyDictionary(Dictionary<string, Uri> dictionary)
        {
            return new SerializedPropertyInfo(JsonConvert.SerializeObject(dictionary, _settings), false);
        }
    }
}
