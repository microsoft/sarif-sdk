// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Xunit;

namespace Microsoft.CodeAnalysis.Sarif.Converters.UnitTests
{
    public class FortifyUtilitiesTests
    {
        [Fact]
        public void FortifyUtilities_ParseFormattedContentText_Correct()
        {
            string content = @"<Content><Paragraph><b>General relativity</b> is the <pre>geometric theory of gravitation</pre> published by <i>Albert Einstein</i> in 1915 and the current description of <IfDef var=""ConditionalDescriptions""><ConditionalText attr=""value"">gravitation in modern physics</ConditionalText></IfDef>.<AltParagraph>General relativity generalizes special relativity and <Replace key=""Scientist.lastName""/>'s law of universal gravitation, providing a unified description of gravity as a geometric property of space and time, or spacetime. In particular, the curvature of spacetime is directly related to the energy and momentum of whatever matter and radiation are present.</AltParagraph>The relation is specified by the <code>Einstein field equations</code>, a system of <code>partial differential equations</code>.";
            content += "\r\nAnd here's a garbage\n\r\n \n  \n\ndouble line break!</Paragraph></Content>";
            string expected = @"**General relativity** is the `geometric theory of gravitation` published by _Albert Einstein_ in 1915 and the current description of gravitation in modern physics.
General relativity generalizes special relativity and {Scientist.lastName}'s law of universal gravitation, providing a unified description of gravity as a geometric property of space and time, or spacetime. In particular, the curvature of spacetime is directly related to the energy and momentum of whatever matter and radiation are present.
The relation is specified by the `Einstein field equations`, a system of `partial differential equations`.
And here's a garbage

double line break!";
            string actual = FortifyUtilities.ParseFormattedContentText(content);
            Assert.Equal(expected, actual);
        }
    }
}
