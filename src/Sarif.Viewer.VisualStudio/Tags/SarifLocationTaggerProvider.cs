// Copyright (c) Microsoft. All rights reserved. 
// Licensed under the MIT license. See LICENSE file in the project root for full license information. 

using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Operations;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;

using System;
using System.ComponentModel.Composition;

namespace Microsoft.Sarif.Viewer.Tags
{
    /// <summary>
    /// Tagger provider for highlighting the 'any' ContentType.
    /// </summary>
    /// <remarks>
    /// This is similar to the TextMarkerProviderFactory, except it applies to the 'any' ContentType.
    /// We can't use TextMarkerProviderFactory because it only applies to 'text' ContentTypes.
    /// HTML files are 'projection' types, which doesn't inherit from 'text'. So TextMarkerProviderFactory
    /// cannot highlight HTML file contents.
    /// </remarks>
    [Export(typeof(ISarifLocationProviderFactory))]
    [Export(typeof(IViewTaggerProvider))]
    [TagType(typeof(TextMarkerTag))]
    [ContentType("any")]
    internal class SarifLocationTaggerProvider : IViewTaggerProvider, ISarifLocationProviderFactory
    {
        public ITagger<T> CreateTagger<T>(ITextView textView, ITextBuffer buffer) where T : ITag
        {
            if (textView == null)
            {
                throw new ArgumentNullException("textView");
            }

            if (buffer == null)
            {
                throw new ArgumentNullException("buffer");
            }

            if (textView.TextBuffer != buffer)
            {
                return null;
            }

            return CreateSarifLocationTaggerInternal(buffer) as ITagger<T>;
        }

        public SimpleTagger<TextMarkerTag> GetTextMarkerTagger(ITextBuffer buffer)
        {
            if (buffer == null)
            {
                throw new ArgumentNullException("buffer");
            }

            return CreateSarifLocationTaggerInternal(buffer);
        }

        internal static SimpleTagger<TextMarkerTag> CreateSarifLocationTaggerInternal(ITextBuffer textBuffer)
        {
            SimpleTagger<TextMarkerTag> sarifLocationTagger = textBuffer.Properties.GetOrCreateSingletonProperty<SimpleTagger<TextMarkerTag>>(delegate
            {
                return new SimpleTagger<TextMarkerTag>(textBuffer);
            });

            return sarifLocationTagger;
        }
    }
}