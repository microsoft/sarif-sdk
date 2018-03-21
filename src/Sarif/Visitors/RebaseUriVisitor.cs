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

        private string _baseName;
        private Uri _baseUri;

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
            _baseName = baseName;
            _baseUri = baseUri;
            Debug.Assert(_baseUri.IsAbsoluteUri);
        }

        public override PhysicalLocation VisitPhysicalLocation(PhysicalLocation node)
        {
            PhysicalLocation newNode = base.VisitPhysicalLocation(node);
            
            if (string.IsNullOrEmpty(newNode.UriBaseId))
            {
                if (newNode.Uri.IsAbsoluteUri && _baseUri.IsBaseOf(newNode.Uri))
                {
                    newNode.UriBaseId = _baseName;
                    newNode.Uri = _baseUri.MakeRelativeUri(node.Uri);
                }
            }

            return newNode;
        }

        public override Run VisitRun(Run node)
        {
            Run newRun = base.VisitRun(node);

            if (newRun.Files != null)
            {
                FixFiles(newRun);
            }
            
            if (node.Properties == null)
            {
                node.Properties = new Dictionary<string, SerializedPropertyInfo>();
            }

            // If the dictionary doesn't exist, we should add it to the properties.  If it does, we should add/update the existing dictionary.
            Dictionary<string, Uri> baseUriDictionary = new Dictionary<string, Uri>();
            if (node.Properties.ContainsKey(BaseUriDictionaryName))
            {
                if (!TryDeserializePropertyDictionary(node.Properties[BaseUriDictionaryName], out baseUriDictionary) || baseUriDictionary == null)
                {
                    // If for some reason we don't have a valid dictionary in the originalUriBaseIds, we move it to another location.
                    node.Properties[BaseUriDictionaryName + IncorrectlyFormattedDictionarySuffix] = node.Properties[BaseUriDictionaryName];
                    baseUriDictionary = new Dictionary<string, Uri>();
                }
            }
            
            // Note--this is an add or update, so if this is run twice with the same base variable, we'll replace the path.
            baseUriDictionary[_baseName] = _baseUri;
            
            newRun.Properties[BaseUriDictionaryName] = ReserializePropertyDictionary(baseUriDictionary);

            return newRun;
        }

        /// <summary>
        /// If we are changing the URIs in Results to be relative, we need to also change the URI keys in the files dictionary
        /// to be relative.
        /// 
        /// For FileData, we need to fix up the URI data (making it relative to the appropriate base address), 
        /// and also fix up the ParentKey (as we are patching the keys in the files dictionary in the Run).
        /// (We need to fix up the keys as we are patching the PhysicalLocation in the Result objects.)
        /// </summary>
        /// <param name="run">A run to fix the Files dictionary of.</param>
        internal void FixFiles(Run run)
        {
            Dictionary<string, FileData> newDictionary = new Dictionary<string, FileData>();

            foreach (var key in run.Files.Keys)
            {
                Uri oldUri;
                string newKey = key;
                FileData data = run.Files[key];
                // If the old uri is absolute and we need to rebase it, we should.
                if (Uri.TryCreate(key, UriKind.Absolute, out oldUri) && oldUri.IsAbsoluteUri && _baseUri.IsBaseOf(oldUri))
                {
                    Uri newUri = _baseUri.MakeRelativeUri(oldUri);

                    // Ensure the filedata reflects the correct base URI details.
                    if (data != null)
                    {
                        data.Uri = newUri;
                        data.UriBaseId = _baseName;

                        if (data.ParentKey != null)
                        {
                            Uri parentUri;
                            // If the parent URI is absolute and we need to rebase it, we should.
                            if (Uri.TryCreate(data.ParentKey, UriKind.Absolute, out parentUri) && parentUri.IsAbsoluteUri && _baseUri.IsBaseOf(parentUri))
                            {
                                data.ParentKey = _baseUri.MakeRelativeUri(new Uri(data.ParentKey)).ToString();
                            }
                        }
                    }

                    if (newDictionary.ContainsKey(newUri.ToString()))
                    {
                        throw new InvalidOperationException("Cannot rebase this file, as two URIs will collide in the file dictionary.");
                    }

                    newKey = newUri.ToString();
                }

                newDictionary[newKey] = data;
            }

            run.Files = newDictionary;
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
