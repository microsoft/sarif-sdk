// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using System;
using Microsoft.CodeAnalysis.Sarif.Readers;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.CodeAnalysis.Sarif.Visitors
{
    public class AbsoluteUrisVisitor : SarifRewritingVisitor
    {
        private Dictionary<string, Uri> uriMappings;

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
        public AbsoluteUrisVisitor()
        {
        }

        public override PhysicalLocation VisitPhysicalLocation(PhysicalLocation node)
        {
            PhysicalLocation newNode = base.VisitPhysicalLocation(node);
            
            if (!string.IsNullOrEmpty(newNode.UriBaseId) && uriMappings.ContainsKey(newNode.UriBaseId))
            {
                Uri baseUri = uriMappings[newNode.UriBaseId];
                newNode.Uri = new Uri(baseUri, newNode.Uri);
                newNode.UriBaseId = null;
            }

            return newNode;
        }

        public override Run VisitRun(Run node)
        {
            Run newRun;
            if (node.Properties != null)
            {
                // If the dictionary doesn't exist, we should add it to the properties.  If it does, we should add/update the existing dictionary.
                Dictionary<string, Uri> baseUriDictionary = new Dictionary<string, Uri>();
                if (node.Properties.ContainsKey(RebaseUriVisitor.BaseUriDictionaryName))
                {
                    if (!TryDeserializePropertyDictionary(node.Properties[RebaseUriVisitor.BaseUriDictionaryName], out uriMappings))
                    {
                        throw new InvalidOperationException("Base URI Dictionary incorrectly formatted");
                    }

                    newRun = base.VisitRun(node);

                    if (node.Files != null)
                    {
                        FixFiles(node);
                    }
                }
                else
                {
                    newRun = base.VisitRun(node);
                }
            }
            else
            {
                newRun = base.VisitRun(node);
            }
            return newRun;
        }

        /// <summary>
        /// If we are changing the URIs in Results to be relative, we need to also change the URI keys in the files dictionary
        /// to be relative.
        /// </summary>
        /// <param name="run">A run to fix the Files dictionary of.</param>
        private void FixFiles(Run run)
        {
            Dictionary<string, FileData> newDictionary = new Dictionary<string, FileData>();

            foreach (var key in run.Files.Keys)
            {
                Uri oldUri;

                FileData newNode = run.Files[key];

                Uri baseUri;
                // Node has a UriBaseId.
                if (!string.IsNullOrEmpty(newNode.UriBaseId) && uriMappings.ContainsKey(newNode.UriBaseId))
                {
                    baseUri = uriMappings[newNode.UriBaseId];
                    newNode.Uri = new Uri(baseUri, newNode.Uri);

                    Uri parentUri;
                    if (Uri.TryCreate(newNode.ParentKey, UriKind.Relative, out parentUri))
                    {
                        newNode.ParentKey = new Uri(baseUri, parentUri);
                    }

                    newNode.UriBaseId = null;
                }
                // fix dictionary
                throw new NotImplementedException("TODO")
            }

            run.Files = newDictionary;
        }

        private static bool TryDeserializePropertyDictionary(SerializedPropertyInfo serializedProperty, out Dictionary<string, Uri> dictionary)
        {
            try
            {
                dictionary = JsonConvert.DeserializeObject<Dictionary<string, Uri>>(serializedProperty.SerializedValue, _settings);

                return true;
            }
            // Didn't deserialize correctly
            catch (Exception)
            {
                dictionary = null;
                return false;
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
