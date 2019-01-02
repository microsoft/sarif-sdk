// Copyright (c) Microsoft.  All Rights Reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Linq;

namespace Microsoft.CodeAnalysis.Sarif
{
    /// <summary>
    /// Rewriting visitor for the Sarif object model.
    /// </summary>
    public abstract partial class SarifRewritingVisitor
    {
        // NOTE: this partial class is intended to alleviate some manual work that repeatedly needs to
        //       be done when updating the schema (as our current code emit doesn't handle all 
        //       constructs required by this class. At some point, either the code emit will be
        //       updated or alternately we will have eliminated the need to visit dictionaries in 
        //       the format, and this partial class can be deleted again.
        private T VisitNullChecked<T>(T node, ref string key) where T : class, ISarifNode
        {
            if (node == null)
            {
                return null;
            }

            if (key == null)
            {
                return (T)Visit(node);
            }

            return (T)VisitDictionaryEntry(node, ref key);
        }

        private ISarifNode VisitDictionaryEntry(ISarifNode node, ref string key)
        {
            if (node == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            switch (node.SarifNodeKind)
            {
                case SarifNodeKind.FileData:
                    return VisitFileDataDictionaryEntry((FileData)node, ref key);

                // add other dictionary things

                default:
                    throw new InvalidOperationException(); // whoops! unknown type
            }
        }

        public virtual FileData VisitFileDataDictionaryEntry(FileData node, ref string key)
        {
            return (FileData)Visit(node);
        }

        public virtual Resources VisitResources(Resources node)
        {
            if (node != null)
            {
                if (node.Rules != null)
                {
                    var keys = node.Rules.Keys.ToArray();
                    foreach (var key in keys)
                    {
                        var value = node.Rules[key];

                        if (value != null)
                        {
                            node.Rules[key] = VisitNullChecked(value);
                        }
                    }
                }
            }

            return node;
        }
    }
}
