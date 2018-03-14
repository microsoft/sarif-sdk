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
        internal Dictionary<string, Uri> _currentUriMappings;

        /// <summary>
        /// Create a RebaseUriVisitor, with a given name for the Base URI and a value for the base URI.
        /// </summary>
        public AbsoluteUrisVisitor()
        {
        }

        public override PhysicalLocation VisitPhysicalLocation(PhysicalLocation node)
        {
            PhysicalLocation newNode = base.VisitPhysicalLocation(node);
            
            if (_currentUriMappings != null && !string.IsNullOrEmpty(newNode.UriBaseId) && _currentUriMappings.ContainsKey(newNode.UriBaseId))
            {
                Uri baseUri = _currentUriMappings[newNode.UriBaseId];
                newNode.Uri = CombineUris(baseUri, newNode.Uri);
                newNode.UriBaseId = null;
            }

            return newNode;
        }

        public override Run VisitRun(Run node)
        {
            Run newRun;

            // Reset URI mappings for this run.
            _currentUriMappings = new Dictionary<string, Uri>();

            // Try to get the uri mappings dictionary out of the 
            if (node.Properties != null && node.Properties.ContainsKey(RebaseUriVisitor.BaseUriDictionaryName))
            {
                // For a given run, we'll reset the Uri Mappings while traversing it.
                if (!RebaseUriVisitor.TryDeserializePropertyDictionary(node.Properties[RebaseUriVisitor.BaseUriDictionaryName], out _currentUriMappings))
                {
                    throw new InvalidOperationException($"Base URI Dictionary incorrectly formatted, we expect a string->uri dictionary in the Run Properties with name {RebaseUriVisitor.BaseUriDictionaryName}");
                }
                    
                // If we don't have a dictionary we won't need to fix the files up.
                if (node.Files != null)
                {
                    FixFiles(node);
                }
            }
            
            newRun = base.VisitRun(node);
            
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
                FileData newNode = run.Files[key];

                Uri baseUri;
                // Node has a UriBaseId && we're going to rewrite it.
                if (!string.IsNullOrEmpty(newNode.UriBaseId) && _currentUriMappings.ContainsKey(newNode.UriBaseId))
                {
                    // Rewrite the filedata's URI
                    baseUri = _currentUriMappings[newNode.UriBaseId];
                    newNode.Uri = CombineUris(baseUri, newNode.Uri);                    

                    Uri parentUri;
                    // If the parent uri is relative, we should rewrite it as well.
                    if (Uri.TryCreate(newNode.ParentKey, UriKind.Relative, out parentUri))
                    {
                        newNode.ParentKey = CombineUris(baseUri, parentUri).ToString();
                    }

                    newNode.UriBaseId = null;
                }

                // fix dictionary
                newDictionary[newNode.Uri.ToString()] = newNode;
            }

            run.Files = newDictionary;
        }

        private static Uri CombineUris(Uri baseUri, Uri relativeUri)
        {
            Uri relativePart = relativeUri;
            if(relativeUri.OriginalString.StartsWith("/"))
            {
                relativePart = new Uri(relativeUri.ToString().TrimStart('/'), UriKind.Relative);
            }

            return new Uri(baseUri, relativePart);
        }
    }
}
