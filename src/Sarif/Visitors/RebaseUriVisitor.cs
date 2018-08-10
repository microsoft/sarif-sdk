// Copyright (c) Microsoft. All rights reserved. Licensed under the MIT        
// license. See LICENSE file in the project root for full license information. 

using System;
using System.Collections.Generic;
using System.Diagnostics;
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

        public override PhysicalLocation VisitPhysicalLocation(PhysicalLocation node)
        {
            PhysicalLocation newNode = base.VisitPhysicalLocation(node);
            
            if (string.IsNullOrEmpty(newNode.FileLocation?.UriBaseId))
            {
                if (newNode.FileLocation.Uri.IsAbsoluteUri && _baseUri.IsBaseOf(newNode.FileLocation.Uri))
                {
                    newNode.FileLocation.UriBaseId = _baseName;
                    newNode.FileLocation.Uri = _baseUri.MakeRelativeUri(node.FileLocation.Uri);
                    RebaseFilesDictionary(newNode);
                }
                else if (_rebaseRelativeUris && !newNode.FileLocation.Uri.IsAbsoluteUri)
                {
                    newNode.FileLocation.UriBaseId = _baseName;
                    RebaseFilesDictionary(newNode);
                }
            }

            return newNode;
        }

        public override Run VisitRun(Run node)
        {
            _files = node.Files;

            Run newRun = base.VisitRun(node);

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
        /// <param name="node">Result location being changed to relative.</param>
        internal void RebaseFilesDictionary(PhysicalLocation node)
        {
            _files = _files ?? new Dictionary<string, FileData>(StringComparer.OrdinalIgnoreCase);

            FileLocation fileLocation = node.FileLocation;

            string uriText = Uri.EscapeUriString(fileLocation.Uri.ToString());
            string uriTextOriginal = uriText;
            string uriTextOriginalWithBase = _baseUri + uriText;

            if (!string.IsNullOrEmpty(fileLocation.UriBaseId))
            {
                uriText = "#" + fileLocation.UriBaseId + "#" + uriText;
            }

            if (!_files.ContainsKey(uriText))
            {
                string mimeType = Writers.MimeType.DetermineFromFileExtension(uriText);

                if (_files.ContainsKey(uriTextOriginal))
                {
                    _files[uriText] = _files[uriTextOriginal];
                    _files.Remove(uriTextOriginal);
                }
                else if (_files.ContainsKey(uriTextOriginalWithBase))
                {
                    _files[uriText] = _files[uriTextOriginalWithBase];
                    _files.Remove(uriTextOriginalWithBase);
                }
                else
                {
                    _files[uriText] = new FileData()
                    {
                        MimeType = mimeType
                    };
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
