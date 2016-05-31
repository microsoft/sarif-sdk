// Copyright (c) Microsoft. All rights reserved. 
// Licensed under the MIT license. See LICENSE file in the project root for full license information. 

using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Tagging;

namespace Microsoft.Sarif.Viewer.Tags
{
    public interface ISarifLocationProviderFactory
    {
        SimpleTagger<TextMarkerTag> GetTextMarkerTagger(ITextBuffer textBuffer);
    }
}
