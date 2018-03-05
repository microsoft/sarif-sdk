// Copyright (c) Microsoft. All rights reserved. Licensed under the MIT        
// license. See LICENSE file in the project root for full license information. 

using Microsoft.CodeAnalysis.Sarif.Readers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.CodeAnalysis.Sarif.Visitors
{
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
                if(_settings == null)
                {
                    _settings = new JsonSerializerSettings();
                    _settings.ContractResolver = SarifContractResolver.Instance;
                }
                return _settings;
            }
        }

        
        public RebaseUriVisitor(string baseName, Uri baseUri)
        {
            _baseName = baseName;
            _baseUri = baseUri;
        }

        public override PhysicalLocation VisitPhysicalLocation(PhysicalLocation node)
        {
            PhysicalLocation newNode = base.VisitPhysicalLocation(node);
            
            if(string.IsNullOrEmpty(newNode.UriBaseId))
            {
                if (_baseUri.IsBaseOf(newNode.Uri))
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

            if(node.Properties == null)
            {
                node.Properties = new Dictionary<string, SerializedPropertyInfo>();
            }

            Dictionary<string, Uri> baseUriDictionary = new Dictionary<string, Uri>();
            if (node.Properties.ContainsKey(BaseUriDictionaryName))
            {
                if(!TryDeserializePropertyDictionary(node.Properties[BaseUriDictionaryName], out baseUriDictionary))
                {
                    node.Properties[BaseUriDictionaryName + IncorrectlyFormattedDictionarySuffix] = node.Properties[BaseUriDictionaryName];
                    baseUriDictionary = new Dictionary<string, Uri>();
                }
            }
            
            baseUriDictionary.Add(_baseName, _baseUri);

            newRun.Properties[BaseUriDictionaryName] = ReserializePropertyDictionary(baseUriDictionary);

            return newRun;
        }

        private static bool TryDeserializePropertyDictionary(SerializedPropertyInfo serializedProperty, out Dictionary<string, Uri> dictionary)
        {
            try
            {

                dictionary = JsonConvert.DeserializeObject<Dictionary<string, Uri>>(serializedProperty.SerializedValue, _settings);

                return true;
            }
            catch (ArgumentException)
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
